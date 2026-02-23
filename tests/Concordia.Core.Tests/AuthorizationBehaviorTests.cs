using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Concordia;
using Concordia.Behaviors;

namespace Concordia.Core.Tests;

/// <summary>
/// Tests for <see cref="AuthorizationBehavior{TRequest,TResponse}"/> that verify
/// permission enforcement in the request pipeline.
/// </summary>
public class AuthorizationBehaviorTests
{
    // --- Test requests ---

    /// <summary>
    /// A request that requires the "products:write" permission.
    /// </summary>
    public class SecuredRequest : IRequest<string>, IRequirePermission
    {
        /// <inheritdoc/>
        public string RequiredPermission => "products:write";
    }

    /// <summary>
    /// A request that does NOT require any permission.
    /// </summary>
    public class PublicRequest : IRequest<string>
    {
    }

    // --- Test handlers ---

    /// <summary>
    /// Handler for <see cref="SecuredRequest"/>.
    /// </summary>
    public class SecuredRequestHandler : IRequestHandler<SecuredRequest, string>
    {
        /// <inheritdoc/>
        public Task<string> Handle(SecuredRequest request, CancellationToken cancellationToken)
            => Task.FromResult("secured-ok");
    }

    /// <summary>
    /// Handler for <see cref="PublicRequest"/>.
    /// </summary>
    public class PublicRequestHandler : IRequestHandler<PublicRequest, string>
    {
        /// <inheritdoc/>
        public Task<string> Handle(PublicRequest request, CancellationToken cancellationToken)
            => Task.FromResult("public-ok");
    }

    // --- Stub ICurrentUserService implementations ---

    /// <summary>
    /// Simulates an authenticated user who has the required permission.
    /// </summary>
    private sealed class AuthorizedUserService : ICurrentUserService
    {
        public string? UserId => "user-123";
        public bool HasPermission(string permission) => permission == "products:write";
    }

    /// <summary>
    /// Simulates an authenticated user who does NOT have the required permission.
    /// </summary>
    private sealed class UnauthorizedUserService : ICurrentUserService
    {
        public string? UserId => "user-456";
        public bool HasPermission(string permission) => false;
    }

    /// <summary>
    /// Simulates a user who is not authenticated (UserId is null).
    /// </summary>
    private sealed class AnonymousUserService : ICurrentUserService
    {
        public string? UserId => null;
        public bool HasPermission(string permission) => false;
    }

    // --- Helpers ---

    private static IServiceProvider BuildServices(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddConcordiaCoreServices();
        services.AddTransient<IRequestHandler<SecuredRequest, string>, SecuredRequestHandler>();
        services.AddTransient<IRequestHandler<PublicRequest, string>, PublicRequestHandler>();
        services.AddTransient<IPipelineBehavior<SecuredRequest, string>, AuthorizationBehavior<SecuredRequest, string>>();
        services.AddTransient<IPipelineBehavior<PublicRequest, string>, AuthorizationBehavior<PublicRequest, string>>();
        configure(services);
        return services.BuildServiceProvider();
    }

    // --- Tests ---

    /// <summary>
    /// A request that requires a permission passes through when the user holds it.
    /// </summary>
    [Fact]
    public async Task AuthorizedUser_WithRequiredPermission_Succeeds()
    {
        var sp = BuildServices(s => s.AddSingleton<ICurrentUserService, AuthorizedUserService>());
        var sender = sp.GetRequiredService<ISender>();

        var result = await sender.Send(new SecuredRequest());

        Assert.Equal("secured-ok", result);
    }

    /// <summary>
    /// A request that requires a permission is rejected when the user lacks it.
    /// </summary>
    [Fact]
    public async Task UnauthorizedUser_WithoutRequiredPermission_ThrowsUnauthorizedAccessException()
    {
        var sp = BuildServices(s => s.AddSingleton<ICurrentUserService, UnauthorizedUserService>());
        var sender = sp.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sender.Send(new SecuredRequest()));
    }

    /// <summary>
    /// An anonymous (unauthenticated) user cannot execute a permission-protected request.
    /// </summary>
    [Fact]
    public async Task AnonymousUser_OnSecuredRequest_ThrowsUnauthorizedAccessException()
    {
        var sp = BuildServices(s => s.AddSingleton<ICurrentUserService, AnonymousUserService>());
        var sender = sp.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sender.Send(new SecuredRequest()));
    }

    /// <summary>
    /// A request that does not implement <see cref="IRequirePermission"/> is always allowed,
    /// even when no <see cref="ICurrentUserService"/> is registered.
    /// </summary>
    [Fact]
    public async Task PublicRequest_WithoutCurrentUserService_Succeeds()
    {
        var sp = BuildServices(_ => { /* no ICurrentUserService registered */ });
        var sender = sp.GetRequiredService<ISender>();

        var result = await sender.Send(new PublicRequest());

        Assert.Equal("public-ok", result);
    }

    /// <summary>
    /// A permission-protected request raises <see cref="UnauthorizedAccessException"/> when
    /// no <see cref="ICurrentUserService"/> is registered at all.
    /// </summary>
    [Fact]
    public async Task SecuredRequest_WithoutCurrentUserService_ThrowsUnauthorizedAccessException()
    {
        var sp = BuildServices(_ => { /* no ICurrentUserService registered */ });
        var sender = sp.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sender.Send(new SecuredRequest()));
    }
}
