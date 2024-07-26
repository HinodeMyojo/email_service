using EmailService.Core.CustomExceptions;
using EmailService_Core.Abstractions;
using EmailService_Core.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EmailService_Core.Infrastructure
{
    public class EmailDispatcher : IEmailDispatcher
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ILogger<EmailDispatcher> _logger;

        public EmailDispatcher(EmailConfiguration emailConfig, ILogger<EmailDispatcher> logger)
        {
            _emailConfig = emailConfig;
            _logger = logger;
        }

        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            Send(emailMessage);
        }

        public async Task SendEmailAsync(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            await SendAsync(emailMessage);
        }

        public void SendEmailWithFiles(
            IFormFileCollection file, Message model, CancellationToken cancellationToken)
        {
            var emailMessage = CreateEmailMessage(model, file);
            Send(emailMessage);
        }

        public async Task SendEmailWithFilesAsync(
            IFormFileCollection file, Message model, CancellationToken cancellationToken)
        {
            var emailMessage = CreateEmailMessage(model, file);
            await SendAsync(emailMessage);
        }

        #region privateMethods
        private void Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_emailConfig.UserName, _emailConfig.Password);

                    client.Send(mailMessage);
                }
                catch
                {
                    throw new NotConnectionException();
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }

        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

                    await client.SendAsync(mailMessage);
                }
                catch
                {
                    //log an error message or throw an exception or both.
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }

        private MimeMessage CreateEmailMessage(Message message, IFormFileCollection? files = default)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("email", _emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            if (files != null)
            {
                var bodyBuilder = new BodyBuilder { HtmlBody = string.Format("<h2 style='color:red;'>{0}</h2>", message.Content) };
                byte[] fileBytes;
                foreach (var file in files)
                {
                    using(var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }

                    bodyBuilder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    emailMessage.Body = bodyBuilder.ToMessageBody();
                }
                return emailMessage;
            }

            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };
            return emailMessage;
        }
        #endregion
    }
}
