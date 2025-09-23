using System.Globalization;
using Karandash.Shared.Enums.Language;
using Karandash.Shared.Extensions.Language;
using Microsoft.AspNetCore.Http;

namespace Karandash.Shared.Middlewares.Language;

public class LanguageMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    private const string CookieKey = "Language";

    private readonly HashSet<string> _supportedCultures = LanguageCodeExtensions.GetSupportedCultures().ToHashSet();
    private readonly string _defaultCulture = LanguageCode.Az.ToCulture();

    public async Task InvokeAsync(HttpContext context)
    {
        string? language = null;

        if (context.Request.Query.ContainsKey("lang"))
        {
            string queryLang = context.Request.Query["lang"].ToString();
            if (_supportedCultures.Contains(queryLang))
                language = queryLang;
        }

        if (string.IsNullOrEmpty(language) && context.Request.Cookies.ContainsKey(CookieKey))
        {
            string? cookieLang = context.Request.Cookies[CookieKey];
            if (_supportedCultures.Contains(cookieLang))
                language = cookieLang;
        }

        if (string.IsNullOrEmpty(language))
        {
            string? headerLang = context.Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0];
            if (!string.IsNullOrEmpty(headerLang) && _supportedCultures.Contains(headerLang))
                language = headerLang;
        }

        if (string.IsNullOrEmpty(language))
            language = _defaultCulture;

        CultureInfo cultureInfo = new CultureInfo(language);
        CultureInfo.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = cultureInfo;

        await _next(context);
    }
}