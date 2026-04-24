using Ezra.Api.DTOs;
using Ezra.Api.Models;
using Ezra.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ezra.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodosController : ControllerBase
{
    private readonly ITodoService _todos;

    public TodosController(ITodoService todos)
    {
        _todos = todos;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedTodosResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedTodosResponse>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] bool? isCompleted = null,
        [FromQuery] TodoStatus? status = null,
        [FromQuery] TodoPriority? priority = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDir = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _todos.ListAsync(
            userId,
            page,
            pageSize,
            isCompleted,
            status,
            priority,
            search,
            sortBy,
            sortDir,
            cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TodoResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var item = await _todos.GetByIdAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (item is null)
        {
            if (await _todos.ExistsAsync(id, cancellationToken).ConfigureAwait(false))
                return Forbid();
            return NotFound();
        }
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoResponse>> Create(
        [FromBody] CreateTodoRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetUserId();
        var created = await _todos.CreateAsync(userId, request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TodoResponse>> Update(
        Guid id,
        [FromBody] UpdateTodoRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = GetUserId();
        var (notFound, conflict, updated) = await _todos.UpdateAsync(userId, id, request, cancellationToken).ConfigureAwait(false);
        if (notFound)
        {
            if (await _todos.ExistsAsync(id, cancellationToken).ConfigureAwait(false))
                return Forbid();
            return NotFound();
        }
        if (conflict)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Concurrency conflict",
                detail: "The todo was modified by another request. Refresh and try again.");
        }

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var deleted = await _todos.DeleteAsync(userId, id, cancellationToken).ConfigureAwait(false);
        if (!deleted)
        {
            if (await _todos.ExistsAsync(id, cancellationToken).ConfigureAwait(false))
                return Forbid();
            return NotFound();
        }
        return NoContent();
    }

    private Guid GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(raw, out var userId))
            throw new UnauthorizedAccessException("Missing or invalid user identifier.");
        return userId;
    }
}
