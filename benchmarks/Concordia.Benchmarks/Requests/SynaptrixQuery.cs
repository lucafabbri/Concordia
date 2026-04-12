namespace Synaptrix.Benchmarks.Requests;

// --- Query with response ---
public class SynaptrixQuery : IRequest<string>
{
    public int Id { get; set; }
}

// --- Command with no response ---
public class SynaptrixCommand : IRequest
{
    public int Id { get; set; }
}

// --- Notification ---
public class SynaptrixNotification : INotification
{
    public int Id { get; set; }
}
