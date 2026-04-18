using System.Net;
using System.Net.Http.Json;
using Ezra.Api.DTOs;
using Xunit;

namespace Ezra.Api.Tests;

public class TodosApiIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TodosApiIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_get_put_delete_roundtrip()
    {
        var createRes = await _client.PostAsJsonAsync(
            "/api/todos",
            new CreateTodoRequest { Title = "Integration", Description = "d" });
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var created = await createRes.Content.ReadFromJsonAsync<TodoResponse>();
        Assert.NotNull(created);

        var getRes = await _client.GetAsync($"/api/todos/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var putRes = await _client.PutAsJsonAsync(
            $"/api/todos/{created.Id}",
            new UpdateTodoRequest
            {
                Title = "Integration updated",
                Description = "d",
                IsCompleted = true,
                Version = created.Version,
            });
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);

        var putConflict = await _client.PutAsJsonAsync(
            $"/api/todos/{created.Id}",
            new UpdateTodoRequest
            {
                Title = "Should fail",
                Version = 0,
                IsCompleted = false,
            });
        Assert.Equal(HttpStatusCode.Conflict, putConflict.StatusCode);

        var del = await _client.DeleteAsync($"/api/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var get404 = await _client.GetAsync($"/api/todos/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get404.StatusCode);
    }

    [Fact]
    public async Task Health_returns_ok()
    {
        var res = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
