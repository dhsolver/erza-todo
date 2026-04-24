using Ezra.Api.DTOs;
using Ezra.Api.Models;

namespace Ezra.Api.Services;

public interface ITodoService
{
    Task<PagedTodosResponse> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        bool? isCompleted,
        TodoStatus? status,
        TodoPriority? priority,
        string? search,
        string? sortBy,
        string? sortDir,
        CancellationToken cancellationToken);
    Task<TodoResponse?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<TodoResponse> CreateAsync(Guid userId, CreateTodoRequest request, CancellationToken cancellationToken);
    Task<(bool NotFound, bool Conflict, TodoResponse? Updated)> UpdateAsync(Guid userId, Guid id, UpdateTodoRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
}
