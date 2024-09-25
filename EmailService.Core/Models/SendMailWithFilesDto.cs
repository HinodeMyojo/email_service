using Microsoft.AspNetCore.Http;
namespace EmailService.Core.Models
{
    public class SendMailWithFilesDto
    {
        public required SendMessageDto Message { get; set; }
        public required ICollection<byte[]> Files { get; set; }
    }
}
