using Concordia;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Concordia.MediatR;

/// <summary>
/// Helper to find closed generic interfaces.
/// Similar to MediatR.Registration.Extensions.FindInterfacesThatClose.
/// </summary>
internal static class ReflectionExtensions
{
    /// <summary>
    /// Finds the interfaces that close using the specified plugged type
    /// </summary>
    /// <param name="pluggedType">The plugged type</param>
    /// <param name="templateType">The template type</param>
    /// <returns>An enumerable of type</returns>
    public static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
    {
        return FindInterfacesThatCloseAll(pluggedType, templateType);
    }

    /// <summary>
    /// Finds the interfaces that close all using the specified plugged type
    /// </summary>
    /// <param name="pluggedType">The plugged type</param>
    /// <param name="templateType">The template type</param>
    /// <returns>An enumerable of type</returns>
    private static IEnumerable<Type> FindInterfacesThatCloseAll(Type pluggedType, Type templateType)
    {
        if (!pluggedType.IsClass) // Only classes can implement interfaces
        {
            yield break;
        }

        foreach (var @interface in pluggedType.GetInterfaces())
        {
            if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == templateType)
            {
                yield return @interface;
            }
        }

        // Recursive search in base types' interfaces
        if (pluggedType.BaseType != null && pluggedType.BaseType != typeof(object))
        {
            foreach (var result in FindInterfacesThatCloseAll(pluggedType.BaseType, templateType))
            {
                yield return result;
            }
        }
    }
}

/// <summary>
/// Configuration class for Concordia's AddMediator extension,
/// mirroring MediatRServiceConfiguration functionalities.
/// </summary>
public class ConcordiaMediatRServiceConfiguration
{
    /// <summary>
    /// Mediator implementation type to register. Default is <see cref="Mediator"/>.
    /// </summary>
    public Type MediatorImplementationType { get; set; } = typeof(Mediator);

    /// <summary>
    /// Strategy for publishing notifications. Defaults to <see cref="ForeachAwaitPublisher"/>.
    /// </summary>
    public INotificationPublisher NotificationPublisher { get; set; } = new ForeachAwaitPublisher();

    /// <summary>
    /// Type of notification publisher strategy to register. If set, overrides <see cref="NotificationPublisher"/>.
    /// </summary>
    public Type? NotificationPublisherType { get; set; }

    /// <summary>
    /// Service lifetime to register services under. Default value is <see cref="ServiceLifetime.Transient"/>.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// Flag that controls whether Concordia should attempt to automatically scan assemblies
    /// for handler registration. If set to true, automatic scanning is disabled.
    /// </summary>
    public bool DisableAssemblyScanning { get; set; } = false;

    /// <summary>
    /// Gets the value of the assemblies to register
    /// </summary>
    internal List<Assembly> AssembliesToRegister { get; } = new();

    /// <summary>
    /// List of behaviors to register in specific order.
    /// </summary>
    public List<ServiceDescriptor> BehaviorsToRegister { get; } = new();

    /// <summary>
    /// List of stream behaviors to register in specific order.
    /// </summary>
    public List<ServiceDescriptor> StreamBehaviorsToRegister { get; } = new();

    /// <summary>
    /// List of request pre-processors to register in specific order.
    /// </summary>
    public List<ServiceDescriptor> RequestPreProcessorsToRegister { get; } = new();

    /// <summary>
    /// List of request post-processors to register in specific order.
    /// </summary>
    public List<ServiceDescriptor> RequestPostProcessorsToRegister { get; } = new();

    /// <summary>
    /// List of request handlers to register in specific order.
    /// </summary>
    public List<ServiceDescriptor> RequestHandlersToRegister { get; } = new();

    /// <summary>
    /// List of notification handlers to register in specific order.
    /// </summary>
    public List<ServiceDescriptor> NotificationHandlersToRegister { get; } = new();

    /// <summary>
    /// Register various handlers from assembly containing given type.
    /// </summary>
    /// <typeparam name="T">Type from assembly to scan.</typeparam>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration RegisterServicesFromAssemblyContaining<T>()
        => RegisterServicesFromAssemblyContaining(typeof(T));

    /// <summary>
    /// Register various handlers from assembly containing given type.
    /// </summary>
    /// <param name="type">Type from assembly to scan.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration RegisterServicesFromAssemblyContaining(Type type)
        => RegisterServicesFromAssembly(type.Assembly);

