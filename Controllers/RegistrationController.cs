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

            if (string.IsNullOrWhiteSpace(model.DateOfBirth) ||
                !DateTime.TryParse(model.DateOfBirth, out _))
                return BadRequest(new { status = 400, message = "A valid date of birth is required." });

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