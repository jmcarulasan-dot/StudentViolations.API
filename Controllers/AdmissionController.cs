using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
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

        public AdmissionController(IAdmissionRepository admissionRepository)
        {
            _admissionRepository = admissionRepository;
        }

        [HttpPost("enrollments")]
        public async Task<IActionResult> CreateEnrollment([FromBody] EnrollmentModel model)
        {
            if (model == null)
                return BadRequest(new { status = 400, message = "Enrollment information is required." });

            model.StudentNo = model.StudentNo?.Trim().ToUpperInvariant() ?? "";
            model.FirstName = model.FirstName?.Trim() ?? "";
            model.LastName = model.LastName?.Trim() ?? "";
            model.Email = model.Email?.Trim().ToLowerInvariant() ?? "";
            model.Course = model.Course?.Trim().ToUpperInvariant() ?? "";
            model.Year = model.Year?.Trim() ?? "";

            var errors = new Dictionary<string, string[]>();
            if (string.IsNullOrWhiteSpace(model.StudentNo))
                errors["studentNo"] = new[] { "Student number is required." };
            else if (!Regex.IsMatch(model.StudentNo, @"^[A-Za-z0-9]{3}-\d{2}-\d{4}-[A-Za-z0-9]{6}$"))
                errors["studentNo"] = new[] { "Use the format ABC-00-0000-ABC123." };

            if (string.IsNullOrWhiteSpace(model.FirstName))
                errors["firstName"] = new[] { "First name is required." };
            if (string.IsNullOrWhiteSpace(model.LastName))
                errors["lastName"] = new[] { "Last name is required." };

            if (string.IsNullOrWhiteSpace(model.Email))
                errors["email"] = new[] { "Gmail address is required." };
            else if (!Regex.IsMatch(model.Email, @"^[^@\s]+@gmail\.com$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                errors["email"] = new[] { "Enter a valid email ending in @gmail.com." };

            if (!new[] { "BSIT", "BSHM", "BSBA" }.Contains(model.Course, StringComparer.OrdinalIgnoreCase))
                errors["course"] = new[] { "Choose BSIT, BSHM, or BSBA." };
            if (!new[] { "1", "2", "3", "4" }.Contains(model.Year, StringComparer.OrdinalIgnoreCase))
                errors["year"] = new[] { "Choose a year level from 1 to 4." };

            if (errors.Count > 0)
                return BadRequest(new { status = 400, message = "Please correct the highlighted enrollment fields.", errors });

            var result = await _admissionRepository.CreateEnrollment(model);
            return StatusCode(result.Status, new { status = result.Status, message = result.Message, data = result.Data });
        }

        [HttpGet("enrollments")]
        public async Task<IActionResult> GetEnrollments()
        {
            var result = await _admissionRepository.GetEnrollments();
            return Ok(new { status = result.Status, message = result.Message, data = result.Data });
        }

        [HttpGet("registration-requests")]
        public async Task<IActionResult> GetRegistrationRequests()
        {
            var result = await _admissionRepository.GetRegistrationRequests();
            return Ok(new { status = result.Status, message = result.Message, data = result.Data });
        }

        [HttpPost("registration-requests/{requestId:int}/approve")]
        public async Task<IActionResult> ApproveRegistration(int requestId)
        {
            var result = await _admissionRepository.ApproveRegistration(requestId);
            return StatusCode(result.Status, new { status = result.Status, message = result.Message, data = result.Data });
        }

        [HttpPost("registration-requests/{requestId:int}/reject")]
        public async Task<IActionResult> RejectRegistration(int requestId)
        {
            var result = await _admissionRepository.RejectRegistration(requestId);
            return StatusCode(result.Status, new { status = result.Status, message = result.Message, data = result.Data });
        }

        [HttpGet("students/{studentNo}/clearance")]
        public async Task<IActionResult> CheckClearance(string studentNo)
        {
            var result = await _admissionRepository.CanSignClearance(studentNo);
            return Ok(new { status = result.Status, message = result.Message, canSign = result.Data });
        }

        [HttpPost("students/{studentNo}/clearance/sign")]
        public async Task<IActionResult> SignClearance(string studentNo)
        {
            var result = await _admissionRepository.SignClearance(studentNo);
            return StatusCode(result.Status, new { status = result.Status, message = result.Message, data = result.Data });
        }
    }
}
