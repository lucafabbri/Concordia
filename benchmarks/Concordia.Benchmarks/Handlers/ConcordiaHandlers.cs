using Concordia.Benchmarks.Requests;

namespace Concordia.Benchmarks.Handlers;

public class ConcordiaQueryHandler : IRequestHandler<ConcordiaQuery, string>
{
    public Task<string> Handle(ConcordiaQuery request, CancellationToken cancellationToken)
        => Task.FromResult($"pong:{request.Id}");
}

public class ConcordiaCommandHandler : IRequestHandler<ConcordiaCommand>
{
    public Task Handle(ConcordiaCommand request, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public class ConcordiaNotificationHandler1 : INotificationHandler<ConcordiaNotification>
{
    public Task Handle(ConcordiaNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public class ConcordiaNotificationHandler2 : INotificationHandler<ConcordiaNotification>
{
    public Task Handle(ConcordiaNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
