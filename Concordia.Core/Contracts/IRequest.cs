namespace Concordia;

/// <summary>
/// Marker interface for a request.
/// </summary>
public interface IRequest { }

/// <summary>
/// Marker interface for a request that returns a response.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IRequest<out TResponse> : IRequest { }
