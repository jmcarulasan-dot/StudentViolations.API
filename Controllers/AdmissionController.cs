using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/admission")]
    [Authorize(Roles = "Admission")]
    [ApiExplorerSettings(GroupName = "Admission")]
    public class AdmissionController : ControllerBase
    {
        private readonly IAdmissionRepository _admissionRepository;

        private static readonly string[] ValidGenders = { "male", "female" };
        private static readonly string[] ValidCourses = { "bsit", "bshm", "bsba", "bscs", "bsa" };
        private static readonly string[] ValidYears = { "1", "2", "3", "4" };

        public AdmissionController(IAdmissionRepository admissionRepository)
        {
            _admissionRepository = admissionRepository;
        }

        // POST api/admission/students/register
        // Admission registers the student account and the API generates/stores the student's QR code.
        [HttpPost("students/register")]
        public async Task<IActionResult> RegisterStudent([FromBody] RegistrationModel model)
        {
            if (model == null)
                return BadRequest(new { status = 400, message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(model.Username) || model.Username.Trim().Length < 2)
                return BadRequest(new { status = 400, message = "Username must be at least 2 characters." });

            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8)
                return BadRequest(new { status = 400, message = "Password must be at least 8 characters." });

            if (string.IsNullOrWhiteSpace(model.Email) ||
                !Regex.IsMatch(model.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest(new { status = 400, message = "A valid email is required." });

            if (string.IsNullOrWhiteSpace(model.FirstName) ||
                !Regex.IsMatch(model.FirstName.Trim(), @"^[a-zA-Z\s\-]+$"))
                return BadRequest(new { status = 400, message = "First name is required and must contain letters only." });

            if (string.IsNullOrWhiteSpace(model.LastName) ||
                !Regex.IsMatch(model.LastName.Trim(), @"^[a-zA-Z\s\-]+$"))
                return BadRequest(new { status = 400, message = "Last name is required and must contain letters only." });

            if (!DateTime.TryParse(model.DateOfBirth, out DateTime parsedDob) || parsedDob >= DateTime.Today)
                return BadRequest(new { status = 400, message = "A valid past date of birth is required." });

            int age = DateTime.Today.Year - parsedDob.Year;
            if (parsedDob > DateTime.Today.AddYears(-age)) age--;
            if (age < 15)
                return BadRequest(new { status = 400, message = "Student must be at least 15 years old." });

            if (string.IsNullOrWhiteSpace(model.Gender) ||
                !ValidGenders.Contains(model.Gender.Trim().ToLowerInvariant()))
                return BadRequest(new { status = 400, message = "Gender must be either male or female." });

            if (string.IsNullOrWhiteSpace(model.Address))
                return BadRequest(new { status = 400, message = "Address is required." });

            if (string.IsNullOrWhiteSpace(model.Number) ||
                !Regex.IsMatch(model.Number.Trim(), @"^09\d{9}$"))
                return BadRequest(new { status = 400, message = "Contact number must start with 09 and be exactly 11 digits." });

            if (string.IsNullOrWhiteSpace(model.Course) ||
                !ValidCourses.Contains(model.Course.Trim().ToLowerInvariant()))
                return BadRequest(new { status = 400, message = "Invalid course. Accepted: BSIT, BSHM, BSBA, BSCS, BSA." });

            if (string.IsNullOrWhiteSpace(model.Year) || !ValidYears.Contains(model.Year.Trim()))
                return BadRequest(new { status = 400, message = "Invalid year. Accepted: 1, 2, 3, 4." });

            if (string.IsNullOrWhiteSpace(model.StudentNo) ||
                !Regex.IsMatch(model.StudentNo.Trim(), @"^[A-Za-z0-9]{3}-\d{2}-\d{4}-[A-Za-z0-9]{6}$"))
                return BadRequest(new { status = 400, message = "Student number format must be like C26-01-0001-MAN121." });

            model.Username = model.Username.Trim();
            model.Email = model.Email.Trim().ToLowerInvariant();
            model.FirstName = model.FirstName.Trim();
            model.LastName = model.LastName.Trim();
            model.Gender = model.Gender.Trim().ToLowerInvariant();
            model.Address = model.Address.Trim();
            model.Number = model.Number.Trim();
            model.Course = model.Course.Trim().ToUpperInvariant();
            model.Year = model.Year.Trim();
            model.StudentNo = model.StudentNo.Trim().ToUpperInvariant();

            string salt = GenerateSalt();
            string passwordHash = HashPassword(model.Password, salt);

            var user = new UserModel
            {
                Username = model.Username,
                PasswordHash = passwordHash,
                Salt = salt,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = parsedDob,
                Gender = model.Gender,
                Address = model.Address,
                ContactNumber = model.Number,
                RegistrationDate = DateTime.Now,
                Role = "Student",
                Course = model.Course,
                Year = model.Year,
                StudentNo = model.StudentNo
            };

            var result = await _admissionRepository.RegisterStudent(user);

            return StatusCode(result.Status, new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }

        private static string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return Convert.ToBase64String(salt);
        }

        private static string HashPassword(string password, string salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password,
                Convert.FromBase64String(salt),
                KeyDerivationPrf.HMACSHA256,
                10000,
                32));
        }
    }
}