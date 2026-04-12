namespace Synaptrix.Benchmarks.Requests;

// --- Query with response ---
public class MartinQuery : global::Mediator.IRequest<string>
{
    public int Id { get; set; }
}

// --- Command with no response ---
public class MartinCommand : global::Mediator.IRequest
{
    public int Id { get; set; }
}

// --- Notification ---
public class MartinNotification : global::Mediator.INotification
{
    public int Id { get; set; }
}
