namespace GatewayService.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using System.Threading.Tasks;
    using global::GatewayService.Services;

    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly HttpForwardingService _forwardingService;

        public DocumentController(HttpForwardingService forwardingService)
        {
            _forwardingService = forwardingService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            var result = await _forwardingService.SendToDocumentService(file);
            return Ok(result);
        }
    }

}
