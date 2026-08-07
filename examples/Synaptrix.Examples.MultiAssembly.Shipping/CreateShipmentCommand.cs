namespace Synaptrix.Examples.MultiAssembly.Shipping;

/// <summary>
/// A command handled entirely within this assembly - deliberately unrelated to Catalog's
/// commands, to demonstrate that a request from a sibling assembly stays dispatchable through
/// IMediator no matter which assembly's GeneratedMediator ends up bound to it.
/// </summary>
public class CreateShipmentCommand : IRequest<string>
{
    public int OrderId { get; set; }
}

public class CreateShipmentCommandHandler : IRequestHandler<CreateShipmentCommand, string>
{
    public ValueTask<string> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
        => new($"SHIP-{request.OrderId}");
}
