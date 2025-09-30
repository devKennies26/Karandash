using System.Security.Claims;
using FluentValidation;
using FluentValidation.AspNetCore;
using Karandash.Authentication.Business;
using Karandash.Authentication.Business.DTOs.Auth;
using Karandash.Authentication.Core.Entities;
using Karandash.Authentication.DataAccess;
using Karandash.Authentication.DataAccess.Contexts;
using Karandash.Shared;
using Karandash.Shared.Middlewares.Exception;
using Karandash.Shared.Middlewares.Language;
using Karandash.Shared.Utils.Methods;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

builder.Services.AddValidatorsFromAssemblyContaining<RegisterDto>()
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLayer(builder.Configuration);

IConfigurationSection jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthenticationService(
    issuer: jwtSection["Issuer"]!,
    audience: jwtSection["Audience"]!,
    secretKey: jwtSection["SigningKey"]!
);

builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, opt =>
{
    ServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
    AuthenticationDbContext dbContext = serviceProvider.GetRequiredService<AuthenticationDbContext>();

    opt.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            string? userId = context.Principal?.FindFirst(ClaimTypes.Sid)?.Value;

            User? user = await dbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

            if (user is null || user.IsDeleted)
                context.Fail(MessageHelper.GetMessage("AccountDeleted"));
        }
    };
});

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<LanguageMiddleware>();
app.UseGlobalExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();