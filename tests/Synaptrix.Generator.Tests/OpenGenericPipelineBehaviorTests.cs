using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Synaptrix.Generator.Tests;

public class OpenGenericPipelineBehaviorTests
{
    private const string OpenGenericBehaviorSource = @"
using Synaptrix;
using Synaptrix.Attributes;

[assembly: DiscoverSynaptrixHandlers]

namespace MyTestApp.Behaviors
{
    public class MyRequest : IRequest<string> { }

    public class MyHandler : IRequestHandler<MyRequest, string>
    {
        public System.Threading.Tasks.ValueTask<string> Handle(MyRequest request, System.Threading.CancellationToken cancellationToken)
        {
            return new System.Threading.Tasks.ValueTask<string>(""ok"");
        }
    }

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

    [Fact]
    public void OpenGenericPipelineBehavior_RegistersWith_TypeOfSyntax()
    {
        var (_, generatedSource) = GeneratorTestHelper.RunGenerator(OpenGenericBehaviorSource);

        Assert.NotEmpty(generatedSource);
        // Must use the typeof(...) overload because TRequest/TResponse are not in scope at the call site.
        Assert.Contains(
            "services.AddTransient(typeof(global::Synaptrix.IPipelineBehavior<,>), typeof(global::MyTestApp.Behaviors.LoggingBehavior<,>));",
            generatedSource);
    }

    [Fact]
    public void OpenGenericPipelineBehavior_DoesNotEmit_InvalidGenericRegistration()
    {
        var (_, generatedSource) = GeneratorTestHelper.RunGenerator(OpenGenericBehaviorSource);

        // The buggy emission shape we are guarding against:
        //   services.AddTransient<global::...LoggingBehavior<TRequest, TResponse>, global::...LoggingBehavior<TRequest, TResponse>>();
        // and the variant against the interface:
        //   services.AddTransient<global::Synaptrix.IPipelineBehavior<TRequest, TResponse>, ...>();
        Assert.DoesNotContain("LoggingBehavior<TRequest, TResponse>", generatedSource);
        Assert.DoesNotContain("IPipelineBehavior<TRequest, TResponse>", generatedSource);
        // And the standalone concrete-type registration must not be emitted with the unbound generic form either.
        Assert.DoesNotContain("services.AddTransient<global::MyTestApp.Behaviors.LoggingBehavior<,>>", generatedSource);
    }

    [Fact]
    public void OpenGenericPipelineBehavior_GeneratesCompilableCode()
    {
        var (diagnostics, _) = GeneratorTestHelper.RunGeneratorAndCompile(OpenGenericBehaviorSource);

        var errors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            // CS7003 = "Unexpected use of an unbound generic name" — exactly the bug we are guarding.
            // Any other compile error in the generated output should also fail the test.
            .ToArray();

        Assert.True(
            errors.Length == 0,
            "Generated source produced compile errors:\n" + string.Join("\n", errors.Select(e => e.ToString())));
    }
}
