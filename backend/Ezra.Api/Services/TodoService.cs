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

    public async Task<PagedTodosResponse> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        bool? isCompleted,
        TodoStatus? status,
        TodoPriority? priority,
        string? search,
        string? sortBy,
        string? sortDir,
        CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = _db.Todos.AsNoTracking().Where(t => t.UserId == userId).AsQueryable();
        if (isCompleted is not null)
            query = query.Where(t => t.IsCompleted == isCompleted);
        if (status is not null)
            query = query.Where(t => t.Status == status);
        if (priority is not null)
            query = query.Where(t => t.Priority == priority);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLowerInvariant();
            query = query.Where(t =>
                t.Title.ToLower().Contains(q) ||
                (t.Description != null && t.Description.ToLower().Contains(q)));
        }

        var desc = !string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
        query = ApplySorting(query, sortBy, desc);

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var rows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var items = rows.Select(Map).ToList();

        return new PagedTodosResponse(items, page, pageSize, total);
    }

    public async Task<TodoResponse?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.Todos.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken)
            .ConfigureAwait(false);
        return entity is null ? null : Map(entity);
    }

    public async Task<TodoResponse> CreateAsync(Guid userId, CreateTodoRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var entity = new TodoItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsCompleted = false,
            Status = request.Status ?? TodoStatus.Todo,
            Priority = request.Priority ?? TodoPriority.Medium,
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
        Guid userId,
        Guid id,
        UpdateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            return (true, false, null);

        if (entity.Version != request.Version)
            return (false, true, null);

        entity.Title = request.Title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.IsCompleted = request.IsCompleted;
        entity.Status = request.Status ?? entity.Status;
        entity.Priority = request.Priority ?? entity.Priority;
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

    public async Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var entity = await _db.Todos.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            return false;
        _db.Todos.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return true;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        => _db.Todos.AsNoTracking().AnyAsync(t => t.Id == id, cancellationToken);

    private static TodoResponse Map(TodoItem t) =>
        new(t.Id, t.Title, t.Description, t.IsCompleted, t.Status, t.Priority, t.DueAtUtc, t.CreatedAtUtc, t.UpdatedAtUtc, t.Version);

    private static IQueryable<TodoItem> ApplySorting(IQueryable<TodoItem> query, string? sortBy, bool desc)
    {
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => desc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "createdatutc" => desc ? query.OrderByDescending(t => t.CreatedAtUtc) : query.OrderBy(t => t.CreatedAtUtc),
            "dueatutc" => desc ? query.OrderByDescending(t => t.DueAtUtc) : query.OrderBy(t => t.DueAtUtc),
            "priority" => desc ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "status" => desc ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            _ => desc ? query.OrderByDescending(t => t.UpdatedAtUtc) : query.OrderBy(t => t.UpdatedAtUtc),
        };
    }
}
