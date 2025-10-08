namespace GatewayService.Controllers
{
    using global::GatewayService.Services;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly HttpForwardingService _forwardingService;
        public ChatController(HttpForwardingService service) => _forwardingService = service;

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] string question)
        {
            var answer = await _forwardingService.SendToChatService(question);
            return Ok(answer);
        }
    }

}
