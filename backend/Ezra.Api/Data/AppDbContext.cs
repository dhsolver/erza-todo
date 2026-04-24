using Ezra.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Ezra.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> Todos => Set<TodoItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<TodoItem>();
        e.HasKey(x => x.Id);
        e.Property(x => x.Title).IsRequired().HasMaxLength(500);
        e.Property(x => x.Description).HasMaxLength(4000);
        e.Property(x => x.Version).IsConcurrencyToken();
        e.Property(x => x.UserId).IsRequired();
        e.Property(x => x.Status).HasConversion<int>().IsRequired();
        e.Property(x => x.Priority).HasConversion<int>().IsRequired();
        e.HasIndex(x => x.IsCompleted);
        e.HasIndex(x => x.DueAtUtc);
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.Status);
        e.HasIndex(x => x.Priority);

        var u = modelBuilder.Entity<User>();
        u.HasKey(x => x.Id);
        u.Property(x => x.Email).IsRequired().HasMaxLength(320);
        u.HasIndex(x => x.Email).IsUnique();
        u.Property(x => x.PasswordHash).IsRequired().HasMaxLength(500);
    }
}
