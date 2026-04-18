using Ezra.Api.DTOs;
using Ezra.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ezra.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        CancellationToken cancellationToken = default)
    {
        var result = await _todos.ListAsync(page, pageSize, isCompleted, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var item = await _todos.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        if (item is null)
            return NotFound();
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

        var created = await _todos.CreateAsync(request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TodoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TodoResponse>> Update(
        Guid id,
        [FromBody] UpdateTodoRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var (notFound, conflict, updated) = await _todos.UpdateAsync(id, request, cancellationToken).ConfigureAwait(false);
        if (notFound)
            return NotFound();
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
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _todos.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
        if (!deleted)
            return NotFound();
        return NoContent();
    }
}
