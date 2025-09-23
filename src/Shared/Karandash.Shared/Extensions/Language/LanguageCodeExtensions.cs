using Karandash.Shared.Enums.Language;

namespace Karandash.Shared.Extensions.Language;

public static class LanguageCodeExtensions
{
    public static string ToCulture(this LanguageCode code) => code switch
    {
        LanguageCode.Az => "az",
        LanguageCode.Tr => "tr",
        LanguageCode.En => "en",
        LanguageCode.Ru => "ru",
        _ => "az"
    };

    public static IEnumerable<string> GetSupportedCultures()
    {
        return Enum.GetValues(typeof(LanguageCode))
            .Cast<LanguageCode>()
            .Select(c => c.ToCulture());
    }
}