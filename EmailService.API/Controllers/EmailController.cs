using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers
{
    public class EmailController : ControllerBase
    {
        private ILogger<EmailController> _logger;
        public EmailController(ILogger<EmailController> logger)
        {
            _logger = logger;
        }

        [HttpGet("send")]
        public async Task<IActionResult> SendEmail()
        {

        }
    }
}
