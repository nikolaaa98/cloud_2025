using Microsoft.AspNetCore.Mvc;

namespace GatewayService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHealth()
        {
            var healthStatus = new
            {
                GatewayService = "Running ✅",
                DocumentService = "Integrated in GatewayService ✅",
                ChatService = "Integrated in GatewayService ✅",
                Timestamp = DateTime.UtcNow,
                Note = "All services running in GatewayService"
            };

            return Ok(healthStatus);
        }
    }
}