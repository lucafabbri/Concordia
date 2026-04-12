using Synaptrix.Benchmarks.Requests;

namespace Synaptrix.Benchmarks.Handlers;

public class MediatRQueryHandler : MediatR.IRequestHandler<MediatRQuery, string>
{
    public Task<string> Handle(MediatRQuery request, CancellationToken cancellationToken)
        => Task.FromResult($"pong:{request.Id}");
}

public class MediatRCommandHandler : MediatR.IRequestHandler<MediatRCommand>
{
    public Task Handle(MediatRCommand request, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public class MediatRNotificationHandler1 : MediatR.INotificationHandler<MediatRNotification>
{
    public Task Handle(MediatRNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

public class MediatRNotificationHandler2 : MediatR.INotificationHandler<MediatRNotification>
{
    public Task Handle(MediatRNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}
