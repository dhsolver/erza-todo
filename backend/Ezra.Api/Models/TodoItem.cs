namespace Ezra.Api.Models;

public class TodoItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    /// <summary>Incremented on each successful update for optimistic concurrency.</summary>
    public int Version { get; set; }
}
