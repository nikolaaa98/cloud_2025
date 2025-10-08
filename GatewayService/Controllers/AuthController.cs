namespace GatewayService.Controllers
{
    using global::GatewayService.Models;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            return Ok(new { Token = "JWT_TOKEN_PLACEHOLDER" });
        }
    }

}
