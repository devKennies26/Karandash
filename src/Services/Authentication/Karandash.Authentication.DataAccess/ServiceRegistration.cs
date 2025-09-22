using Karandash.Authentication.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karandash.Authentication.DataAccess;

public static class ServiceRegistration
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuthenticationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("AuthDbConnection"));
        });

        return services;
    }
}