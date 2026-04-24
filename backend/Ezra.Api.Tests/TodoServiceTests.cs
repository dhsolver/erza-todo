using Ezra.Api.Data;
using Ezra.Api.DTOs;
using Ezra.Api.Models;
using Ezra.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ezra.Api.Tests;

public class TodoServiceTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Create_then_list_contains_item()
    {
        await using var db = CreateDb();
        var sut = new TodoService(db);
        var userId = Guid.NewGuid();

        var created = await sut.CreateAsync(userId, new CreateTodoRequest { Title = "Buy milk" }, CancellationToken.None);
        var page = await sut.ListAsync(userId, 1, 10, null, null, null, null, null, null, CancellationToken.None);

        Assert.Single(page.Items);
        Assert.Equal("Buy milk", page.Items[0].Title);
        Assert.Equal(created.Id, page.Items[0].Id);
        Assert.Equal(0, page.Items[0].Version);
    }

    [Fact]
    public async Task Update_with_wrong_version_returns_conflict()
    {
        await using var db = CreateDb();
        var sut = new TodoService(db);
        var userId = Guid.NewGuid();
        var created = await sut.CreateAsync(userId, new CreateTodoRequest { Title = "A" }, CancellationToken.None);

        var result = await sut.UpdateAsync(
            userId,
            created.Id,
            new UpdateTodoRequest { Title = "B", Version = 999, IsCompleted = false },
            CancellationToken.None);

        Assert.False(result.NotFound);
        Assert.True(result.Conflict);
        Assert.Null(result.Updated);
    }

    [Fact]
    public async Task Update_with_correct_version_succeeds_and_increments_version()
    {
        await using var db = CreateDb();
        var sut = new TodoService(db);
        var userId = Guid.NewGuid();
        var created = await sut.CreateAsync(userId, new CreateTodoRequest { Title = "A" }, CancellationToken.None);

        var result = await sut.UpdateAsync(
            userId,
            created.Id,
            new UpdateTodoRequest { Title = "B", Version = created.Version, IsCompleted = true },
            CancellationToken.None);

        Assert.False(result.NotFound);
        Assert.False(result.Conflict);
        Assert.NotNull(result.Updated);
        Assert.Equal("B", result.Updated.Title);
        Assert.True(result.Updated.IsCompleted);
        Assert.Equal(1, result.Updated.Version);
    }

    [Fact]
    public async Task List_respects_completed_filter_and_paging()
    {
        await using var db = CreateDb();
        var sut = new TodoService(db);
        var userId = Guid.NewGuid();
        await sut.CreateAsync(userId, new CreateTodoRequest { Title = "Open" }, CancellationToken.None);
        var done = await sut.CreateAsync(userId, new CreateTodoRequest { Title = "Done task" }, CancellationToken.None);
        await sut.UpdateAsync(
            userId,
            done.Id,
            new UpdateTodoRequest { Title = "Done task", Version = done.Version, IsCompleted = true },
            CancellationToken.None);

        var openOnly = await sut.ListAsync(userId, 1, 10, false, null, null, null, null, null, CancellationToken.None);
        var completedOnly = await sut.ListAsync(userId, 1, 10, true, null, null, null, null, null, CancellationToken.None);

        Assert.Equal(1, openOnly.TotalCount);
        Assert.False(openOnly.Items[0].IsCompleted);
        Assert.Equal(1, completedOnly.TotalCount);
        Assert.True(completedOnly.Items[0].IsCompleted);
    }

    [Fact]
    public async Task List_only_returns_items_for_given_user()
    {
        await using var db = CreateDb();
        var sut = new TodoService(db);
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        await sut.CreateAsync(userA, new CreateTodoRequest { Title = "A1" }, CancellationToken.None);
        await sut.CreateAsync(userB, new CreateTodoRequest { Title = "B1" }, CancellationToken.None);

        var listA = await sut.ListAsync(userA, 1, 10, null, null, null, null, null, null, CancellationToken.None);
        var listB = await sut.ListAsync(userB, 1, 10, null, null, null, null, null, null, CancellationToken.None);

        Assert.Single(listA.Items);
        Assert.Equal("A1", listA.Items[0].Title);
        Assert.Single(listB.Items);
        Assert.Equal("B1", listB.Items[0].Title);
    }

    [Fact]
    public async Task List_supports_status_priority_search_and_sorting()
    {
        await using var db = CreateDb();
        var sut = new TodoService(db);
        var userId = Guid.NewGuid();

        await sut.CreateAsync(userId, new CreateTodoRequest
        {
            Title = "Alpha",
            Status = TodoStatus.Todo,
            Priority = TodoPriority.Low,
        }, CancellationToken.None);
        await sut.CreateAsync(userId, new CreateTodoRequest
        {
            Title = "Beta",
            Status = TodoStatus.InProgress,
            Priority = TodoPriority.High,
        }, CancellationToken.None);

        var filtered = await sut.ListAsync(
            userId,
            1,
            10,
            null,
            TodoStatus.InProgress,
            TodoPriority.High,
            "bet",
            "title",
            "asc",
            CancellationToken.None);

        Assert.Single(filtered.Items);
        Assert.Equal("Beta", filtered.Items[0].Title);
        Assert.Equal(TodoStatus.InProgress, filtered.Items[0].Status);
        Assert.Equal(TodoPriority.High, filtered.Items[0].Priority);
    }
}
