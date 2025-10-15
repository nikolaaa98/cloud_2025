using global::GatewayService.Models;
using Microsoft.AspNetCore.Mvc;

namespace GatewayService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Prosta autentikacija - prihvata bilo koji username/password
            if (string.IsNullOrEmpty(request.Username))
                return BadRequest("Username is required");

            return Ok(new
            {
                Token = $"jwt-token-{Guid.NewGuid()}",
                User = new { Username = request.Username }
            });
        }
    }
}