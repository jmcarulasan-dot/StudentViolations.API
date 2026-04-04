using Microsoft.AspNetCore.Mvc;
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

        public LoginController(ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        // POST /login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            if (request == null)
                return BadRequest(new { status = 400, message = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest(new { status = 400, message = "Username is required." });
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { status = 400, message = "Password is required." });

            request.Username = request.Username.Trim().ToLower();
            request.Password = request.Password.Trim();

            var result = await _loginRepository.Authenticate(request.Username, request.Password);

            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            return Ok(new
            {
                status = 200,
                message = "Login successful.",
                role = result.Data.Role,
                token = result.Token
            });
        }
    }
}