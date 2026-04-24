using System.ComponentModel.DataAnnotations;
using Ezra.Api.Models;

namespace Ezra.Api.DTOs;

public record TodoResponse(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    TodoStatus Status,
    TodoPriority Priority,
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

    [EnumDataType(typeof(TodoStatus))]
    public TodoStatus? Status { get; set; }

    [EnumDataType(typeof(TodoPriority))]
    public TodoPriority? Priority { get; set; }

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

    [EnumDataType(typeof(TodoStatus))]
    public TodoStatus? Status { get; set; }

    [EnumDataType(typeof(TodoPriority))]
    public TodoPriority? Priority { get; set; }

    public DateTime? DueAtUtc { get; set; }

    /// <summary>Must match the current server version or the update returns 409.</summary>
    [Range(0, int.MaxValue)]
    public int Version { get; set; }
}

public record PagedTodosResponse(IReadOnlyList<TodoResponse> Items, int Page, int PageSize, int TotalCount);
