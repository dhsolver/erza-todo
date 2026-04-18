using Ezra.Api.Data;
using Ezra.Api.DTOs;
using Ezra.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ezra.Api.Services;

public class TodoService : ITodoService
{
    public const int MaxPageSize = 100;
    private readonly AppDbContext _db;

    public TodoService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedTodosResponse> ListAsync(int page, int pageSize, bool? isCompleted, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = _db.Todos.AsNoTracking().AsQueryable();
        if (isCompleted is not null)
            query = query.Where(t => t.IsCompleted == isCompleted);

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var rows = await query
            .OrderByDescending(t => t.UpdatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var items = rows.Select(Map).ToList();

        return new PagedTodosResponse(items, page, pageSize, total);
    }

    public async Task<TodoResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.Todos.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? null : Map(entity);
    }

    public async Task<TodoResponse> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var entity = new TodoItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsCompleted = false,
            DueAtUtc = request.DueAtUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Version = 0,
        };
        _db.Todos.Add(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Map(entity);
    }

    public async Task<(bool NotFound, bool Conflict, TodoResponse? Updated)> UpdateAsync(
        Guid id,
        UpdateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.Todos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            return (true, false, null);

        if (entity.Version != request.Version)
            return (false, true, null);

        entity.Title = request.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.IsCompleted = request.IsCompleted;
        entity.DueAtUtc = request.DueAtUtc;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.Version++;

        try
        {
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException)
        {
            return (false, true, null);
        }

        return (false, false, Map(entity));
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.Todos.FirstOrDefaultAsync(t => t.Id == id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            return false;
        _db.Todos.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    private static TodoResponse Map(TodoItem t) =>
        new(t.Id, t.Title, t.Description, t.IsCompleted, t.DueAtUtc, t.CreatedAtUtc, t.UpdatedAtUtc, t.Version);
}
