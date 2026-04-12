using Synaptrix;
using Microsoft.Extensions.DependencyInjection;

namespace Synaptrix;

/// <summary>
/// Extension methods for registering Synaptrix core services in the DI container.
/// </summary>
public static class SynaptrixCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers Synaptrix core services (IMediator, ISender, default INotificationPublisher) in the DI container.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddSynaptrixCoreServices(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<ISender, Mediator>();
        services.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();

        return services;
    }

    /// <summary>
    /// Registers Synaptrix core services (IMediator, ISender) and a custom INotificationPublisher.
    /// </summary>
    /// <typeparam name="TNotificationPublisher">The type of the custom INotificationPublisher implementation.</typeparam>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddSynaptrixCoreServices<TNotificationPublisher>(this IServiceCollection services)
        where TNotificationPublisher : class, INotificationPublisher
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<ISender, Mediator>();
        // Registers the custom INotificationPublisher implementation
        services.AddSingleton<INotificationPublisher, TNotificationPublisher>();

        return services;
    }
}
