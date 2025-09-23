using System.Globalization;
using System.Resources;
using Karandash.Shared.Resources;

namespace Karandash.Shared.Utils.Methods
{
    public static class MessageHelper
    {
        private static readonly ResourceManager _resourceManager;

        static MessageHelper()
        {
            _resourceManager = new ResourceManager("Karandash.Shared.Resources.Messages", typeof(Messages).Assembly);
        }

        public static string GetMessage(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return GetGeneralErrorMessage();

            try
            {
                string? message = _resourceManager.GetString(key, CultureInfo.CurrentUICulture);

                if (string.IsNullOrWhiteSpace(message))
                    return GetGeneralErrorMessage();

                return message;
            }
            catch
            {
                return GetGeneralErrorMessage();
            }
        }

        public static string GetMessage(string key, params object[] parameters)
        {
            string message = GetMessage(key);

            if (string.IsNullOrWhiteSpace(message) || parameters == null || parameters.Length == 0)
                return message;

            try
            {
                return string.Format(CultureInfo.CurrentUICulture, message, parameters);
            }
            catch
            {
                return GetGeneralErrorMessage();
            }
        }

        private static string GetGeneralErrorMessage()
        {
            try
            {
                string? errorMessage =
                    _resourceManager.GetString("GeneralMessageErrorOccured", CultureInfo.CurrentUICulture);

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName?.ToLower() switch
                    {
                        "az" => "A system error occurred",
                        "ru" => "Произошла системная ошибка",
                        "tr" => "Sistem hatası oluştu",
                        _ => "Sistem xətası baş verdi"
                    };
                }

                return errorMessage;
            }
            catch
            {
                return "Sistem xətası baş verdi";
            }
        }
    }
}