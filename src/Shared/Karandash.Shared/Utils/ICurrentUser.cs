using Karandash.Shared.Enums.Language;

namespace Karandash.Shared.Utils;

public interface ICurrentUser
{
    public string? UserId { get; }
    public Guid? UserGuid { get; }
    public string? UserFullName { get; }
    public string? BaseUrl { get; }
    public byte UserRole { get;}
    public LanguageCode LanguageCode { get; }
}