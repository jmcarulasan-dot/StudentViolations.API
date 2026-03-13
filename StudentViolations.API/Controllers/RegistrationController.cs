using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Data.SqlClient;
using StudentViolationsAPI.Model.Entities;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("")]
    public class RegistrationController : ControllerBase
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IRegisterRepository _registerRepository;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(
            ILogger<RegistrationController> logger,
            ILoginRepository loginRepository,
            IRegisterRepository registerRepository)
        {
            _loginRepository = loginRepository;
            _registerRepository = registerRepository;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (await _loginRepository.UserExists(model.Username, model.Email))
                {
                    return BadRequest(new
                    {
                        status = 0,
                        message = "User already exists with this username or email.",
                        data = (object?)null
                    });
                }

                string salt = GenerateSalt();
                string hashedPassword = HashPassword(model.Password, salt);

                var newUser = new User
                {
                    Username = model.Username,
                    PasswordHash = hashedPassword,
                    Email = model.Email,
                    Gender = model.Gender,
                    ContactNumber = model.Number,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = !string.IsNullOrEmpty(model.DateOfBirth) ? DateTime.Parse(model.DateOfBirth) : (DateTime?)null,
                    Address = model.Address,
                    Salt = salt,
                    RegistrationDate = DateTime.Now,
                    Role = model.Role,
                    Course = model.Course,
                    Year = model.Year
                };

                await _registerRepository.RegisterUser(newUser);

                return Ok(new
                {
                    status = 1,
                    message = "Registration successful!",
                    data = new
                    {
                        id = "",
                        username = model.Username,
                        name = $"{model.FirstName} {model.LastName}",
                        role = model.Role,
                        email = model.Email,
                        contactNumber = model.Number,
                        course = model.Course,
                        year = model.Year,
                    }
                });
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error during registration for user: {Username}", model.Username);
                return StatusCode(500, new { status = 0, message = ex.Message, data = (object?)null });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", model.Username);
                return StatusCode(500, new { status = 0, message = ex.Message, data = (object?)null });
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