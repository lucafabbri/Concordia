using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Synaptrix.Generator.Tests;

/// <summary>
/// Regression tests for a handler shape that's valid conceptually but not auto-registrable:
/// an open-generic handler whose single type parameter fills two slots on the
/// interface it implements, e.g. <c>Handler&lt;T&gt; : IStreamRequestHandler&lt;Command&lt;T&gt;, T&gt;</c>.
/// The handler class has arity 1; the interface has arity 2. .NET's open-generic DI
/// mapping (<c>services.AddTransient(typeof(IFoo&lt;,&gt;), typeof(Impl&lt;&gt;))</c>) binds type
/// parameter lists positionally and requires matching arity, so registering this shape
/// naively throws <c>ArgumentException</c> ("Arity of open generic service type does not
/// equal arity of open generic implementation type") out of <c>BuildServiceProvider()</c>,
/// which fails the whole DI container - not just this one handler.
/// </summary>
public class AsymmetricArityOpenGenericTests
{
    private const string AsymmetricArityHandlerSource = @"
using Synaptrix;
using Synaptrix.Attributes;

[assembly: DiscoverSynaptrixHandlers]

namespace MyTestApp.Tabular
{
    public interface ITabular { }

    public class FetchTabularCommand<TTabular> : IStreamRequest<TTabular> { }

    public class FetchTabularCommandHandler<TTabular> : IStreamRequestHandler<FetchTabularCommand<TTabular>, TTabular>
        where TTabular : class, ITabular
    {
        public System.Collections.Generic.IAsyncEnumerable<TTabular> Handle(FetchTabularCommand<TTabular> request, System.Threading.CancellationToken cancellationToken)
            => throw new System.NotImplementedException();
    }
}";

    [Fact]
    public void AsymmetricArityHandler_DoesNotEmit_MismatchedOpenGenericRegistration()
    {
        var (_, generatedSource) = GeneratorTestHelper.RunGenerator(AsymmetricArityHandlerSource);

        Assert.NotEmpty(generatedSource);
        // The crashing shape we're guarding against: registering a 1-arity implementation
        // against a 2-arity service type via the naive typeof(...) open-generic overload.
        Assert.DoesNotContain(
            "services.AddTransient(typeof(global::Synaptrix.IStreamRequestHandler<,>), typeof(global::MyTestApp.Tabular.FetchTabularCommandHandler<>));",
            generatedSource);
    }

    [Fact]
    public void AsymmetricArityHandler_EmitsSkipComment_ExplainingWhy()
    {
        var (_, generatedSource) = GeneratorTestHelper.RunGenerator(AsymmetricArityHandlerSource);

        Assert.Contains("Skipped", generatedSource);
        Assert.Contains("FetchTabularCommandHandler", generatedSource);
        Assert.Contains("arity mismatch", generatedSource);
    }

    [Fact]
    public void AsymmetricArityHandler_GeneratesCompilableCode()
    {
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorAndCompile(AsymmetricArityHandlerSource);

        var errors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.True(
            errors.Length == 0,
            "Generated source produced compile errors:\n" + string.Join("\n", errors.Select(e => e.ToString())));
    }

    [Fact]
    public void SymmetricArityHandler_StillRegistersNormally()
    {
        // Sanity check: a handler whose arity DOES match its interface (the common case,
        // e.g. IPipelineBehavior<TRequest, TResponse>) must keep registering as before -
        // the arity guard must not over-trigger on the normal, symmetric shape.
        const string source = @"
using Synaptrix;
using Synaptrix.Attributes;

[assembly: DiscoverSynaptrixHandlers]

namespace MyTestApp.Symmetric
{
    public class MyRequest : IRequest<string> { }

    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public System.Threading.Tasks.ValueTask<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            System.Threading.CancellationToken cancellationToken)
            => next(cancellationToken);
    }
}";

        var (_, generatedSource) = GeneratorTestHelper.RunGenerator(source);

        Assert.Contains(
            "services.AddTransient(typeof(global::Synaptrix.IPipelineBehavior<,>), typeof(global::MyTestApp.Symmetric.LoggingBehavior<,>));",
            generatedSource);
        Assert.DoesNotContain("Skipped", generatedSource);
    }
}
