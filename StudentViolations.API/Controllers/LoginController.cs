using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Data.SqlClient; 
using Microsoft.Extensions.Configuration; 

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository _loginRepository; // Changed to private readonly
        private readonly ILogger<LoginController> _logger;
        private readonly string _connectionString; // Add this

        public LoginController(ILogger<LoginController> logger, ILoginRepository loginRepository, IConfiguration configuration) // Modified constructor
        {
            _loginRepository = loginRepository;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection"); // Add this
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                if (await _loginRepository.UserExists(model.Username, model.Email))
                {
                    return BadRequest(new RegistrationResponseModel { Message = "Username or email already exists." });
                }

                string salt = GenerateSalt();
                string hashedPassword = HashPassword(model.Password, salt);

                var newUser = new User
                {
                    Username = model.Username,
                    PasswordHash = hashedPassword,
                    Email = model.Email,
                    Gender = model.Gender,
                    Number = model.Number,
                    Salt = salt
                };

                await _loginRepository.RegisterUser(newUser);

                var response = new RegistrationResponseModel { Message = "Registration successful!" };
                return Ok(response);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error during registration for user: {Username}, Email: {Email}", model.Username, model.Email);
                return StatusCode(500, "An error occurred during registration. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for user: {Username}, Email: {Email}", model.Username, model.Email);
                return StatusCode(500, "Registration failed. Please try again later.");
            }
        }

        private string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        private string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
    }

    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Number { get; set; }
        public string Salt { get; set; }
    }
}