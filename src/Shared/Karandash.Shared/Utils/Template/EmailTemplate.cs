using Karandash.Shared.DTOs;
using Microsoft.Extensions.Configuration;

namespace Karandash.Shared.Utils.Template;

public class EmailTemplate(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    /* TODO: Current User hissəsi yazıldıqdan sonra template user'in dilinə görə tərcümə olunmalıdır! Və təbii ki, formatı da düzənlənməlidir. */
    public EmailMessageDto RegisterCompleted(string fullName)
    {
        string title = "Welcome to Karandash!";
        string subject = "Your Registration is Complete!";
        string greeting = $"Hello {fullName},";
        string callToAction = "Click the button below to explore your account:";
        string buttonText = "Get Started";
        string followUs = "Follow us on social media for updates and news.";

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
                                        Thank you for registering with <b>Karandash</b>. We're excited to have you on board!
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
                                        Best regards,<br/>
                                        <b>The Karandash Team</b>
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