using Xunit;

namespace Synaptrix.Generator.Tests;

public class BasicGenerationTests
{
    [Fact]
    public void ShouldGenerate_AddSynaptrixHandlers_When_AttributeIsPresent()
    {
        var source = @"
using Synaptrix;
using Synaptrix.Attributes;

[assembly: DiscoverSynaptrixHandlers]

namespace MyTestApp.Handlers
{
    public class MyRequest : IRequest<string> { }

    public class MyHandler : IRequestHandler<MyRequest, string>
    {
        public System.Threading.Tasks.ValueTask<string> Handle(MyRequest request, System.Threading.CancellationToken cancellationToken)
        {
            return new System.Threading.Tasks.ValueTask<string>(""ok"");
        }
    }
}"; 

        var (diagnostics, generatedSource) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(generatedSource);
        Assert.Contains("public static IServiceCollection AddSynaptrixHandlers(this IServiceCollection services, ServiceLifetime mediatorLifetime = ServiceLifetime.Scoped)", generatedSource);
        Assert.Contains("services.AddTransient<global::Synaptrix.IRequestHandler<global::MyTestApp.Handlers.MyRequest, string>, global::MyTestApp.Handlers.MyHandler>();", generatedSource);
    }

    [Fact]
    public void ShouldNotGenerate_When_AttributeIsMissing()
    {
        var source = @"
using Synaptrix;

namespace MyTestApp.Handlers
{
    public class MyRequest : IRequest<string> { }

    public class MyHandler : IRequestHandler<MyRequest, string>
    {
        public System.Threading.Tasks.ValueTask<string> Handle(MyRequest request, System.Threading.CancellationToken cancellationToken)
        {
            return new System.Threading.Tasks.ValueTask<string>(""ok"");
}";

        var (diagnostics, generatedSource) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics);
        Assert.Empty(generatedSource);
    }
}
