namespace Concordia.Benchmarks.Requests;

// --- Query with response ---
public class MediatRQuery : MediatR.IRequest<string>
{
    public int Id { get; set; }
}

// --- Command with no response ---
public class MediatRCommand : MediatR.IRequest
{
    public int Id { get; set; }
}

// --- Notification ---
public class MediatRNotification : MediatR.INotification
{
    public int Id { get; set; }
}
