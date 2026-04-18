using Ezra.Api.Data;
using Ezra.Api.DTOs;
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

        var created = await sut.CreateAsync(new CreateTodoRequest { Title = "Buy milk" }, CancellationToken.None);
        var page = await sut.ListAsync(1, 10, null, CancellationToken.None);

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
        var created = await sut.CreateAsync(new CreateTodoRequest { Title = "A" }, CancellationToken.None);

        var result = await sut.UpdateAsync(
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
        var created = await sut.CreateAsync(new CreateTodoRequest { Title = "A" }, CancellationToken.None);

        var result = await sut.UpdateAsync(
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
        await sut.CreateAsync(new CreateTodoRequest { Title = "Open" }, CancellationToken.None);
        var done = await sut.CreateAsync(new CreateTodoRequest { Title = "Done task" }, CancellationToken.None);
        await sut.UpdateAsync(
            done.Id,
            new UpdateTodoRequest { Title = "Done task", Version = done.Version, IsCompleted = true },
            CancellationToken.None);

        var openOnly = await sut.ListAsync(1, 10, false, CancellationToken.None);
        var completedOnly = await sut.ListAsync(1, 10, true, CancellationToken.None);

        Assert.Equal(1, openOnly.TotalCount);
        Assert.False(openOnly.Items[0].IsCompleted);
        Assert.Equal(1, completedOnly.TotalCount);
        Assert.True(completedOnly.Items[0].IsCompleted);
    }
}
