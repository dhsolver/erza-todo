using Ezra.Api.DTOs;
using Ezra.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ezra.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var (conflict, result) = await _auth.RegisterAsync(request, cancellationToken).ConfigureAwait(false);
        if (conflict)
            return Conflict(new ProblemDetails { Title = "Email already registered" });
        return Created(string.Empty, result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var (result, error) = await _auth.LoginAsync(request, cancellationToken).ConfigureAwait(false);
        if (result is null)
            return Unauthorized(new ProblemDetails { Title = "Login failed", Detail = error ?? "Invalid credentials." });
        return Ok(result);
    }
}
