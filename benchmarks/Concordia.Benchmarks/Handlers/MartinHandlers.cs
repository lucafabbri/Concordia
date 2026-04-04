using Concordia.Benchmarks.Requests;

namespace Concordia.Benchmarks.Handlers;

public class MartinQueryHandler : global::Mediator.IRequestHandler<MartinQuery, string>
{
    public ValueTask<string> Handle(MartinQuery request, CancellationToken cancellationToken)
        => ValueTask.FromResult($"pong:{request.Id}");
}

public class MartinCommandHandler : global::Mediator.IRequestHandler<MartinCommand, global::Mediator.Unit>
{
    public ValueTask<global::Mediator.Unit> Handle(MartinCommand request, CancellationToken cancellationToken)
        => default;
}

public class MartinNotificationHandler1 : global::Mediator.INotificationHandler<MartinNotification>
{
    public ValueTask Handle(MartinNotification notification, CancellationToken cancellationToken)
        => default;
}

public class MartinNotificationHandler2 : global::Mediator.INotificationHandler<MartinNotification>
{
    public ValueTask Handle(MartinNotification notification, CancellationToken cancellationToken)
        => default;
}
