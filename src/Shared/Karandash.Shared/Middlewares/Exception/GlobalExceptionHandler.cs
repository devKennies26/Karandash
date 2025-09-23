using System.Net.Mime;
using System.Text.Json;
using Karandash.Shared.Exceptions.Base;
using Karandash.Shared.Utils.Methods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Karandash.Shared.Middlewares.Exception;

public static class GlobalExceptionHandler
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(handlerApp =>
        {
            handlerApp.Run(async context =>
            {
                context.Response.ContentType = MediaTypeNames.Application.Json;

                IExceptionHandlerFeature? feature = context.Features.Get<IExceptionHandlerFeature>();

                if (feature?.Error is IUserFriendlyException ex)
                {
                    context.Response.StatusCode = ex.StatusCode;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        StatusCode = ex.StatusCode,
                        Message = ex.Message
                    }));
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        Message = MessageHelper.GetMessage("AnUnhandledException")
                    }));
                }
            });
        });
    }
}