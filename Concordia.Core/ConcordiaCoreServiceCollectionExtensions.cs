using Concordia;
using Microsoft.Extensions.DependencyInjection;

namespace Concordia;

/// <summary>
/// Extension methods for registering Concordia core services in the DI container.
/// </summary>
public static class ConcordiaCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers Concordia core services (IMediator, ISender, default INotificationPublisher) in the DI container.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddConcordiaCoreServices(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddTransient<IMediator, Mediator>();
        services.AddTransient<ISender, Mediator>();
        services.AddSingleton<INotificationPublisher, ForeachAwaitPublisher>();

        return services;
    }

    /// <summary>
    /// Registers Concordia core services (IMediator, ISender) and a custom INotificationPublisher.
    /// </summary>
    /// <typeparam name="TNotificationPublisher">The type of the custom INotificationPublisher implementation.</typeparam>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddConcordiaCoreServices<TNotificationPublisher>(this IServiceCollection services)
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
