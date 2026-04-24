using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ezra.Api.DTOs;
using Ezra.Api.Models;
using Xunit;

namespace Ezra.Api.Tests;

public class TodosApiIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TodosApiIntegrationTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetTokenAsync(string email, string password)
    {
        var registerRes = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            Email = email,
            Password = password,
        });
        Assert.Equal(HttpStatusCode.Created, registerRes.StatusCode);
        var auth = await registerRes.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        return auth!.Token;
    }

    [Fact]
    public async Task Todos_endpoint_requires_auth()
    {
        var res = await _client.GetAsync("/api/todos");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Register_login_and_todo_roundtrip_with_authorization()
    {
        var email = $"user-{Guid.NewGuid()}@example.com";
        var password = "Password123!";
        var token = await RegisterAndGetTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var createRes = await _client.PostAsJsonAsync(
            "/api/todos",
            new { title = "Integration", description = "d", status = 1, priority = 2 });
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var createdJson = await createRes.Content.ReadAsStringAsync();
        using var createdDoc = JsonDocument.Parse(createdJson);
        var createdId = createdDoc.RootElement.GetProperty("id").GetGuid();
        var createdVersion = createdDoc.RootElement.GetProperty("version").GetInt32();

        var getRes = await _client.GetAsync($"/api/todos/{createdId}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var putRes = await _client.PutAsJsonAsync(
            $"/api/todos/{createdId}",
            new
            {
                title = "Integration updated",
                description = "d",
                isCompleted = true,
                status = 2,
                priority = 1,
                version = createdVersion,
            });
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);

        var putConflict = await _client.PutAsJsonAsync(
            $"/api/todos/{createdId}",
            new UpdateTodoRequest
            {
                Title = "Should fail",
                Version = 0,
                IsCompleted = false,
            });
        Assert.Equal(HttpStatusCode.Conflict, putConflict.StatusCode);

        var del = await _client.DeleteAsync($"/api/todos/{createdId}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var get404 = await _client.GetAsync($"/api/todos/{createdId}");
        Assert.Equal(HttpStatusCode.NotFound, get404.StatusCode);
    }

    [Fact]
    public async Task Users_cannot_access_other_users_todos()
    {
        var user1Token = await RegisterAndGetTokenAsync($"u1-{Guid.NewGuid()}@example.com", "Password123!");
        var user2Token = await RegisterAndGetTokenAsync($"u2-{Guid.NewGuid()}@example.com", "Password123!");

        _client.DefaultRequestHeaders.Authorization = new("Bearer", user1Token);
        var createRes = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest { Title = "Private task" });
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var createdJson = await createRes.Content.ReadAsStringAsync();
        using var createdDoc = JsonDocument.Parse(createdJson);
        var createdId = createdDoc.RootElement.GetProperty("id").GetGuid();

        _client.DefaultRequestHeaders.Authorization = new("Bearer", user2Token);
        var getRes = await _client.GetAsync($"/api/todos/{createdId}");
        Assert.Equal(HttpStatusCode.Forbidden, getRes.StatusCode);

        var deleteRes = await _client.DeleteAsync($"/api/todos/{createdId}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteRes.StatusCode);
    }

    [Fact]
    public async Task Register_duplicate_email_returns_conflict()
    {
        var email = $"dup-{Guid.NewGuid()}@example.com";
        var payload = new RegisterRequest { Email = email, Password = "Password123!" };

        var first = await _client.PostAsJsonAsync("/api/auth/register", payload);
        var second = await _client.PostAsJsonAsync("/api/auth/register", payload);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Login_invalid_password_returns_unauthorized()
    {
        var email = $"login-{Guid.NewGuid()}@example.com";
        await RegisterAndGetTokenAsync(email, "Password123!");

        var res = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Email = email,
            Password = "WrongPass123!",
        });
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Create_invalid_payload_returns_bad_request()
    {
        var token = await RegisterAndGetTokenAsync($"invalid-{Guid.NewGuid()}@example.com", "Password123!");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var res = await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest
        {
            Title = "",
        });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task List_supports_query_filters_and_sorting()
    {
        var token = await RegisterAndGetTokenAsync($"query-{Guid.NewGuid()}@example.com", "Password123!");
        _client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        await _client.PostAsJsonAsync("/api/todos", new { title = "Alpha", status = 0, priority = 0 });
        await _client.PostAsJsonAsync("/api/todos", new { title = "Beta", status = 1, priority = 2 });

        var listRes = await _client.GetAsync("/api/todos?status=1&priority=2&search=bet&sortBy=title&sortDir=asc&page=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);
        var json = await listRes.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var items = doc.RootElement.GetProperty("items");
        Assert.Equal(1, items.GetArrayLength());
        Assert.Equal("Beta", items[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task Health_returns_ok()
    {
        var res = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
