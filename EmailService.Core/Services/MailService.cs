using EmailService.Core.Models;
using EmailService_Core.Abstractions;
using EmailService_Core.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.Text;

namespace EmailService.Core.Services
{
    public class MailService : IMailService
    {
        private readonly IEmailDispatcher _dispatcher;

        public MailService(IEmailDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public async Task SendEmailAsync(SendMessageDto model, CancellationToken cancellationToken)
        {

            Message message =  MessageChecker(model);

            await _dispatcher.SendEmailAsync(message);
        }

        public async Task SendEmailWithFilesAsync(IFormFileCollection file, SendMessageDto model, CancellationToken cancellationToken)
        {
            Message message = MessageChecker(model);

            await _dispatcher.SendEmailWithFilesAsync(file, message, cancellationToken);
        }

        public void SendEmail(SendMessageDto model, CancellationToken cancellationToken)
        {
            Message message = MessageChecker(model);

            _dispatcher.SendEmail(message);
        }

        public void SendEmailWithFiles(IFormFileCollection file, SendMessageDto model, CancellationToken cancellationToken)
        {
            Message message = MessageChecker(model);

            _dispatcher.SendEmailWithFiles(file, message, cancellationToken);
        }

        private static Message MessageChecker(SendMessageDto model)
        {

            Message message;

            if (model != null)
            {
                List<string> inValidEmails = [];
                foreach (string email in model.To)
                {
                    if (!MailAddress.TryCreate(email, out var mailAddress))
                        inValidEmails.Add(email);
                }
                if (inValidEmails.Any())
                {
                    StringBuilder exceptionBuilder = new StringBuilder();
                    foreach (var item in inValidEmails)
                    {
                        exceptionBuilder.AppendLine(item);
                    }
                    // TODO send true exceptions (нужно чтобы каждый неправильный email был на новой строке)
                    throw new FormatException($"Письмо не отправлено! Следующие email неверны: {exceptionBuilder}");
                }
                message = new(model.To, model.Subject ?? "", model.Content ?? "");
            }
            else
            {
                throw new FormatException("Нельзя отправить письмо, которое не содержит ни получателя, ни темы, ни содержания!");
            }

            return message;

            //Message message;
            //if (model.Subject != null)
            //{
            //    if (model.Content == null)
            //    {
            //        model.Content = String.Empty;
            //    }
            //    message = new(model.To, model.Subject, model.Content);
            //}
            //else
            //{
            //    throw new Exception("Не указана тема письма");
            //}

            //return message;
        }
    }
}
