using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Synaptrix.Generator;

/// <summary>
/// The Synaptrix generator class
/// </summary>
/// <seealso cref="IIncrementalGenerator"/>
[Generator]
// This class is a source generator that automatically registers Synaptrix handlers.
public class SynaptrixGenerator : IIncrementalGenerator
{
    private const string DiscoverAttributeName = "Synaptrix.Attributes.DiscoverSynaptrixHandlersAttribute";

    // Initializes the incremental generator.
    /// <summary>
    /// Initializes the context
    /// </summary>
    /// <param name="context">The context</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
        // Uncomment the following line to enable debugging during development.
        // System.Diagnostics.Debugger.Launch();
#endif
        // Check for the attribute
        var hasAttribute = context.CompilationProvider
            .Select(static (compilation, _) => compilation.Assembly.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == DiscoverAttributeName));

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

        // Combines collected handlers with compilation options and attribute check.
        var combinedProvider = collectedHandlers
            .Combine(compilationAndOptions)
            .Combine(context.CompilationProvider)
            .Combine(hasAttribute);

        // Registers the source output.
        context.RegisterSourceOutput(combinedProvider, (ctx, source) =>
        {
            var ((handlersAndOptions, compilation), shouldGenerate) = source;
            var (handlers, options) = handlersAndOptions;

            if (!shouldGenerate)
            {
                return;
            }

            // Default method name for registering handlers.
            var methodName = "AddSynaptrixHandlers";
            // Default namespace for generated code.
            var generatedNamespace = "SynaptrixGenerated";

            // Reads custom method name from build properties if specified.
            if (options.GlobalOptions.TryGetValue("build_property.Synaptrixgeneratedmethodname", out var customMethodName) && !string.IsNullOrWhiteSpace(customMethodName))
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
            var sourceCode = GenerateHandlersRegistrationCode(methodName, generatedNamespace, handlers, compilation);
            ctx.AddSource("SynaptrixGeneratedHandlersRegistrations.g.cs", SourceText.From(sourceCode, Encoding.UTF8));

            // Generate the concrete mediator class.
            if (!handlers.IsEmpty)
            {
                var mediatorCode = GenerateConcreteMediatorCode(generatedNamespace, handlers);
                ctx.AddSource("SynaptrixGeneratedMediator.g.cs", SourceText.From(mediatorCode, Encoding.UTF8));
            }
        });
    }

    // Checks if a syntax node is a candidate for a handler.
    /// <summary>
    /// Ises the handler candidate using the specified node
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns>The bool</returns>
    private static bool IsHandlerCandidate(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax { BaseList: not null };
    }

    // Retrieves handler information from a syntax context.
    /// <summary>
    /// Gets the handler info using the specified context
    /// </summary>
    /// <param name="context">The context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The handler info</returns>
    private static HandlerInfo? GetHandlerInfo(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        // Gets the declared symbol for the class.
        if (semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        if (classSymbol.IsAbstract)
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

                // Checks if the interface is a Synaptrix handler interface.
                if (genericDefinitionFullName == "global::Synaptrix.IRequestHandler<TRequest, TResponse>")
                {
                    var requestType = @interface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var responseType = @interface.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    implementedInterfaces.Add($"global::Synaptrix.IRequestHandler<{requestType}, {responseType}>");
                }
                else if (genericDefinitionFullName == "global::Synaptrix.IRequestHandler<TRequest>")
                {
                    var requestType = @interface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    implementedInterfaces.Add($"global::Synaptrix.IRequestHandler<{requestType}>");
                }
                else if (genericDefinitionFullName == "global::Synaptrix.INotificationHandler<TNotification>")
                {
                    var notificationType = @interface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    implementedInterfaces.Add($"global::Synaptrix.INotificationHandler<{notificationType}>");
                }
                else if (genericDefinitionFullName == "global::Synaptrix.IPipelineBehavior<TRequest, TResponse>")
                {
                    var requestType = @interface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var responseType = @interface.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    implementedInterfaces.Add($"global::Synaptrix.IPipelineBehavior<{requestType}, {responseType}>");
                }
                else if (genericDefinitionFullName == "global::Synaptrix.IStreamRequestHandler<TRequest, TResponse>")
                {
                    var requestType = @interface.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var responseType = @interface.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    implementedInterfaces.Add($"global::Synaptrix.IStreamRequestHandler<{requestType}, {responseType}>");
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
    /// <summary>
    /// Generates the handlers registration code using the specified method name
    /// </summary>
    /// <param name="methodName">The method name</param>
    /// <param name="generatedNamespace">The generated namespace</param>
    /// <param name="handlers">The handlers</param>
    /// <param name="compilation">The compilation to check references</param>
    /// <returns>The string</returns>
    private static string GenerateHandlersRegistrationCode(string methodName, string generatedNamespace, ImmutableArray<HandlerInfo> handlers, Compilation compilation)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// This file is automatically generated by Synaptrix.Generator.");
        sb.AppendLine("// Do not modify this file manually.");
        sb.AppendLine();
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Synaptrix;");
        sb.AppendLine();
        sb.AppendLine($"namespace {generatedNamespace}.Generated");
        sb.AppendLine("{");
        sb.AppendLine($"    public static class SynaptrixGeneratedRegistrations");
        sb.AppendLine("    {");
        sb.AppendLine("        /// <summary>");
        sb.AppendLine("        /// Automatically registers Synaptrix handlers.");
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

        // Recursive application functionality
        sb.AppendLine();
        sb.AppendLine("            // Register handlers from referenced assemblies");
        foreach (var referencedAssembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            // Check if the referenced assembly has the attribute
            bool shouldScan = referencedAssembly.GetAttributes()
                .Any(a => a.AttributeClass?.ToDisplayString() == DiscoverAttributeName);

            if (shouldScan)
            {
                // We assume default namespace conventions or we need to find the specific class. 
                // Since we can't easily know the namespace of the referenced assembly's generated code without scanning it, 
                // we'll try to guess based on assembly name or build properties if accessible (not easily accessible here).
                
                var refNamespace = referencedAssembly.Name; // Default default
                // Checking if the class exists (with .Generated suffix)
                var candidateType = compilation.GetTypeByMetadataName($"{refNamespace}.Generated.SynaptrixGeneratedRegistrations");

                // If not found, maybe it used "SynaptrixGenerated" default?
                if (candidateType == null)
                {
                   candidateType = compilation.GetTypeByMetadataName("SynaptrixGenerated.Generated.SynaptrixGeneratedRegistrations");
                }

                if (candidateType != null)
                {
                     // Fix: Dynamic discovery of the registration method
                     // We look for a public static method that returns IServiceCollection and takes IServiceCollection as parameter
                     var registrationMethod = candidateType.GetMembers()
                        .OfType<IMethodSymbol>()
                        .FirstOrDefault(m => 
                            m.IsStatic && 
                            m.DeclaredAccessibility == Accessibility.Public &&
                            m.ReturnType.Name == "IServiceCollection" &&
                            m.Parameters.Length == 1 &&
                            m.Parameters[0].Type.Name == "IServiceCollection");

                     if (registrationMethod != null)
                     {
                         sb.AppendLine($"            {candidateType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.{registrationMethod.Name}(services);");
                     }
                     else
                     {
                         // Fallback to default if signature lookup fails (though it shouldn't for generated code)
                         sb.AppendLine($"            {candidateType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.AddSynaptrixHandlers(services);");
                     }
                }
            }
        }

        sb.AppendLine();

        // Collect unique concrete handler types.
        var uniqueHandlers = handlers.Select(h => h.ImplementationTypeName).Distinct().ToList();

        // Register each concrete handler type as Singleton (for GeneratedMediator constructor injection).
        sb.AppendLine("            // Singleton registrations for GeneratedMediator constructor injection.");
        foreach (var handlerType in uniqueHandlers)
        {
            sb.AppendLine($"            services.AddSingleton<{handlerType}>();");
        }

        sb.AppendLine();
        sb.AppendLine("            // Register INotificationPublisher for use with the non-generated Mediator class.");
        sb.AppendLine("            if (!services.Any(d => d.ServiceType == typeof(global::Synaptrix.INotificationPublisher)))");
        sb.AppendLine("            {");
        sb.AppendLine("                services.AddSingleton<global::Synaptrix.INotificationPublisher, global::Synaptrix.ForeachAwaitPublisher>();");
        sb.AppendLine("            }");

        sb.AppendLine();
        if (handlers.IsEmpty)
        {
            // No local handlers: GeneratedMediator is not produced for this project.
            // Fall back to the reflection-based Mediator so IMediator/ISender are still resolvable.
            sb.AppendLine("            // No local handlers found: register the standard Mediator as fallback.");
            sb.AppendLine("            if (!services.Any(d => d.ServiceType == typeof(global::Synaptrix.IMediator)))");
            sb.AppendLine("            {");
            sb.AppendLine("                services.AddTransient<global::Synaptrix.IMediator, global::Synaptrix.Mediator>();");
            sb.AppendLine("                services.AddTransient<global::Synaptrix.ISender, global::Synaptrix.Mediator>();");
            sb.AppendLine("            }");
        }
        else
        {
            sb.AppendLine("            // Register the source-generated mediator as Singleton.");
            sb.AppendLine($"            services.AddSingleton<global::{generatedNamespace}.Generated.GeneratedMediator>();");
            sb.AppendLine($"            services.AddSingleton<global::Synaptrix.IMediator>(static sp => sp.GetRequiredService<global::{generatedNamespace}.Generated.GeneratedMediator>());");
            sb.AppendLine($"            services.AddSingleton<global::Synaptrix.ISender>(static sp => sp.GetRequiredService<global::{generatedNamespace}.Generated.GeneratedMediator>());");
        }

        sb.AppendLine();
        sb.AppendLine("            return services;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    // ── Concrete mediator generation ───────────────────────────────────────────────

    private static string GenerateConcreteMediatorCode(string generatedNamespace, ImmutableArray<HandlerInfo> handlers)
    {
        // ── Build field/param name maps ────────────────────────────────────────
        var handlerFields = new Dictionary<string, string>();   // implType → fieldName
        var ctorParams    = new Dictionary<string, string>();   // implType → ctorParamName
        var seenNames     = new HashSet<string>();

        foreach (var h in handlers)
        {
            var simple    = GetSimpleName(h.ImplementationTypeName);
            var fieldBase = "_" + Uncapitalize(simple);
            var fieldName = fieldBase;
            int idx = 0;
            while (!seenNames.Add(fieldName))
                fieldName = fieldBase + (++idx);
            handlerFields[h.ImplementationTypeName] = fieldName;
            ctorParams[h.ImplementationTypeName]    = Uncapitalize(simple) + (idx > 0 ? idx.ToString() : "");
        }

        // ── Build request/notification mappings ───────────────────────────────
        var responseHandlers     = new List<(string ReqType, string RespType, string Field)>();
        var voidHandlers         = new List<(string ReqType, string Field)>();
        var notifHandlersByType  = new Dictionary<string, List<string>>();  // notifType → fields
        var streamHandlers       = new List<(string ReqType, string RespType, string Field)>();

        foreach (var h in handlers)
        {
            var field = handlerFields[h.ImplementationTypeName];
            foreach (var iface in h.ImplementedInterfaceTypeNames)
            {
                if (iface.StartsWith("global::Synaptrix.IRequestHandler<"))
                {
                    var inner    = iface.Substring("global::Synaptrix.IRequestHandler<".Length, iface.Length - "global::Synaptrix.IRequestHandler<".Length - 1);
                    var commaIdx = FindTopLevelComma(inner);
                    if (commaIdx >= 0)
                        responseHandlers.Add((inner.Substring(0, commaIdx).Trim(), inner.Substring(commaIdx + 1).Trim(), field));
                    else
                        voidHandlers.Add((inner.Trim(), field));
                }
                else if (iface.StartsWith("global::Synaptrix.INotificationHandler<"))
                {
                    var notifType = iface.Substring("global::Synaptrix.INotificationHandler<".Length, iface.Length - "global::Synaptrix.INotificationHandler<".Length - 1).Trim();
                    if (!notifHandlersByType.ContainsKey(notifType))
                        notifHandlersByType[notifType] = new List<string>();
                    notifHandlersByType[notifType].Add(field);
                }
                else if (iface.StartsWith("global::Synaptrix.IStreamRequestHandler<"))
                {
                    var inner    = iface.Substring("global::Synaptrix.IStreamRequestHandler<".Length, iface.Length - "global::Synaptrix.IStreamRequestHandler<".Length - 1);
                    var commaIdx = FindTopLevelComma(inner);
                    if (commaIdx >= 0)
                        streamHandlers.Add((inner.Substring(0, commaIdx).Trim(), inner.Substring(commaIdx + 1).Trim(), field));
                }
            }
        }

        // ── Emit source ────────────────────────────────────────────────────────
        var sb = new StringBuilder();
        sb.AppendLine("// This file is automatically generated by Synaptrix.Generator.");
        sb.AppendLine("// Do not modify this file manually.");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine($"namespace {generatedNamespace}.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>Source-generated mediator with direct handler dispatch (zero DI lookup on hot path).</summary>");
        sb.AppendLine("    public sealed partial class GeneratedMediator : global::Synaptrix.IMediator, global::Synaptrix.ISender");
        sb.AppendLine("    {");

        // ── Fields ─────────────────────────────────────────────────────────────
        foreach (var h in handlers)
            sb.AppendLine($"        private readonly {h.ImplementationTypeName} {handlerFields[h.ImplementationTypeName]};");
        sb.AppendLine();

        // ── Constructor ────────────────────────────────────────────────────────
        var ctorArgList = string.Join(",\n            ",
            handlers.Select(h => $"{h.ImplementationTypeName} {ctorParams[h.ImplementationTypeName]}"));
        sb.AppendLine($"        public GeneratedMediator(");
        sb.AppendLine($"            {ctorArgList})");
        sb.AppendLine("        {");
        foreach (var h in handlers)
            sb.AppendLine($"            {handlerFields[h.ImplementationTypeName]} = {ctorParams[h.ImplementationTypeName]};");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ── Send<TResponse>(IRequest<TResponse>) ──────────────────────────────
        sb.AppendLine("        public async global::System.Threading.Tasks.ValueTask<TResponse> Send<TResponse>(global::Synaptrix.IRequest<TResponse> request, global::System.Threading.CancellationToken cancellationToken = default)");
        sb.AppendLine("        {");
        for (int i = 0; i < responseHandlers.Count; i++)
        {
            var (reqType, respType, field) = responseHandlers[i];
            sb.AppendLine($"            if (request is {reqType} r{i})");
            sb.AppendLine($"                return (TResponse)(object)await {field}.Handle(r{i}, cancellationToken).ConfigureAwait(false);");
        }
        sb.AppendLine("            throw new global::System.InvalidOperationException($\"No handler registered for {request.GetType().FullName}\");");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ── Send(IRequest) ─────────────────────────────────────────────────────
        sb.AppendLine("        public async global::System.Threading.Tasks.ValueTask Send(global::Synaptrix.IRequest request, global::System.Threading.CancellationToken cancellationToken = default)");
        sb.AppendLine("        {");
        for (int i = 0; i < voidHandlers.Count; i++)
        {
            var (reqType, field) = voidHandlers[i];
            sb.AppendLine($"            if (request is {reqType} v{i}) {{ await {field}.Handle(v{i}, cancellationToken).ConfigureAwait(false); return; }}");
        }
        // Response requests also implement IRequest — handle them here too (discards response).
        for (int i = 0; i < responseHandlers.Count; i++)
        {
            var (reqType, respType, field) = responseHandlers[i];
            sb.AppendLine($"            if (request is {reqType} rv{i}) {{ await {field}.Handle(rv{i}, cancellationToken).ConfigureAwait(false); return; }}");
        }
        sb.AppendLine("            throw new global::System.InvalidOperationException($\"No handler registered for {request.GetType().FullName}\");");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ── Send(object) ───────────────────────────────────────────────────────
        sb.AppendLine("        public async global::System.Threading.Tasks.ValueTask<object?> Send(object request, global::System.Threading.CancellationToken cancellationToken = default)");
        sb.AppendLine("        {");
        for (int i = 0; i < responseHandlers.Count; i++)
        {
            var (reqType, respType, field) = responseHandlers[i];
            sb.AppendLine($"            if (request is {reqType} o{i})");
            sb.AppendLine($"                return (object?)await {field}.Handle(o{i}, cancellationToken).ConfigureAwait(false);");
        }
        for (int i = 0; i < voidHandlers.Count; i++)
        {
            var (reqType, field) = voidHandlers[i];
            sb.AppendLine($"            if (request is {reqType} ov{i}) {{ await {field}.Handle(ov{i}, cancellationToken).ConfigureAwait(false); return null; }}");
        }
        sb.AppendLine("            throw new global::System.InvalidOperationException($\"No handler registered for {request.GetType().FullName}\");");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ── Publish(INotification) — inline sequential dispatch with sync fast-path ──
        // No allocation when all handlers complete synchronously (IsCompletedSuccessfully).
        sb.AppendLine("        public global::System.Threading.Tasks.ValueTask Publish(global::Synaptrix.INotification notification, global::System.Threading.CancellationToken cancellationToken = default)");
        sb.AppendLine("        {");
        foreach (var kvp in notifHandlersByType)
        {
            var safe      = GetSafeIdentifier(kvp.Key);
            var hList     = kvp.Value;  // ordered list of field names
            sb.AppendLine($"            if (notification is {kvp.Key} _n_{safe})");
            sb.AppendLine("            {");
            for (int i = 0; i < hList.Count; i++)
            {
                sb.AppendLine($"                var _t{i}_{safe} = {hList[i]}.Handle(_n_{safe}, cancellationToken);");
                if (i < hList.Count - 1)
                {
                    // Not the last handler — bail out to async slow-path if not already done.
                    sb.AppendLine($"                if (!_t{i}_{safe}.IsCompletedSuccessfully) return _FinishPublish_{safe}_From{i}(_n_{safe}, _t{i}_{safe}, cancellationToken);");
                }
                else
                {
                    // Last handler — return default (completed ValueTask) or the ValueTask itself (avoid state machine).
                    sb.AppendLine($"                if (_t{i}_{safe}.IsCompletedSuccessfully) return default;");
                    sb.AppendLine($"                return _t{i}_{safe};");
                }
            }
            if (hList.Count == 0)
                sb.AppendLine("                return default;");
            sb.AppendLine("            }");
        }
        sb.AppendLine("            throw new global::System.InvalidOperationException($\"No handler registered for {notification.GetType().FullName}\");");
        sb.AppendLine("        }");
        sb.AppendLine();

        // ── Async slow-path helpers for Publish (only entered when a handler is truly async) ─
        foreach (var kvp in notifHandlersByType)
        {
            var safe  = GetSafeIdentifier(kvp.Key);
            var hList = kvp.Value;
            // Generate one helper per "starting index" 0..(n-2). Index n-1 (last) is never the
            // starting point because we return its Task directly in the fast-path above.
            for (int fromIdx = 0; fromIdx < hList.Count - 1; fromIdx++)
            {
                sb.AppendLine($"        private async global::System.Threading.Tasks.ValueTask _FinishPublish_{safe}_From{fromIdx}({kvp.Key} notification, global::System.Threading.Tasks.ValueTask pending, global::System.Threading.CancellationToken cancellationToken)");
                sb.AppendLine("        {");
                sb.AppendLine("            await pending.ConfigureAwait(false);");
                for (int j = fromIdx + 1; j < hList.Count; j++)
                    sb.AppendLine($"            await {hList[j]}.Handle(notification, cancellationToken).ConfigureAwait(false);");
                sb.AppendLine("        }");
                sb.AppendLine();
            }
        }

        // ── CreateStream<TResponse>(IStreamRequest<TResponse>) ─────────────────
        sb.AppendLine("        public global::System.Collections.Generic.IAsyncEnumerable<TResponse> CreateStream<TResponse>(global::Synaptrix.IStreamRequest<TResponse> request, global::System.Threading.CancellationToken cancellationToken = default)");
        sb.AppendLine("        {");
        for (int i = 0; i < streamHandlers.Count; i++)
        {
            var (reqType, respType, field) = streamHandlers[i];
            sb.AppendLine($"            if (request is {reqType} s{i})");
            sb.AppendLine($"                return (global::System.Collections.Generic.IAsyncEnumerable<TResponse>){field}.Handle(s{i}, cancellationToken);");
        }
        sb.AppendLine("            throw new global::System.InvalidOperationException($\"No stream handler registered for {request.GetType().FullName}\");");
        sb.AppendLine("        }");
        sb.AppendLine();

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────────

    /// <summary>Returns the simple (unqualified) type name from a fully qualified name.</summary>
    private static string GetSimpleName(string fqn)
    {
        var lastDot = fqn.LastIndexOf('.');
        return lastDot >= 0 ? fqn.Substring(lastDot + 1) : fqn;
    }

    /// <summary>Lower-cases the first character of <paramref name="s"/>.</summary>
    private static string Uncapitalize(string s)
        => s.Length == 0 ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);

    /// <summary>
    /// Finds the index of the first top-level comma (depth 0 relative to angle brackets)
    /// in <paramref name="s"/>. Returns -1 if not found.
    /// </summary>
    private static int FindTopLevelComma(string s)
    {
        int depth = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if      (s[i] == '<') depth++;
            else if (s[i] == '>') depth--;
            else if (s[i] == ',' && depth == 0) return i;
        }
        return -1;
    }

    /// <summary>
    /// Converts <paramref name="typeName"/> into a valid C# identifier by replacing
    /// non-alphanumeric characters with underscores and collapsing consecutive underscores.
    /// </summary>
    private static string GetSafeIdentifier(string typeName)
    {
        var sb = new StringBuilder(typeName.Length);
        foreach (var c in typeName)
            sb.Append(char.IsLetterOrDigit(c) ? c : '_');
        var result = sb.ToString();
        // Collapse runs of underscores.
        while (result.Contains("__"))
            result = result.Replace("__", "_");
        return result.Trim('_');
    }
}