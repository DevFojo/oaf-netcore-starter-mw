using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using Application.Common.DTOs;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Http
{
    public class SMTPEmailClient : ISendEmails
    {
        private readonly SmtpClient _smtpClient;
        private readonly IConfiguration _config;

        private readonly ILogger<SMTPEmailClient> _logger;

        public SMTPEmailClient(IConfiguration config, ILogger<SMTPEmailClient> logger)
        {
            _logger = logger;
            _config = config;
            _smtpClient = new SmtpClient("smtp.gmail.com") {
                Port = int.Parse(_config["DefaultEmailAccount:Port"]),
                Credentials = new NetworkCredential(_config["DefaultEmailAccount:Email"], _config["DefaultEmailAccount:Password"]),
                EnableSsl = bool.Parse(_config["DefaultEmailAccount:EnableSsl"]),
            };

        }

        public Task SendEmail(EmailDto emailDto)
        {
            _smtpClient.Send(emailDto.SenderEmail, emailDto.RecipientEmail, emailDto.Subject, emailDto.Body);
            _logger.LogInformation("Sent Email to {Recepient}", emailDto.RecipientEmail);
            return Task.CompletedTask;
        }
    }
}