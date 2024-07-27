using EmailService.Core.Models;
using EmailService_Core.Abstractions;
using EmailService_Core.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

        /// <summary>
        /// Отправка сообщений
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("send")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> SendEmailAsync(
            [FromQuery] SendMessageDto model, CancellationToken cancellationToken)
        {
            await _emailService.SendEmailAsync(model, cancellationToken);
            return Ok();
        }

        /// <summary>
        /// Отправка сообщений с вложенными файлами
        /// </summary>
        /// <param name="file"></param>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("send/files")]
        public async Task<IActionResult> SendEmailWithFiles(
            IFormFileCollection file, [FromForm] SendMessageDto model,CancellationToken cancellationToken)
        {
            await _emailService.SendEmailWithFilesAsync(file, model, cancellationToken);
            return Ok();
        }
        
        //TODO Добавить возможность отложенной отправки сообщений
        //TODO Добавить возможность ...
    }
}
