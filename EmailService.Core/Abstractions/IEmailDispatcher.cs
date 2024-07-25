using EmailService_Core.Models;
using Microsoft.AspNetCore.Http;

namespace EmailService_Core.Abstractions
{
    public interface IEmailDispatcher
    {
        void SendEmail(Message model);
        Task SendEmailAsync(Message model);
        void SendEmailWithFiles(IFormFileCollection file, Message model, CancellationToken cancellationToken);
        Task SendEmailWithFilesAsync(IFormFileCollection file, Message model, CancellationToken cancellationToken);
    }
}
