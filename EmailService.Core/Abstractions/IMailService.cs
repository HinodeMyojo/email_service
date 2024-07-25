using EmailService.Core.Models;
using EmailService_Core.Models;
using Microsoft.AspNetCore.Http;

namespace EmailService_Core.Abstractions
{
    public interface IMailService
    {
        void SendEmail(SendMessageDto model, CancellationToken cancellationToken);
        Task SendEmailAsync(SendMessageDto model, CancellationToken cancellationToken);
        void SendEmailWithFiles(IFormFileCollection file, SendMessageDto model, CancellationToken cancellationToken);
        Task SendEmailWithFilesAsync(IFormFileCollection file, SendMessageDto model, CancellationToken cancellationToken);
    }
}