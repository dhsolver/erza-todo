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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var e = modelBuilder.Entity<TodoItem>();
        e.HasKey(x => x.Id);
        e.Property(x => x.Title).IsRequired().HasMaxLength(500);
        e.Property(x => x.Description).HasMaxLength(4000);
        e.Property(x => x.Version).IsConcurrencyToken();
        e.HasIndex(x => x.IsCompleted);
        e.HasIndex(x => x.DueAtUtc);
    }
}
