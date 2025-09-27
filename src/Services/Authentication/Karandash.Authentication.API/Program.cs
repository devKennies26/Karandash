using FluentValidation;
using FluentValidation.AspNetCore;
using Karandash.Authentication.Business;
using Karandash.Authentication.Business.DTOs.Auth;
using Karandash.Authentication.DataAccess;
using Karandash.Shared;
using Karandash.Shared.Middlewares.Exception;
using Karandash.Shared.Middlewares.Language;
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