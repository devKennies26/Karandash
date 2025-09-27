using Karandash.Shared.DTOs;
using Karandash.Shared.Enums.Language;
using Microsoft.Extensions.Configuration;

namespace Karandash.Shared.Utils.Template;

public class EmailTemplate(IConfiguration configuration, ICurrentUser currentUser)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ICurrentUser _currentUser = currentUser;

    public EmailMessageDto RegisterCompleted(string fullName)
    {
        LanguageCode lang = _currentUser.LanguageCode;

        string title, subject, greeting, callToAction, buttonText, followUs, signature;

        switch (lang)
        {
            case LanguageCode.En:
                title = "Welcome to Karandash!";
                subject = "Your Registration is Complete!";
                greeting = $"Hello {fullName},";
                callToAction = "Click the button below to explore your account:";
                buttonText = "Get Started";
                followUs = "Follow us on social media for updates and news.";
                signature = "Best regards,<br/><b>The Karandash Team</b>";
                break;

            case LanguageCode.Ru:
                title = "Добро пожаловать в Karandash!";
                subject = "Ваша регистрация завершена!";
                greeting = $"Здравствуйте, {fullName},";
                callToAction = "Нажмите кнопку ниже, чтобы войти в ваш аккаунт:";
                buttonText = "Начать";
                followUs = "Подписывайтесь на нас в соцсетях для новостей и обновлений.";
                signature = "С уважением,<br/><b>Команда Karandash</b>";
                break;

            case LanguageCode.Tr:
                title = "Karandash’a Hoş Geldiniz!";
                subject = "Kaydınız Tamamlandı!";
                greeting = $"Merhaba {fullName},";
                callToAction = "Hesabınızı keşfetmek için aşağıdaki butona tıklayın:";
                buttonText = "Başla";
                followUs = "Güncellemeler ve haberler için bizi sosyal medyada takip edin.";
                signature = "Saygılarımızla,<br/><b>Karandash Ekibi</b>";
                break;

            case LanguageCode.Az:
            default:
                title = "Karandash-a xoş gəldiniz!";
                subject = "Qeydiyyatınız tamamlandı!";
                greeting = $"Salam {fullName},";
                callToAction = "Aşağıdakı düyməyə klikləyərək hesabınızı araşdırın:";
                buttonText = "Başla";
                followUs = "Yeniliklər və xəbərlər üçün bizi sosial şəbəkələrdə izləyin.";
                signature = "Hörmətlə,<br/><b>Karandash Komandası</b>";
                break;
        }

        string actionUrl = _configuration["Urls:RegisterCompleted"];

        string content = $@"
        <html>
        <body style='font-family: Arial, sans-serif; background-color: #f7f7f7; margin: 0; padding: 0;'>
            <table width='100%' cellspacing='0' cellpadding='0' style='background-color: #f7f7f7; padding: 20px 0;'>
                <tr>
                    <td align='center'>
                        <table width='600' cellpadding='20' cellspacing='0' 
                               style='background-color: #ffffff; border-radius: 10px; box-shadow: 0 2px 6px rgba(0,0,0,0.1);'>
                            <tr>
                                <td align='center' style='padding-bottom: 0;'>
                                    <h1 style='color: #4CAF50; margin: 0;'>{title}</h1>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <p style='font-size: 16px; color: #333;'>{greeting}</p>
                                    <p style='font-size: 16px; color: #333;'>
                                        {subject}
                                    </p>
                                    <p style='font-size: 16px; color: #333;'>{callToAction}</p>
                                    <div style='text-align: center; margin: 30px 0;'>
                                        <a href='{actionUrl}'
                                           style='background-color: #4CAF50; color: #fff; padding: 12px 25px;
                                                  font-size: 16px; text-decoration: none; border-radius: 6px;
                                                  display: inline-block; font-weight: bold;'>
                                           {buttonText}
                                        </a>
                                    </div>
                                    <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'/>
                                    <p style='font-size: 14px; color: #777;'>{followUs}</p>
                                    <p style='font-size: 14px; color: #777; margin-top: 20px;'>
                                        {signature}
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>";

        return new EmailMessageDto
        {
            Subject = subject,
            Content = content
        };
    }

    public EmailMessageDto PasswordReset(string fullName, string token)
    {
        LanguageCode lang = _currentUser.LanguageCode;

        string title, subject, greeting, callToAction, buttonText, followUs, signature;

        switch (lang)
        {
            case LanguageCode.En:
                title = "Password Reset Request";
                subject = "Reset Your Password";
                greeting = $"Hello {fullName},";
                callToAction = "Click the button below to reset your password. The link will expire in 1 hour.";
                buttonText = "Reset Password";
                followUs = "If you did not request this, please ignore this email.";
                signature = "Best regards,<br/><b>The Karandash Team</b>";
                break;

            case LanguageCode.Ru:
                title = "Запрос на сброс пароля";
                subject = "Сбросьте свой пароль";
                greeting = $"Здравствуйте, {fullName},";
                callToAction = "Нажмите кнопку ниже, чтобы сбросить пароль. Ссылка действительна 1 час.";
                buttonText = "Сбросить пароль";
                followUs = "Если вы не запрашивали это, просто игнорируйте письмо.";
                signature = "С уважением,<br/><b>Команда Karandash</b>";
                break;

            case LanguageCode.Tr:
                title = "Şifre Sıfırlama Talebi";
                subject = "Şifrenizi Sıfırlayın";
                greeting = $"Merhaba {fullName},";
                callToAction = "Şifrenizi sıfırlamak için aşağıdaki butona tıklayın. Link 1 saat boyunca geçerlidir.";
                buttonText = "Şifreyi Sıfırla";
                followUs = "Eğer bu isteği siz yapmadıysanız, bu e-postayı görmezden gelin.";
                signature = "Saygılarımızla,<br/><b>Karandash Ekibi</b>";
                break;

            case LanguageCode.Az:
            default:
                title = "Şifrə Sıfırlama Sorğusu";
                subject = "Şifrənizi Sıfırlayın";
                greeting = $"Salam {fullName},";
                callToAction =
                    "Şifrənizi sıfırlamaq üçün aşağıdakı düyməyə klikləyin. Keçid 1 saat ərzində etibarlıdır.";
                buttonText = "Şifrəni Sıfırla";
                followUs = "Əgər bu əməliyyatı siz etməmisinizsə, xahiş edirik bu məktubu nəzərə almayın.";
                signature = "Hörmətlə,<br/><b>Karandash Komandası</b>";
                break;
        }

        string baseUrl = _configuration["Urls:PasswordReset"];
        string actionUrl = $"{baseUrl}?token={token}";

        string content = $@"
    <html>
    <body style='font-family: Arial, sans-serif; background-color: #f7f7f7; margin: 0; padding: 0;'>
        <table width='100%' cellspacing='0' cellpadding='0' style='background-color: #f7f7f7; padding: 20px 0;'>
            <tr>
                <td align='center'>
                    <table width='600' cellpadding='20' cellspacing='0' 
                           style='background-color: #ffffff; border-radius: 10px; box-shadow: 0 2px 6px rgba(0,0,0,0.1);'>
                        <tr>
                            <td align='center' style='padding-bottom: 0;'>
                                <h1 style='color: #E53935; margin: 0;'>{title}</h1>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <p style='font-size: 16px; color: #333;'>{greeting}</p>
                                <p style='font-size: 16px; color: #333;'>{callToAction}</p>
                                <div style='text-align: center; margin: 30px 0;'>
                                    <a href='{actionUrl}'
                                       style='background-color: #E53935; color: #fff; padding: 12px 25px;
                                              font-size: 16px; text-decoration: none; border-radius: 6px;
                                              display: inline-block; font-weight: bold;'>
                                       {buttonText}
                                    </a>
                                </div>
                                <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;'/>
                                <p style='font-size: 14px; color: #777;'>{followUs}</p>
                                <p style='font-size: 14px; color: #777; margin-top: 20px;'>
                                    {signature}
                                </p>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </body>
    </html>";

        return new EmailMessageDto
        {
            Subject = subject,
            Content = content
        };
    }
}