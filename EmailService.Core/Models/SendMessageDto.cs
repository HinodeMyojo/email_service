namespace EmailService.Core.Models
{
    public class SendMessageDto
    {
        public List<string> To { get; set; } = []; 
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }
}
