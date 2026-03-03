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
using StudentViolationsAPI.Model.Entities;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("")]
    public class RegistrationController : ControllerBase
    {
        private readonly ILoginRepository _loginRepository;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(ILogger<RegistrationController> logger, ILoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
            _logger = logger;
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
                    return BadRequest("User already exists with this username or email.");
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

                var response = new { success = true, message = "Registration successful!" };
                return Ok(response);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error during registration for user: {Username}, Email: {Email}", model.Username, model.Email);
                return StatusCode(500, "An error occurred during registration. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Successful during registration for user: {Username}, Email: {Email}", model.Username, model.Email);
                return StatusCode(200, "Login successful.");
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
}