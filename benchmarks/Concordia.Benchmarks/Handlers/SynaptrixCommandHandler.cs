using Synaptrix.Benchmarks.Requests;

namespace Synaptrix.Benchmarks.Handlers;

public class SynaptrixCommandHandler : IRequestHandler<SynaptrixCommand>
{
    public ValueTask Handle(SynaptrixCommand request, CancellationToken cancellationToken)
        => default;
}
