using System.ComponentModel.DataAnnotations;

namespace Ezra.Api.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(string Token, DateTime ExpiresAtUtc);
