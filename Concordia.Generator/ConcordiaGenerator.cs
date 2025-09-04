using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Concordia.Generator;

[Generator]
// This class is a source generator that automatically registers Concordia handlers.
public class ConcordiaGenerator : IIncrementalGenerator
{
    // Initializes the incremental generator.
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        // Uncomment the following line to enable debugging during development.
        // System.Diagnostics.Debugger.Launch();
#endif
        // Retrieves analyzer config options.
        var compilationAndOptions = context.AnalyzerConfigOptionsProvider
            .Select((options, cancellationToken) => options);

        // Creates syntax provider to find handler classes.
        IncrementalValuesProvider<HandlerInfo?> handlerClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsHandlerCandidate(node),
                transform: static (ctx, ct) => GetHandlerInfo(ctx, ct)
            )
            .Where(static handlerInfo => handlerInfo is not null);

        // Collects all handler info.
        IncrementalValueProvider<ImmutableArray<HandlerInfo>> collectedHandlers = handlerClasses.Collect()
            .Select((handlers, _) => handlers.Where(h => h is not null).Select(h => h!).ToImmutableArray());

        // Combines collected handlers with compilation options.
        var combinedProvider = collectedHandlers.Combine(compilationAndOptions);

        // Registers the source output.
        context.RegisterSourceOutput(combinedProvider, (ctx, source) =>
        {
            var handlers = source.Left;
            var options = source.Right;

            // Default method name for registering handlers.
            var methodName = "AddConcordiaHandlers";
            // Default namespace for generated code.
            var generatedNamespace = "ConcordiaGenerated";

            // Reads custom method name from build properties if specified.
            if (options.GlobalOptions.TryGetValue("build_property.concordiageneratedmethodname", out var customMethodName) && !string.IsNullOrWhiteSpace(customMethodName))
            {
                methodName = customMethodName;
            }

            // Reads root namespace from build properties, otherwise uses project name.
            if (options.GlobalOptions.TryGetValue("build_property.rootnamespace", out var projectRootNamespace) && !string.IsNullOrWhiteSpace(projectRootNamespace))
            {
                generatedNamespace = projectRootNamespace;
            }
            else if (options.GlobalOptions.TryGetValue("build_property.msbuildprojectname", out var projectName) && !string.IsNullOrWhiteSpace(projectName))
            {
                generatedNamespace = projectName;
            }

            // Generates the source code for registering handlers.
            var sourceCode = GenerateHandlersRegistrationCode(methodName, generatedNamespace, handlers);
            ctx.AddSource("ConcordiaGeneratedHandlersRegistrations.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        });
    }

    // Checks if a syntax node is a candidate for a handler.
    private static bool IsHandlerCandidate(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.BaseList != null &&
               classDeclaration.BaseList.Types.Any(baseType =>
                   baseType.Type is GenericNameSyntax genericName &&
                   (genericName.Identifier.Text.Contains("RequestHandler") ||
                    genericName.Identifier.Text.Contains("NotificationHandler") ||
                    genericName.Identifier.Text.Contains("PipelineBehavior")));
    }

    // Retrieves handler information from a syntax context.
    private static HandlerInfo? GetHandlerInfo(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        // Gets the declared symbol for the class.
        if (semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        var implementedInterfaces = new List<string>();

        // Iterates through all implemented interfaces.
        foreach (var @interface in classSymbol.AllInterfaces)
        {
            if (@interface.IsGenericType)
            {
                var genericDefinition = @interface.ConstructedFrom;
                var genericDefinitionFullName = genericDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                // Checks if the interface is a Concordia handler interface.
                if (genericDefinitionFullName == "global::Concordia.IRequestHandler<TRequest, TResponse>")
                {
                    var requestType = @interface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var responseType = @interface.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    implementedInterfaces.Add($"global::Concordia.IRequestHandler<{requestType}, {responseType}>");
                }
                else if (genericDefinitionFullName == "global::Concordia.IRequestHandler<TRequest>")
                {
                    var requestType = @interface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    implementedInterfaces.Add($"global::Concordia.IRequestHandler<{requestType}>");
                }
                else if (genericDefinitionFullName == "global::Concordia.INotificationHandler<TNotification>")
                {
                    var notificationType = @interface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    implementedInterfaces.Add($"global::Concordia.INotificationHandler<{notificationType}>");
                }
                else if (genericDefinitionFullName == "global::Concordia.IPipelineBehavior<TRequest, TResponse>")
                {
                    var requestType = @interface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var responseType = @interface.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    implementedInterfaces.Add($"global::Concordia.IPipelineBehavior<{requestType}, {responseType}>");
                }
            }
        }

        // Creates a HandlerInfo if any supported interfaces are implemented.
        if (implementedInterfaces.Any())
        {
            var implementationTypeName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return new HandlerInfo(implementationTypeName, implementedInterfaces);
        }

        return null;
    }

    // Generates the handlers registration code.
    private static string GenerateHandlersRegistrationCode(string methodName, string generatedNamespace, ImmutableArray<HandlerInfo> handlers)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// This file is automatically generated by Concordia.Generator.");
        sb.AppendLine("// Do not modify this file manually.");
        sb.AppendLine();
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Concordia;");
        sb.AppendLine();
        sb.AppendLine($"namespace {generatedNamespace}");
        sb.AppendLine("{");
        sb.AppendLine($"    public static class ConcordiaGeneratedRegistrations");
        sb.AppendLine("    {");
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Automatically registers Concordia handlers.");
        sb.AppendLine("        /// This method is generated at compile time by the Source Generator.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine("        /// <param name=\"services\">The service collection to add to.</param>");
        sb.AppendLine("        /// <returns>The modified service collection.</returns>");
        sb.AppendLine($"        public static IServiceCollection {methodName}(this IServiceCollection services)");
        sb.AppendLine("        {");

        // Registers each handler with its implemented interfaces.
        foreach (var handler in handlers)
        {
            foreach (var implementedInterface in handler.ImplementedInterfaceTypeNames)
            {
                sb.AppendLine($"            services.AddTransient<{implementedInterface}, {handler.ImplementationTypeName}>();");
            }
        }

        sb.AppendLine();
        sb.AppendLine("            return services;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }
}