using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Synaptrix.Generator.Tests;

/// <summary>
/// Regression tests for a handler shape that's harder to catch than a simple arity check:
/// an open-generic handler whose type parameters DO match the interface's arity, but one
/// interface slot is a constructed type wrapping the handler's own type parameters instead
/// of a direct reference to them, e.g.
/// <c>Handler&lt;TId, TEntity&gt; : IStreamRequestHandler&lt;Command&lt;TId, TEntity&gt;, TEntity&gt;</c>.
/// Both sides have arity 2, so the older arity-only guard let this through, but .NET's
/// open-generic DI resolution binds type-parameter lists purely positionally: closing
/// <c>IFoo&lt;A, B&gt;</c> and asking for it constructs <c>Impl&lt;A, B&gt;</c> by substituting
/// A, B directly into Impl's own type-parameter slots. Here slot 0 is
/// <c>Command&lt;TId, TEntity&gt;</c>, not <c>TId</c> itself, so a request for
/// <c>IFoo&lt;Command&lt;string, Product&gt;, Product&gt;</c> would try to build
/// <c>Impl&lt;Command&lt;string, Product&gt;, Product&gt;</c> - substituting TId with
/// <c>Command&lt;string, Product&gt;</c>, which fails Impl's own generic constraints and
/// resolves to nothing. No startup crash (arity matches, so BuildServiceProvider doesn't
/// throw) - it just silently fails to resolve the handler the first time it's actually
/// dispatched at runtime.
/// </summary>
public class WrappedSlotOpenGenericTests
{
    private const string WrappedSlotHandlerSource = @"
using Synaptrix;
using Synaptrix.Attributes;
using System.Collections.Generic;

[assembly: DiscoverSynaptrixHandlers]

namespace MyTestApp.Entities
{
    public interface IEntity<TId, TSelf> { }

    public class FindEntitiesCommand<TId, TEntity> : IStreamRequest<TEntity>
        where TId : System.IEquatable<TId>
        where TEntity : class, IEntity<TId, TEntity>
    { }

    public class FindEntitiesCommandHandler<TId, TEntity> : IStreamRequestHandler<FindEntitiesCommand<TId, TEntity>, TEntity>
        where TId : System.IEquatable<TId>
        where TEntity : class, IEntity<TId, TEntity>
    {
        public IAsyncEnumerable<TEntity> Handle(FindEntitiesCommand<TId, TEntity> request, System.Threading.CancellationToken cancellationToken)
            => throw new System.NotImplementedException();
    }
}";

    [Fact]
    public void WrappedSlotHandler_DoesNotEmit_InvalidOpenGenericRegistration()
    {
        var (_, generatedSource) = GeneratorTestHelper.RunGenerator(WrappedSlotHandlerSource);

        Assert.NotEmpty(generatedSource);
        // The shape that resolves to nothing at runtime despite matching arity: registering
        // Handler<,> against IFoo<,> when slot 0 isn't Handler's own first type parameter.
        Assert.DoesNotContain(
            "services.AddTransient(typeof(global::Synaptrix.IStreamRequestHandler<,>), typeof(global::MyTestApp.Entities.FindEntitiesCommandHandler<,>));",
            generatedSource);
    }

    [Fact]
    public void WrappedSlotHandler_EmitsSkipComment_ExplainingWhy()
    {
        var (_, generatedSource) = GeneratorTestHelper.RunGenerator(WrappedSlotHandlerSource);

        Assert.Contains("Skipped", generatedSource);
        Assert.Contains("FindEntitiesCommandHandler", generatedSource);
        Assert.Contains("don't map directly", generatedSource);
        // This is NOT an arity mismatch - both sides have 2 slots - make sure the message says so.
        Assert.DoesNotContain("arity mismatch", generatedSource);
    }

    [Fact]
    public void WrappedSlotHandler_GeneratesCompilableCode()
    {
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorAndCompile(WrappedSlotHandlerSource);

        var errors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.True(
            errors.Length == 0,
            "Generated source produced compile errors:\n" + string.Join("\n", errors.Select(e => e.ToString())));
    }

    [Fact]
    public void WrappedSlot_AppliesUniformly_ToIRequestHandlerToo()
    {
        // Confirms the positional check isn't special-cased to IStreamRequestHandler: the same
        // wrapped-slot shape on the (non-stream) IRequestHandler<,> interface must be caught too.
        const string source = @"
using Synaptrix;
using Synaptrix.Attributes;

[assembly: DiscoverSynaptrixHandlers]

namespace MyTestApp.WrappedRequestSlot
{
    public class MyRequest<TResponse> : IRequest<TResponse> { }

    public class MyRequestHandler<TResponse> : IRequestHandler<MyRequest<TResponse>, TResponse>
    {
        public System.Threading.Tasks.ValueTask<TResponse> Handle(MyRequest<TResponse> request, System.Threading.CancellationToken cancellationToken)
            => throw new System.NotImplementedException();
    }
}";

        var (_, generatedSource) = GeneratorTestHelper.RunGenerator(source);

        // MyRequest<TResponse> wraps TResponse in slot 0 too, so this is ALSO an invalid open-generic
        // shape by the same rule - confirms the check applies consistently to IRequestHandler, not
        // just IStreamRequestHandler, and isn't specific to the FindEntitiesCommand case.
        Assert.Contains("Skipped", generatedSource);
        Assert.DoesNotContain(
            "services.AddTransient(typeof(global::Synaptrix.IRequestHandler<,>), typeof(global::MyTestApp.WrappedRequestSlot.MyRequestHandler<>));",
            generatedSource);
    }
}
