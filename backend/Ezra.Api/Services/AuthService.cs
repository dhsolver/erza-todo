using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ezra.Api.Data;
using Ezra.Api.DTOs;
using Ezra.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ezra.Api.Services;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";
    public string Issuer { get; set; } = "Ezra.Api";
    public string Audience { get; set; } = "Ezra.Client";
    public string SigningKey { get; set; } = "please-change-this-long-signing-key";
    public int TokenLifetimeMinutes { get; set; } = 60;
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly AuthOptions _options;

    public AuthService(AppDbContext db, IOptions<AuthOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public async Task<(bool Conflict, AuthResponse? Result)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(x => x.Email == email, cancellationToken).ConfigureAwait(false);
        if (exists)
            return (true, null);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAtUtc = DateTime.UtcNow,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return (false, BuildToken(user.Id));
    }

    public async Task<(AuthResponse? Result, string? Error)> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return (null, "Email does not exist.");

        var ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!ok)
            return (null, "Password is incorrect.");

        return (BuildToken(user.Id), null);
    }

    private AuthResponse BuildToken(Guid userId)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_options.TokenLifetimeMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
