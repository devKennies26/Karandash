using Karandash.Authentication.Core.Entities;
using Karandash.Shared.Utils.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Karandash.Authentication.DataAccess.Contexts;

public class AuthenticationDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<OutboxEvent> OutboxEvents { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<PasswordToken> PasswordTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthenticationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}