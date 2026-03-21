using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Data.SqlClient;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository _loginRepository;
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _configuration;

        public LoginController(
            ILogger<LoginController> logger,
            ILoginRepository loginRepository,
            IConfiguration configuration)
        {
            _loginRepository = loginRepository;
            _logger = logger;
            _configuration = configuration;
        }

        // POST /login
        // Verifies username and password, then returns a JWT token if credentials are correct
        [HttpPost("login")]
        public async Task<IActionResult> LoginStudent([FromBody] LoginModel login)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login attempt for user: {Username}", login.Username);
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Login attempt for user: {Username}", login.Username);

                var response = await _loginRepository.GetLogin(login.Username, login.Password);

                // If login failed, return 401 Unauthorized
                if (response == null || response.Status == 0)
                {
                    _logger.LogWarning("Invalid credentials for user: {Username}", login.Username);
                    return Unauthorized(new { status = 0, message = "Invalid username or password." });
                }

                // Extract user data from the response to build the token
                dynamic userData = response.Data;
                string username = userData.username;
                string role = userData.role;
                string userId = userData.id?.ToString() ?? "";
                string name = userData.name ?? "";

                var token = GenerateToken(userId, username, role, name);

                _logger.LogInformation("Login successful for user: {Username}", login.Username);

                return Ok(new
                {
                    status = 1,
                    message = "Login successful.",
                    data = new
                    {
                        id = userId,
                        username = username,
                        name = name,
                        role = role,
                        token = token,
                        expiresIn = $"{_configuration["JwtSettings:ExpiryInHours"]} hours"
                    }
                });
            }
            catch (SqlException ex)
            {
                // Separate SQL errors from general errors for clearer debugging
                _logger.LogError(ex, "SQL error during login for user: {Username}", login.Username);
                return StatusCode(500, new { status = 0, message = "A database error occurred." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for user: {Username}", login.Username);
                return StatusCode(500, new { status = 0, message = "An unexpected error occurred." });
            }
        }

        // Builds a JWT token containing the user's ID, username, role, and name
        private string GenerateToken(string userId, string username, string role, string name)
        {
            // Read JWT settings from appsettings.json
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expiryHours = int.Parse(jwtSettings["ExpiryInHours"]);

            // Claims are the data stored inside the token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role), // Used by [Authorize(Roles = "...")] 
                new Claim("name", name),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Build the token with claims, expiry, and signing credentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expiryHours),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}