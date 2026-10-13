using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Text.RegularExpressions;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [ApiExplorerSettings(GroupName = "Authentication")]
    public class RegistrationController : ControllerBase
    {
        private readonly IAdmissionRepository _admissionRepository;

        public RegistrationController(IAdmissionRepository admissionRepository)
        {
            _admissionRepository = admissionRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] StudentRegistrationRequestModel model)
        {
            if (model == null)
                return BadRequest(new { status = 400, message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(model.StudentNo) ||
                !Regex.IsMatch(model.StudentNo.Trim(), @"^[A-Za-z0-9]{3}-\d{2}-\d{4}-[A-Za-z0-9]{6}$"))
                return BadRequest(new { status = 400, message = "A valid student number is required." });

            if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
                return BadRequest(new { status = 400, message = "First and last name are required." });
            if (model.DateOfBirth == null)
                return BadRequest(new { status = 400, message = "Date of birth is required." });
            if (string.IsNullOrWhiteSpace(model.Gender) ||
                !(model.Gender.Equals("male", StringComparison.OrdinalIgnoreCase) || model.Gender.Equals("female", StringComparison.OrdinalIgnoreCase)))
                return BadRequest(new { status = 400, message = "Choose a valid gender." });
            if (string.IsNullOrWhiteSpace(model.Address) || string.IsNullOrWhiteSpace(model.ContactNumber))
                return BadRequest(new { status = 400, message = "Address and contact number are required." });
            if (string.IsNullOrWhiteSpace(model.Course) ||
                !new[] { "BSIT", "BSA", "BSBA", "BSCS", "BSHM" }.Contains(model.Course.Trim(), StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { status = 400, message = "Choose a valid course." });
            if (string.IsNullOrWhiteSpace(model.Year) || !new[] { "1", "2", "3", "4" }.Contains(model.Year.Trim()))
                return BadRequest(new { status = 400, message = "Choose a year level from 1 to 4." });

            if (string.IsNullOrWhiteSpace(model.Email) ||
                !Regex.IsMatch(model.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest(new { status = 400, message = "A valid email is required." });

            if (string.IsNullOrWhiteSpace(model.Username) || model.Username.Trim().Length < 2)
                return BadRequest(new { status = 400, message = "Username must be at least 2 characters." });

            if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8)
                return BadRequest(new { status = 400, message = "Password must be at least 8 characters." });

            model.StudentNo = model.StudentNo.Trim().ToUpperInvariant();
            model.Email = model.Email.Trim().ToLowerInvariant();
            model.Username = model.Username.Trim();
            model.FirstName = model.FirstName.Trim();
            model.LastName = model.LastName.Trim();
            model.Gender = model.Gender.Trim().ToLowerInvariant();
            model.Address = model.Address.Trim();
            model.ContactNumber = model.ContactNumber.Trim();
            model.Course = model.Course.Trim().ToUpperInvariant();
            model.Year = model.Year.Trim();

            var result = await _admissionRepository.SubmitRegistrationRequest(model);

            return StatusCode(result.Status, new
            {
                status = result.Status,
                message = result.Message,
                data = result.Data
            });
        }
    }
}