    /// <summary>
    /// Register various handlers from assembly.
    /// </summary>
    /// <param name="assembly">Assembly to scan.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration RegisterServicesFromAssembly(Assembly assembly)
    {
        AssembliesToRegister.Add(assembly);
        return this;
    }

    /// <summary>
    /// Register various handlers from assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        AssembliesToRegister.AddRange(assemblies);
        return this;
    }

    /// <summary>
    /// Register a closed behavior type.
    /// </summary>
    /// <typeparam name="TServiceType">Closed behavior interface type.</typeparam>
    /// <typeparam name="TImplementationType">Closed behavior implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddBehavior<TServiceType, TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TServiceType : IPipelineBehavior<IRequest<object>, object>
        where TImplementationType : TServiceType
        => AddBehavior(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed behavior type against all <see cref="IPipelineBehavior{TRequest,TResponse}"/> implementations.
    /// </summary>
    /// <typeparam name="TImplementationType">Closed behavior implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddBehavior<TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TImplementationType : IPipelineBehavior<IRequest<object>, object>
        => AddBehavior(typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed behavior type against all <see cref="IPipelineBehavior{TRequest,TResponse}"/> implementations.
    /// </summary>
    /// <param name="implementationType">Closed behavior implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddBehavior(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        var implementedGenericInterfaces = implementationType.FindInterfacesThatClose(typeof(IPipelineBehavior<,>)).ToList();

        if (implementedGenericInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{implementationType.Name} must implement {typeof(IPipelineBehavior<,>).FullName}");
        }

        foreach (var implementedBehaviorType in implementedGenericInterfaces)
        {
            BehaviorsToRegister.Add(new ServiceDescriptor(implementedBehaviorType, implementationType, serviceLifetime));
        }

        return this;
    }

    /// <summary>
    /// Register a closed behavior type.
    /// </summary>
    /// <param name="serviceType">Closed behavior interface type.</param>
    /// <param name="implementationType">Closed behavior implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddBehavior(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        BehaviorsToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));
        return this;
    }

    /// <summary>
    /// Registers an open behavior type against the <see cref="IPipelineBehavior{TRequest,TResponse}"/> open generic interface type.
    /// </summary>
    /// <param name="openBehaviorType">An open generic behavior type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddOpenBehavior(Type openBehaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (!openBehaviorType.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must be an open generic type definition.");
        }

        var implementedGenericInterfaces = openBehaviorType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
        var implementedOpenBehaviorInterfaces = new HashSet<Type>(implementedGenericInterfaces.Where(i => i == typeof(IPipelineBehavior<,>)));

        if (implementedOpenBehaviorInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must implement {typeof(IPipelineBehavior<,>).FullName}");
        }

        foreach (var openBehaviorInterface in implementedOpenBehaviorInterfaces)
        {
            BehaviorsToRegister.Add(new ServiceDescriptor(openBehaviorInterface, openBehaviorType, serviceLifetime));
        }

        return this;
    }

    /// <summary>
    /// Register a closed stream behavior type.
    /// </summary>
    /// <typeparam name="TServiceType">Closed stream behavior interface type.</typeparam>
    /// <typeparam name="TImplementationType">Closed stream behavior implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddStreamBehavior<TServiceType, TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TServiceType : IStreamPipelineBehavior<IRequest<IAsyncEnumerable<object>>, object>
        where TImplementationType : TServiceType
        => AddStreamBehavior(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed stream behavior type.
    /// </summary>
    /// <param name="serviceType">Closed stream behavior interface type.</param>
    /// <param name="implementationType">Closed stream behavior implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddStreamBehavior(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        StreamBehaviorsToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));
        return this;
    }

    /// <summary>
    /// Register a closed stream behavior type against all <see cref="IStreamPipelineBehavior{TRequest,TResponse}"/> implementations.
    /// </summary>
    /// <typeparam name="TImplementationType">Closed stream behavior implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddStreamBehavior<TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TImplementationType : IStreamPipelineBehavior<IRequest<IAsyncEnumerable<object>>, object>
        => AddStreamBehavior(typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed stream behavior type against all <see cref="IStreamPipelineBehavior{TRequest,TResponse}"/> implementations.
    /// </summary>
    /// <param name="implementationType">Closed stream behavior implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddStreamBehavior(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        var implementedGenericInterfaces = implementationType.FindInterfacesThatClose(typeof(IStreamPipelineBehavior<,>)).ToList();

        if (implementedGenericInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{implementationType.Name} must implement {typeof(IStreamPipelineBehavior<,>).FullName}");
        }

        foreach (var implementedBehaviorType in implementedGenericInterfaces)
        {
            StreamBehaviorsToRegister.Add(new ServiceDescriptor(implementedBehaviorType, implementationType, serviceLifetime));
        }

        return this;
    }

    /// <summary>
    /// Registers an open stream behavior type against the <see cref="IStreamPipelineBehavior{TRequest,TResponse}"/> open generic interface type.
    /// </summary>
    /// <param name="openBehaviorType">An open generic stream behavior type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddOpenStreamBehavior(Type openBehaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (!openBehaviorType.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must be an open generic type definition.");
        }

        var implementedGenericInterfaces = openBehaviorType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
        var implementedOpenBehaviorInterfaces = new HashSet<Type>(implementedGenericInterfaces.Where(i => i == typeof(IStreamPipelineBehavior<,>)));

        if (implementedOpenBehaviorInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must implement {typeof(IStreamPipelineBehavior<,>).FullName}");
        }

        foreach (var openBehaviorInterface in implementedOpenBehaviorInterfaces)
        {
            StreamBehaviorsToRegister.Add(new ServiceDescriptor(openBehaviorInterface, openBehaviorType, serviceLifetime));
        }

        return this;
    }

    /// <summary>
    /// Register a closed request pre-processor type.
    /// </summary>
    /// <typeparam name="TServiceType">Closed request pre-processor interface type.</typeparam>
    /// <typeparam name="TImplementationType">Closed request pre-processor implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestPreProcessor<TServiceType, TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TServiceType : IRequestPreProcessor<IRequest>
        where TImplementationType : TServiceType
        => AddRequestPreProcessor(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed request pre-processor type.
    /// </summary>
    /// <param name="serviceType">Closed request pre-processor interface type.</param>
    /// <param name="implementationType">Closed request pre-processor implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestPreProcessor(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        RequestPreProcessorsToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));
        return this;
    }

    /// <summary>
    /// Register a closed request pre-processor type against all <see cref="IRequestPreProcessor{TRequest}"/> implementations.
    /// </summary>
    /// <typeparam name="TImplementationType">Closed request pre-processor implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestPreProcessor<TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TImplementationType : class // Changed from IRequest to class
        => AddRequestPreProcessor(typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed request pre-processor type against all <see cref="IRequestPreProcessor{TRequest}"/> implementations.
    /// </summary>
    /// <param name="implementationType">Closed request pre-processor implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestPreProcessor(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        var implementedGenericInterfaces = implementationType.FindInterfacesThatClose(typeof(IRequestPreProcessor<>)).ToList();

        if (implementedGenericInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{implementationType.Name} must implement {typeof(IRequestPreProcessor<>).FullName}");
        }

        foreach (var implementedPreProcessorType in implementedGenericInterfaces)
        {
            RequestPreProcessorsToRegister.Add(new ServiceDescriptor(implementedPreProcessorType, implementationType, serviceLifetime));
        }
        return this;
    }

    /// <summary>
    /// Registers an open request pre-processor type against the <see cref="IRequestPreProcessor{TRequest}"/> open generic interface type.
    /// </summary>
    /// <param name="openBehaviorType">An open generic request pre-processor type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddOpenRequestPreProcessor(Type openBehaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (!openBehaviorType.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must be an open generic type definition.");
        }

        var implementedGenericInterfaces = openBehaviorType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
        var implementedOpenBehaviorInterfaces = new HashSet<Type>(implementedGenericInterfaces.Where(i => i == typeof(IRequestPreProcessor<>)));

        if (implementedOpenBehaviorInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must implement {typeof(IRequestPreProcessor<>).FullName}");
        }

        foreach (var openBehaviorInterface in implementedOpenBehaviorInterfaces)
        {
            RequestPreProcessorsToRegister.Add(new ServiceDescriptor(openBehaviorInterface, openBehaviorType, serviceLifetime));
        }

        return this;
    }

    /// <summary>
    /// Register a closed request post-processor type.
    /// </summary>
    /// <typeparam name="TServiceType">Closed request post-processor interface type.</typeparam>
    /// <typeparam name="TImplementationType">Closed request post-processor implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestPostProcessor<TServiceType, TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TServiceType : IRequestPostProcessor<IRequest<object>, object>
        where TImplementationType : TServiceType
        => AddRequestPostProcessor(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed request post-processor type.
    /// </summary>
    /// <param name="serviceType">Closed request post-processor interface type.</param>
    /// <param name="implementationType">Closed request post-processor implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestPostProcessor(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        RequestPostProcessorsToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));
        return this;
    }

    /// <summary>
    /// Register a closed request post-processor type against all <see cref="IRequestPostProcessor{TRequest,TResponse}"/> implementations.
    /// </summary>
    /// <typeparam name="TImplementationType">Closed request post-processor implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestPostProcessor<TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TImplementationType : class // Changed from IRequest<object> to class
        => AddRequestPostProcessor(typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed request post-processor type against all <see cref="IRequestPostProcessor{TRequest,TResponse}"/> implementations.
    /// </summary>
    /// <param name="implementationType">Closed request post-processor implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestPostProcessor(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        var implementedGenericInterfaces = implementationType.FindInterfacesThatClose(typeof(IRequestPostProcessor<,>)).ToList();

        if (implementedGenericInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{implementationType.Name} must implement {typeof(IRequestPostProcessor<,>).FullName}");
        }

        foreach (var implementedPostProcessorType in implementedGenericInterfaces)
        {
            RequestPostProcessorsToRegister.Add(new ServiceDescriptor(implementedPostProcessorType, implementationType, serviceLifetime));
        }
        return this;
    }

    /// <summary>
    /// Registers an open request post-processor type against the <see cref="IRequestPostProcessor{TRequest,TResponse}"/> open generic interface type.
    /// </summary>
    /// <param name="openBehaviorType">An open generic request post-processor type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddOpenRequestPostProcessor(Type openBehaviorType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (!openBehaviorType.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must be an open generic type definition.");
        }

        var implementedGenericInterfaces = openBehaviorType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
        var implementedOpenBehaviorInterfaces = new HashSet<Type>(implementedGenericInterfaces.Where(i => i == typeof(IRequestPostProcessor<,>)));

        if (implementedOpenBehaviorInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openBehaviorType.Name} must implement {typeof(IRequestPostProcessor<,>).FullName}");
        }

        foreach (var openBehaviorInterface in implementedOpenBehaviorInterfaces)
        {
            RequestPostProcessorsToRegister.Add(new ServiceDescriptor(openBehaviorInterface, openBehaviorType, serviceLifetime));
        }

        return this;
    }

    /// <summary>
    /// Register a closed request handler type.
    /// </summary>
    /// <typeparam name="TServiceType">Closed request handler interface type.</typeparam>
    /// <typeparam name="TImplementationType">Closed request handler implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestHandler<TServiceType, TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TServiceType : IRequest // Constraint to ensure it's a request handler type
        where TImplementationType : TServiceType
        => AddRequestHandler(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed request handler type.
    /// </summary>
    /// <param name="serviceType">Closed request handler interface type.</param>
    /// <param name="implementationType">Closed request handler implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestHandler(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        RequestHandlersToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));
        return this;
    }

    /// <summary>
    /// Register a closed request handler type against all <see cref="IRequestHandler{TRequest,TResponse}"/> or <see cref="IRequestHandler{TRequest}"/> implementations.
    /// </summary>
    /// <typeparam name="TImplementationType">Closed request handler implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestHandler<TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TImplementationType : class // Changed from IRequest to class
        => AddRequestHandler(typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed request handler type against all <see cref="IRequestHandler{TRequest,TResponse}"/> or <see cref="IRequestHandler{TRequest}"/> implementations.
    /// </summary>
    /// <param name="implementationType">Closed request handler implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddRequestHandler(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        var implementedGenericInterfaces = implementationType.FindInterfacesThatClose(typeof(IRequestHandler<,>))
                                                             .Concat(implementationType.FindInterfacesThatClose(typeof(IRequestHandler<>)))
                                                             .ToList();

        if (implementedGenericInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{implementationType.Name} must implement {typeof(IRequestHandler<,>).FullName} or {typeof(IRequestHandler<>).FullName}");
        }

        foreach (var implementedHandlerType in implementedGenericInterfaces)
        {
            RequestHandlersToRegister.Add(new ServiceDescriptor(implementedHandlerType, implementationType, serviceLifetime));
        }
        return this;
    }

    /// <summary>
    /// Registers an open request handler type against its open generic interface type.
    /// </summary>
    /// <param name="openHandlerType">An open generic request handler type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddOpenRequestHandler(Type openHandlerType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (!openHandlerType.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"{openHandlerType.Name} must be an open generic type definition.");
        }

        var implementedGenericInterfaces = openHandlerType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
        var implementedOpenHandlerInterfaces = new HashSet<Type>(
            implementedGenericInterfaces.Where(i => i == typeof(IRequestHandler<,>) || i == typeof(IRequestHandler<>)));

        if (implementedOpenHandlerInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openHandlerType.Name} must implement {typeof(IRequestHandler<,>).FullName} or {typeof(IRequestHandler<>).FullName}");
        }

        foreach (var openHandlerInterface in implementedOpenHandlerInterfaces)
        {
            RequestHandlersToRegister.Add(new ServiceDescriptor(openHandlerInterface, openHandlerType, serviceLifetime));
        }

        return this;
    }

    /// <summary>
    /// Register a closed notification handler type.
    /// </summary>
    /// <typeparam name="TServiceType">Closed notification handler interface type.</typeparam>
    /// <typeparam name="TImplementationType">Closed notification handler implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddNotificationHandler<TServiceType, TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TServiceType : INotification // Constraint to ensure it's a notification handler type
        where TImplementationType : TServiceType
        => AddNotificationHandler(typeof(TServiceType), typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed notification handler type.
    /// </summary>
    /// <param name="serviceType">Closed notification handler interface type.</param>
    /// <param name="implementationType">Closed notification handler implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddNotificationHandler(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        NotificationHandlersToRegister.Add(new ServiceDescriptor(serviceType, implementationType, serviceLifetime));
        return this;
    }

    /// <summary>
    /// Register a closed notification handler type against all <see cref="INotificationHandler{TNotification}"/> implementations.
    /// </summary>
    /// <typeparam name="TImplementationType">Closed notification handler implementation type.</typeparam>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddNotificationHandler<TImplementationType>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        where TImplementationType : class // Changed from INotification to class
        => AddNotificationHandler(typeof(TImplementationType), serviceLifetime);

    /// <summary>
    /// Register a closed notification handler type against all <see cref="INotificationHandler{TNotification}"/> implementations.
    /// </summary>
    /// <param name="implementationType">Closed notification handler implementation type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddNotificationHandler(Type implementationType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        var implementedGenericInterfaces = implementationType.FindInterfacesThatClose(typeof(INotificationHandler<>)).ToList();

        if (implementedGenericInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{implementationType.Name} must implement {typeof(INotificationHandler<>).FullName}");
        }

        foreach (var implementedHandlerType in implementedGenericInterfaces)
        {
            NotificationHandlersToRegister.Add(new ServiceDescriptor(implementedHandlerType, implementationType, serviceLifetime));
        }
        return this;
    }

    /// <summary>
    /// Registers an open notification handler type against its open generic interface type.
    /// </summary>
    /// <param name="openHandlerType">An open generic notification handler type.</param>
    /// <param name="serviceLifetime">Optional service lifetime, defaults to <see cref="ServiceLifetime.Transient"/>.</param>
    /// <returns>This</returns>
    public ConcordiaMediatRServiceConfiguration AddOpenNotificationHandler(Type openHandlerType, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        if (!openHandlerType.IsGenericTypeDefinition)
        {
            throw new InvalidOperationException($"{openHandlerType.Name} must be an open generic type definition.");
        }

        var implementedGenericInterfaces = openHandlerType.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
        var implementedOpenHandlerInterfaces = new HashSet<Type>(
            implementedGenericInterfaces.Where(i => i == typeof(INotificationHandler<>)));

        if (implementedOpenHandlerInterfaces.Count == 0)
        {
            throw new InvalidOperationException($"{openHandlerType.Name} must implement {typeof(INotificationHandler<>).FullName}");
        }

        foreach (var openHandlerInterface in implementedOpenHandlerInterfaces)
        {
            NotificationHandlersToRegister.Add(new ServiceDescriptor(openHandlerInterface, openHandlerType, serviceLifetime));
        }

        return this;
    }
}