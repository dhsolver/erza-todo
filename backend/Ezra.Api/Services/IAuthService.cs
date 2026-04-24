using Ezra.Api.DTOs;

namespace Ezra.Api.Services;

public interface IAuthService
{
    Task<(bool Conflict, AuthResponse? Result)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<(AuthResponse? Result, string? Error)> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
