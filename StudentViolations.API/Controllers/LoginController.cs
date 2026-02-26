using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Add logging
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Threading.Tasks;
using System.Linq; // Make sure to include this
using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        ILoginRepository loginRepository;
        private readonly ILogger<LoginController> _logger; // Add logger

        public LoginController(ILoginRepository login, ILogger<LoginController> logger)
        {
            loginRepository = login;
            _logger = logger; // Inject logger
        }

        [HttpPost("login")] // Changed route to "login"
        public async Task<IActionResult> LoginStudent([FromBody] LoginModel login) // Use LoginModel
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await loginRepository.GetLogin(login.Username, login.Password); // Await the result

            if (response != null) // Check if login was successful
            {
                return Ok(response); // Return the successful response
            }
            else
            {
                return Unauthorized("Invalid username or password."); // Return Unauthorized if login fails
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if the username or email already exists (using your repository)
            if (await loginRepository.UserExists(model.Username, model.Email))
            {
                return BadRequest(new RegistrationResponseModel {  Message = "Username or email already exists." });
            }

            // Hash the password
            string salt = GenerateSalt();
            string hashedPassword = HashPassword(model.Password, salt);

            // Create a new user entity (Adapt this to your database entity)
            var newUser = new User
            {
                Username = model.Username,
                PasswordHash = hashedPassword, 
                Email = model.Email,
                Gender = model.Gender,
                Number = model.Number,
                Salt = salt 
            };

            try
            {
                // Register the user using your repository
                await loginRepository.RegisterUser(newUser);

                var response = new RegistrationResponseModel { Message = "Registration successful!" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration."); // Log the error
                return StatusCode(500, new RegistrationResponseModel { Message = "Registration failed. Please try again later." }); // Return 500
            }
        }

        // Helper methods for password hashing (replace with a more robust implementation)
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