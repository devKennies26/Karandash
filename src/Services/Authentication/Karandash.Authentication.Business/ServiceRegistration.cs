using Karandash.Authentication.Business.Services.Authentication;
using Karandash.Authentication.Business.Services.Utils;
using Karandash.Shared.DTOs;
using Karandash.Shared.Utils.Methods;
using Karandash.Shared.Utils.Template;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Karandash.Authentication.Business;

public static class ServiceRegistration
{
    public static IServiceCollection AddBusinessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));

        services.AddScoped<PasswordHasher>();
        services.AddScoped<AuthenticationService>();
        services.AddScoped<EmailService>();
        services.AddScoped<EmailTemplate>();
        services.AddScoped<TokenHandler>();

        return services;
    }
}