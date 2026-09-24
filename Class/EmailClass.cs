using System.Net;
using System.Net.Mail;
using StudentViolations.API.IRepository;

namespace StudentViolations.API.Class
{
    public class EmailClass : IEmailRepository
    {
        private readonly IConfiguration _configuration;

        public EmailClass(IConfiguration configuration) => _configuration = configuration;

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var settings = _configuration.GetSection("EmailSettings");
            var host = settings["Host"];
            var username = settings["Username"];
            var password = settings["Password"];
            var fromEmail = settings["FromEmail"] ?? username;
            var port = int.TryParse(settings["Port"], out var parsedPort) ? parsedPort : 587;
            var enableSsl = bool.TryParse(settings["EnableSsl"], out var ssl) && ssl;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fromEmail))
                return false;

            using var message = new MailMessage(fromEmail, toEmail, subject, htmlBody) { IsBodyHtml = true };
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(username, password)
            };

            await client.SendMailAsync(message);
            return true;
        }
    }
}