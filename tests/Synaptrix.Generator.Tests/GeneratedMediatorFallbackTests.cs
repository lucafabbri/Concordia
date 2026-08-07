using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Synaptrix.Generator.Tests;

/// <summary>
/// Regression tests for <c>GeneratedMediator</c>'s fallback path. Its dispatch methods are
/// built from a compile-time-known switch over the request/notification types this project's
/// own generator run discovered - two situations put a request outside that switch even
/// though a handler for it genuinely exists in the DI container: a handler shape the generator
/// can't safely fast-path (see the wrapped-slot/asymmetric-arity tests) that was registered by
/// hand, or a handler discovered by a *different* referenced assembly's own generator run
/// (cross-assembly discovery composes handler registrations into the same IServiceCollection,
/// but each assembly's own GeneratedMediator only has switch cases for the handlers *it* saw).
/// Previously, falling outside the switch meant an immediate exception regardless of whether
/// a handler was actually resolvable - and since cross-assembly discovery also rebinds
/// IMediator/ISender to whichever assembly's GeneratedMediator registers last, that assembly's
/// necessarily-incomplete switch became the sole determinant of what the whole app could
/// dispatch. GeneratedMediator now falls back to the reflection/DI-based Mediator instead of
/// throwing outright, so anything registered anywhere in the container remains reachable
/// through IMediator no matter which assembly's GeneratedMediator ends up bound to it.
/// </summary>
public class GeneratedMediatorFallbackTests
{
    private const string SingleHandlerSource = @"
using Synaptrix;
using Synaptrix.Attributes;

[assembly: DiscoverSynaptrixHandlers]

namespace MyTestApp.Fallback
{
    public class KnownRequest : IRequest<int> { }

    public class KnownRequestHandler : IRequestHandler<KnownRequest, int>
    {
        public System.Threading.Tasks.ValueTask<int> Handle(KnownRequest request, System.Threading.CancellationToken cancellationToken)
            => throw new System.NotImplementedException();
    }
}";

    private static string GetMediatorSource(string source)
    {
        var (_, sources) = GeneratorTestHelper.RunGeneratorAndCompile(source);
        var mediatorSource = sources.FirstOrDefault(s => s.Contains("class GeneratedMediator"));
        Assert.False(string.IsNullOrEmpty(mediatorSource), "No GeneratedMediator source was produced.");
        return mediatorSource!;
    }

    [Fact]
    public void GeneratedMediator_DeclaresLazyFallbackField()
    {
        var mediatorSource = GetMediatorSource(SingleHandlerSource);

        Assert.Contains("global::Synaptrix.Mediator? _fallback;", mediatorSource);
        Assert.Contains("__Fallback => _fallback ??= new global::Synaptrix.Mediator(_sp);", mediatorSource);
    }

    [Fact]
    public void GeneratedMediator_DelegatesToFallback_InsteadOfThrowingDirectly()
    {
        var mediatorSource = GetMediatorSource(SingleHandlerSource);

        // None of the dispatch methods should end in an unconditional throw anymore - every
        // terminal branch defers to the fallback mediator, which only throws once it too has
        // failed to resolve a handler from the DI container.
        Assert.DoesNotContain("throw new global::System.InvalidOperationException($\"No handler registered", mediatorSource);
        Assert.DoesNotContain("throw new global::System.InvalidOperationException($\"No stream handler registered", mediatorSource);

        Assert.Contains("return await __Fallback.Send(request, cancellationToken).ConfigureAwait(false);", mediatorSource);
        Assert.Contains("await __Fallback.Send(request, cancellationToken).ConfigureAwait(false);", mediatorSource);
        Assert.Contains("return __Fallback.Publish(notification, cancellationToken);", mediatorSource);
        Assert.Contains("return __Fallback.CreateStream(request, cancellationToken);", mediatorSource);
    }

    [Fact]
    public void GeneratedMediator_WithFallback_GeneratesCompilableCode()
    {
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorAndCompile(SingleHandlerSource);

        var errors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToArray();

        Assert.True(
            errors.Length == 0,
            "Generated source produced compile errors:\n" + string.Join("\n", errors.Select(e => e.ToString())));
    }
}
