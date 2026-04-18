using Ezra.Api.DTOs;

namespace Ezra.Api.Services;

public interface ITodoService
{
    Task<PagedTodosResponse> ListAsync(int page, int pageSize, bool? isCompleted, CancellationToken cancellationToken);
    Task<TodoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<TodoResponse> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken);
    Task<(bool NotFound, bool Conflict, TodoResponse? Updated)> UpdateAsync(Guid id, UpdateTodoRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
