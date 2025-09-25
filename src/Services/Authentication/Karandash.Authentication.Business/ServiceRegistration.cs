using Karandash.Authentication.Business.Services.Authentication;
using Karandash.Authentication.Business.Services.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Karandash.Authentication.Business;

public static class ServiceRegistration
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services)
    {
        services.AddScoped<PasswordHasher>();
        services.AddScoped<AuthenticationService>();

        return services;
    }
}