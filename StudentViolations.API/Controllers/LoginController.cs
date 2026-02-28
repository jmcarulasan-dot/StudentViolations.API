using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;


namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository _loginRepository;
        private readonly ILogger<LoginController> _logger;
        private readonly string _connectionString;

        public LoginController(ILogger<LoginController> logger, ILoginRepository loginRepository, IConfiguration configuration)
        {
            _loginRepository = loginRepository;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
     
        [HttpPost("login")]
        public async Task<IActionResult> LoginStudent([FromBody] LoginModel login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _loginRepository.GetLogin(login.Username, login.Password);

                if (response != null)
                {
                    return Ok(response);
                }
                else
                {
                    return Unauthorized("Invalid username or password.");
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error during login for user: {Username}", login.Username);
                return StatusCode(500, "An error occurred during login. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for user: {Username}", login.Username);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}