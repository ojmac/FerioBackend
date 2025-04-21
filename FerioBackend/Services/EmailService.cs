using System;
using System.Net;
using System.Net.Mail;
using FerioBackend.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace FerioBackend.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;

            _smtpServer = _configuration["Smtp:Host"];
            _smtpUser = _configuration["Smtp:User"];
            _smtpPassword = _configuration["Smtp:Pass"];

            if (!int.TryParse(_configuration["Smtp:Port"], out _smtpPort))
            {
                throw new ArgumentException("La configuración de SmtpPort no es válida en appsettings.json.");
            }
        }


        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser, "Ferio App"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUser, _smtpPassword),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correo: {ex.Message}");
                throw;
            }
        }
    }
}
