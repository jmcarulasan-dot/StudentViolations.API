using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.Model;
using StudentViolations.API.IRepository;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [ApiExplorerSettings(GroupName = "Authentication")]
    public class RegistrationController : ControllerBase
    {
        private readonly ILoginRepository _loginRepository;
        private readonly IRegisterRepository _registerRepository;
        private static readonly string[] ValidRoles = { "guard", "student", "guidance", "sao" };
        private static readonly string[] ValidGenders = { "male", "female" };
        private static readonly string[] ValidCourses = { "bsit", "bshm", "bsba" };
        private static readonly string[] ValidYears = { "1", "2", "3", "4" };
        public RegistrationController(
            ILoginRepository loginRepository,
            IRegisterRepository registerRepository)
        {
            _loginRepository = loginRepository;
            _registerRepository = registerRepository;
        }
        // POST /register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (model == null)
                return BadRequest(new { status = 400, message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(model.Username))
                return BadRequest(new { status = 400, message = "Username is required." });
            if (model.Username.Trim().Length < 2)
                return BadRequest(new { status = 400, message = "Username must be at least 2 characters." });

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { status = 400, message = "Password is required." });
            if (model.Password.Length < 8)
                return BadRequest(new { status = 400, message = "Password must be at least 8 characters." });

            if (string.IsNullOrWhiteSpace(model.Email))
                return BadRequest(new { status = 400, message = "Email is required." });
            if (!Regex.IsMatch(model.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest(new { status = 400, message = "Invalid email format." });

            if (string.IsNullOrWhiteSpace(model.FirstName))
                return BadRequest(new { status = 400, message = "First name is required." });
            if (!Regex.IsMatch(model.FirstName.Trim(), @"^[a-zA-Z\s\-]+$"))
                return BadRequest(new { status = 400, message = "First name must contain letters only." });

            if (string.IsNullOrWhiteSpace(model.LastName))
                return BadRequest(new { status = 400, message = "Last name is required." });
            if (!Regex.IsMatch(model.LastName.Trim(), @"^[a-zA-Z\s\-]+$"))
                return BadRequest(new { status = 400, message = "Last name must contain letters only." });

            if (string.IsNullOrWhiteSpace(model.DateOfBirth))
                return BadRequest(new { status = 400, message = "Date of birth is required." });
            if (!DateTime.TryParse(model.DateOfBirth, out DateTime parsedDob))
                return BadRequest(new { status = 400, message = "Invalid date of birth format. Use YYYY-MM-DD." });
            if (parsedDob >= DateTime.Today)
                return BadRequest(new { status = 400, message = "Date of birth must be in the past." });
            int age = DateTime.Today.Year - parsedDob.Year;
            if (parsedDob > DateTime.Today.AddYears(-age)) age--;
            if (age < 15)
                return BadRequest(new { status = 400, message = "User must be at least 15 years old." });

            if (string.IsNullOrWhiteSpace(model.Gender))
                return BadRequest(new { status = 400, message = "Gender is required." });
            if (!ValidGenders.Contains(model.Gender.Trim().ToLower()))
                return BadRequest(new { status = 400, message = "Gender must be either 'male' or 'female'." });

            if (string.IsNullOrWhiteSpace(model.Address))
                return BadRequest(new { status = 400, message = "Address is required." });

            if (string.IsNullOrWhiteSpace(model.Number))
                return BadRequest(new { status = 400, message = "Contact number is required." });
            if (!Regex.IsMatch(model.Number.Trim(), @"^09\d{9}$"))
                return BadRequest(new { status = 400, message = "Contact number must start with 09 and be exactly 11 digits." });

            if (string.IsNullOrWhiteSpace(model.Role))
                return BadRequest(new { status = 400, message = "Role is required." });
            if (!ValidRoles.Contains(model.Role.Trim().ToLower()))
                return BadRequest(new { status = 400, message = "Role must be one of: guard, student, guidance, sao." });

            model.Username = model.Username.Trim();
            model.Email = model.Email.Trim().ToLower();
            model.FirstName = model.FirstName.Trim();
            model.LastName = model.LastName.Trim();
            model.Gender = model.Gender.Trim().ToLower();
            model.Address = model.Address.Trim();
            model.Number = model.Number.Trim();
            model.Role = model.Role.Trim().ToLower();
            model.StudentNo = model.StudentNo?.Trim().ToUpper();
            model.Course = model.Course?.Trim();
            model.Year = model.Year?.Trim();

            // Student-specific validation
            if (model.Role == "student")
            {
                if (string.IsNullOrWhiteSpace(model.Course))
                    return BadRequest(new { status = 400, message = "Course is required for student role." });
                if (!ValidCourses.Contains(model.Course.Trim().ToLower()))
                    return BadRequest(new { status = 400, message = "Invalid course. Accepted: BSIT, BSHM, BSBA, BSCS, BSA." });
                if (string.IsNullOrWhiteSpace(model.Year))
                    return BadRequest(new { status = 400, message = "Year is required for student role." });
                if (!ValidYears.Contains(model.Year.Trim()))
                    return BadRequest(new { status = 400, message = "Invalid year. Accepted: 1, 2, 3, 4." });
                if (string.IsNullOrWhiteSpace(model.StudentNo))
                    return BadRequest(new { status = 400, message = "Student number is required for student role." });
                if (!Regex.IsMatch(model.StudentNo, @"^[A-Za-z0-9]{3}-\d{2}-\d{4}-[A-Za-z0-9]{6}$"))
                    return BadRequest(new { status = 400, message = "Student number format must be like C26-01-0001-MAN121." });
            }

            if (model.Role != "student" && !string.IsNullOrWhiteSpace(model.StudentNo))
                return BadRequest(new { status = 400, message = "Student number should only be provided for student role." });

            var existsResult = await _loginRepository.UserExists(model.Username, model.Email);
            if (existsResult.Status == 200 && existsResult.Data)
                return BadRequest(new { status = 400, message = "Username or email is already registered." });

            string salt = GenerateSalt();
            string hashedPassword = HashPassword(model.Password, salt);

            var newUser = new UserModel
            {
                Username = model.Username,
                PasswordHash = hashedPassword,
                Salt = salt,
                Email = model.Email,
                Gender = model.Gender,
                ContactNumber = model.Number,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = parsedDob,
                Address = model.Address,
                RegistrationDate = DateTime.Now,
                Role = char.ToUpper(model.Role[0]) + model.Role.Substring(1).ToLower(),
                Course = model.Course?.ToUpper(),
                Year = model.Year,
                StudentNo = model.StudentNo
            };

            var result = await _registerRepository.RegisterUser(newUser);
            return StatusCode(result.Status, result);
        }
        private string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }
        private string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}