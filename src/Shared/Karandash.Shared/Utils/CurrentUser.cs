using System.Globalization;
using System.Security.Claims;
using Karandash.Shared.Enums.Auth;
using Karandash.Shared.Enums.Language;
using Microsoft.AspNetCore.Http;

namespace Karandash.Shared.Utils;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Sid)?.Value;
    public Guid? UserGuid => UserId is not null ? new Guid(UserId) : null;
    public string? UserFullName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

    public string? BaseUrl =>
        $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host.Value}{_httpContextAccessor.HttpContext?.Request.PathBase.Value}";

    public byte UserRole
    {
        get
        {
            string? roleClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

            if (Enum.TryParse(roleClaim, out UserRole role))
                return (byte)role;

            return 14;
        }
    }

    public LanguageCode LanguageCode
    {
        get
        {
            string currentLanguage = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpper();

            if (!Enum.TryParse<LanguageCode>(currentLanguage, true, out LanguageCode languageEnum))
                languageEnum = LanguageCode.Az;

            return languageEnum;
        }
    }
}