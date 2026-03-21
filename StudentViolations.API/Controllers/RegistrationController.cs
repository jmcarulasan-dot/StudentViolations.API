using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

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

        // POST /register
        // Creates a new user account — students also get a StudentNo and QR code saved
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Check if the username or email is already taken before proceeding
                if (await _loginRepository.UserExists(model.Username, model.Email))
                {
                    return BadRequest(new
                    {
                        status = 0,
                        message = "User already exists with this username or email.",
                        data = (object?)null
                    });
                }

                // Generate a unique salt and hash the password before saving
                string salt = GenerateSalt();
                string hashedPassword = HashPassword(model.Password, salt);

                // Build the User object with all registration details
                User newUser = new User
                {
                    Username = model.Username,
                    PasswordHash = hashedPassword,
                    Email = model.Email,
                    Gender = model.Gender,
                    ContactNumber = model.Number,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = !string.IsNullOrEmpty(model.DateOfBirth)
                        ? DateTime.Parse(model.DateOfBirth)
                        : (DateTime?)null,
                    Address = model.Address,
                    Salt = salt,
                    RegistrationDate = DateTime.Now,
                    // Capitalize the first letter of role to keep it consistent (e.g. "student" → "Student")
                    Role = char.ToUpper(model.Role[0]) + model.Role.Substring(1).ToLower(),
                    Course = model.Course,
                    Year = model.Year,
                    StudentNo = model.StudentNo
                };

                ServiceResponse<object> result = await _registerRepository.RegisterUser(newUser);

                return Ok(new
                {
                    status = result.Status,
                    message = result.Message,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", model.Username);
                return StatusCode(500, new { status = 0, message = ex.Message, data = (object?)null });
            }
        }

        // Generates a random 16-byte salt and returns it as a Base64 string
        private string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        // Hashes the password using PBKDF2 with the provided salt — same method used in LoginClass
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