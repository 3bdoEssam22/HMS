using Hms.Services.Abstraction;
using HMS.Shared.Messages;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Infrastructure.ExternalService
{
    public class EmailService(IOptions<EmailSettings> _options) : IEmailService
    {
        public async Task SendEmailAsync(Email email)
        {
            //Combine message
            var mail = new MimeMessage()
            {
                Sender = MailboxAddress.Parse(_options.Value.Email),
                Subject = email.Subject
            };

            mail.To.Add(MailboxAddress.Parse(email.To));
            mail.From.Add(new MailboxAddress(_options.Value.DisplayName, _options.Value.Email));

            var builder = new BodyBuilder();

            builder.TextBody = email.Body;

            mail.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_options.Value.Host, _options.Value.Port, SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(_options.Value.Email, _options.Value.Password);

            await smtp.SendAsync(mail);

            await smtp.DisconnectAsync(true);


        }
    }
}
