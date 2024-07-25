using EmailService.Core.Models;
using EmailService_Core.Abstractions;
using EmailService_Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers
{
    public class MailController : ControllerBase
    {
        private ILogger<MailController> _logger;
        private EmailService_Core.Abstractions.IMailService _emailService;
        public MailController(
            ILogger<MailController> logger, EmailService_Core.Abstractions.IMailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        [HttpGet("send")]
        public async Task<IActionResult> SendEmailAsync(
            [FromQuery] SendMessageDto model, CancellationToken cancellationToken)
        {
            await _emailService.SendEmailAsync(model, cancellationToken);
            return Ok();
        }

        [HttpPost("send/files")]
        public async Task<IActionResult> SendEmailWithFiles(
            IFormFileCollection file, [FromForm] SendMessageDto model,CancellationToken cancellationToken)
        {
            await _emailService.SendEmailWithFilesAsync(file, model, cancellationToken);
            return Ok();
        }
    }
}
