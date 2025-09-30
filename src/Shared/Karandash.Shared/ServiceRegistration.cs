using System.Text;
using Karandash.Shared.Filters.Language;
using Karandash.Shared.Filters.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Karandash.Shared;

public static class ServiceRegistration
{
    public static IServiceCollection AddAuthenticationService(this IServiceCollection services, string issuer,
        string audience, string secretKey)
    {
        services.AddSwaggerGen(opt =>
        {
            opt.SchemaFilter<EnumSchemaFilter>();

            opt.OperationFilter<AddLanguageHeaderParameter>();
            opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Karandash.API", Version = "v1" });
            opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter the token (without 'bearer' key)",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "bearer"
            });

            opt.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    []
                }
            });
        });

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = issuer,
                ValidAudience = audience,
                LifetimeValidator = (_, expires, token, _) => token is not null ? DateTime.UtcNow < expires : false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });
        services.AddAuthorization();

        return services;
    }
}