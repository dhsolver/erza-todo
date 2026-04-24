using Ezra.Api.Data;
using Ezra.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=ezra-todos.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var auth = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
var envSigningKey = builder.Configuration["AUTH_SIGNING_KEY"];
if (!string.IsNullOrWhiteSpace(envSigningKey))
    auth.SigningKey = envSigningKey;
if (string.IsNullOrWhiteSpace(auth.SigningKey))
{
    throw new InvalidOperationException(
        "JWT signing key is missing. Set AUTH_SIGNING_KEY or Auth:SigningKey in configuration.");
}
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.SigningKey));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = auth.Issuer,
            ValidAudience = auth.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
}

await app.RunAsync().ConfigureAwait(false);

public partial class Program;
