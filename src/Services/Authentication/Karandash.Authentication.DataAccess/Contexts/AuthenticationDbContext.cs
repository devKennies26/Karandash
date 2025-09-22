using Karandash.Authentication.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karandash.Authentication.DataAccess.Contexts;

public class AuthenticationDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthenticationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}