using Synaptrix.Benchmarks.Requests;

namespace Synaptrix.Benchmarks.Handlers;

public class SynaptrixNotificationHandler1 : INotificationHandler<SynaptrixNotification>
{
    public ValueTask Handle(SynaptrixNotification notification, CancellationToken cancellationToken)
        => default;
}
