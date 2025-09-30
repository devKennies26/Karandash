using System.Net;
using System.Net.Mail;
using Karandash.Shared.DTOs;
using Karandash.Shared.Utils.Template;
using Microsoft.Extensions.Options;

namespace Karandash.Shared.Utils.Methods;

public class EmailService(IOptions<SmtpSettings> smtpSettings, EmailTemplate emailTemplate)
{
    private readonly SmtpSettings _smtpSettings = smtpSettings.Value;
    private readonly EmailTemplate _emailTemplate = emailTemplate;

    public void SendRegistrationEmail(string toEmail, string fullName)
    {
        EmailMessageDto emailMessage = _emailTemplate.RegisterCompleted(fullName);
        SendEmail(toEmail, new EmailMessageDto()
        {
            Subject = emailMessage.Subject,
            Content = emailMessage.Content
        });
    }

    public void SendPasswordResetEmail(string toEmail, string fullName, string token)
    {
        EmailMessageDto emailMessage = _emailTemplate.PasswordReset(fullName, token);
        SendEmail(toEmail, emailMessage);
    }

    public void SendAccountDeactivationEmail(string toEmail, string fullName)
    {
        EmailMessageDto emailMessage = _emailTemplate.AccountDeactivated(fullName);
        SendEmail(toEmail, new EmailMessageDto
        {
            Subject = emailMessage.Subject,
            Content = emailMessage.Content
        });
    }

    private void SendEmail(string toEmail, EmailMessageDto emailMessage)
    {
        MailAddress fromAddress = new MailAddress(_smtpSettings.Username, _smtpSettings.SenderName);
        MailAddress toAddress = new MailAddress(toEmail);

        SmtpClient smtpClient = new SmtpClient()
        {
            Host = _smtpSettings.Server,
            Port = _smtpSettings.Port,
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromAddress.Address, _smtpSettings.Password)
        };

        using MailMessage message = new MailMessage(fromAddress, toAddress)
        {
            Subject = emailMessage.Subject,
            IsBodyHtml = true,
            Body = emailMessage.Content,
        };
        smtpClient.Send(message);
    }
}