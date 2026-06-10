using Xunit;
using System.Collections.Generic;

namespace Synaptrix.Generator.Tests;

public class SynaptrixGeneratedMethodNameTests
{
    [Fact]
    public void ShouldGenerate_CustomMethodName_When_PropertyIsSet()
    {
        var source = @"
using Synaptrix;
using Synaptrix.Attributes;

[assembly: DiscoverSynaptrixHandlers]

namespace MyTestApp.Handlers
{
    public class MyRequest : IRequest { }

    public class MyHandler : IRequestHandler<MyRequest>
    {
        public System.Threading.Tasks.ValueTask Handle(MyRequest request, System.Threading.CancellationToken cancellationToken)
        {
            return default;
        }
    }
}";

        var globalOptions = new Dictionary<string, string>
        {
            { "build_property.Synaptrixgeneratedmethodname", "AddMyCustomHandlers" }
        };

        var (diagnostics, generatedSource) = GeneratorTestHelper.RunGenerator(source, globalOptions);

        Assert.Empty(diagnostics);
        Assert.NotEmpty(generatedSource);
        
        // Use Assert.Contains with a substring to avoid whitespace issues, but make it specific enough
        Assert.Contains("public static IServiceCollection AddMyCustomHandlers(this IServiceCollection services, ServiceLifetime mediatorLifetime = ServiceLifetime.Scoped)", generatedSource);
        Assert.DoesNotContain("AddSynaptrixHandlers", generatedSource);
    }
}
