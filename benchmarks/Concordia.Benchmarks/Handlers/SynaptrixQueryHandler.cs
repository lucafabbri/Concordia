using Synaptrix.Benchmarks.Requests;

namespace Synaptrix.Benchmarks.Handlers;

public class SynaptrixQueryHandler : IRequestHandler<SynaptrixQuery, string>
{
    public ValueTask<string> Handle(SynaptrixQuery request, CancellationToken cancellationToken)
        => new ValueTask<string>($"pong:{request.Id}");
}
