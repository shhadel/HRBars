using HRBars.Application.Interfaces;
using HRBars.Domain.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace HRBars.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;

        public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task SendCredentialsAsync(string email, string fullName, string password)
        {
            try
            {
                using var client = new SmtpClient();

                var socketOptions = GetSecureSocketOptions(_settings.SmtpPort, _settings.EnableSsl);

                await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, socketOptions);

                await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("HRBars", _settings.SenderEmail));
                message.To.Add(new MailboxAddress(fullName, email));
                message.Subject = "Данные для входа в систему HRBars";

                var body = $@"
Здравствуйте, {fullName}!

Вам создана учётная запись в системе HRBars.

📧 Логин: {email}
🔑 Пароль: {password}

🔗 Ссылка для входа: http://localhost:5017

---
С уважением,
Команда БАРС Груп
";

                message.Body = new TextPart("plain") { Text = body };

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Письмо отправлено на {Email} через {Server}:{Port}",
                    email, _settings.SmtpServer, _settings.SmtpPort);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке письма на {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Автоматический выбор SecureSocketOptions
        /// </summary>
        private SecureSocketOptions GetSecureSocketOptions(int port, bool enableSsl)
        {
            return port switch
            {
                465 => SecureSocketOptions.SslOnConnect,      // SSL
                587 => SecureSocketOptions.StartTls,           // STARTTLS
                25 => SecureSocketOptions.StartTls,            // SMTP
                _ => enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto
            };
        }
    }
}