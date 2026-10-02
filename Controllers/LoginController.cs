using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [ApiExplorerSettings(GroupName = "Authentication")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository _loginRepository;
        public LoginController(ILoginRepository loginRepository) => _loginRepository = loginRepository;

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            if (request == null)
                return BadRequest(new { status = 400, message = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new { status = 400, message = "Username is required." });
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { status = 400, message = "Password is required." });

            var result = await _loginRepository.Authenticate(request.Username.Trim().ToLowerInvariant(), request.Password);
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });
            return Ok(new { status = result.Status, message = result.Message, data = result.Data });
        }

        [HttpPost("mfa/verify")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> VerifyAuthenticator([FromBody] AuthenticatorCodeRequest request)
        {
            if (request == null)
                return BadRequest(new { status = 400, message = "Request body is required." });
            var result = await _loginRepository.VerifyAuthenticatorCode(request.ChallengeId, request.Code);
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });
            return Ok(new { status = result.Status, message = result.Message, data = result.Data });
        }
    }
}
