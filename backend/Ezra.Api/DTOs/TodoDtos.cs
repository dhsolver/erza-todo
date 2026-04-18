using System.ComponentModel.DataAnnotations;

namespace Ezra.Api.DTOs;

public record TodoResponse(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTime? DueAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    int Version);

public class CreateTodoRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    public DateTime? DueAtUtc { get; set; }
}

public class UpdateTodoRequest
{
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? DueAtUtc { get; set; }

    /// <summary>Must match the current server version or the update returns 409.</summary>
    [Range(0, int.MaxValue)]
    public int Version { get; set; }
}

public record PagedTodosResponse(IReadOnlyList<TodoResponse> Items, int Page, int PageSize, int TotalCount);
