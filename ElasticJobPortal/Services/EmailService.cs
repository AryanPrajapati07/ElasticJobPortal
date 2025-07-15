using MailKit.Net.Smtp;
using MimeKit;

namespace ElasticJobPortal.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("aryanprajapati5523@gmail.com"));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            // Fix: Create a BodyBuilder to handle HTML or plain text content
            var bodyBuilder = new BodyBuilder
            {
                TextBody = body, // Plain text body
                HtmlBody = $"<p>{body}</p>" // HTML body
            };
            email.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync("aryanprajapati5523@gmail.com", "bojvatecoqvsxjds");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}