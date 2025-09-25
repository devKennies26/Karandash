using FluentValidation;
using FluentValidation.AspNetCore;
using Karandash.Authentication.Business;
using Karandash.Authentication.Business.DTOs.Register;
using Karandash.Authentication.DataAccess;
using Karandash.Shared.Filters.Language;
using Karandash.Shared.Filters.Swagger;
using Karandash.Shared.Middlewares.Exception;
using Karandash.Shared.Middlewares.Language;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

builder.Services.AddValidatorsFromAssemblyContaining<RegisterDto>()
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Karandash, Authentication.API",
        Version = "v1"
    });

    options.SchemaFilter<EnumSchemaFilter>();
    options.OperationFilter<AddLanguageHeaderParameter>();
});

builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLayer();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<LanguageMiddleware>();

app.UseGlobalExceptionHandler();

app.MapControllers();

app.Run();