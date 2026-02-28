using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

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
                _logger.LogWarning("Invalid login attempt for user: {Username}. Model state errors: {ModelState}", login.Username, ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Login attempt for user: {Username}", login.Username);
                var response = await _loginRepository.GetLogin(login.Username, login.Password);

                if (response != null)
                {
                    _logger.LogInformation("Login successful for user: {Username}", login.Username);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Invalid username or password for user: {Username}", login.Username);
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