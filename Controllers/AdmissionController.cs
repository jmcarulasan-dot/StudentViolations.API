using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Text.RegularExpressions;
using System.Security.Claims;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/admission")]
    [Authorize(Roles = "Admission")]
    [ApiExplorerSettings(GroupName = "Admission")]
    public class AdmissionController : ControllerBase
    {
        private readonly IAdmissionRepository _admissionRepository;
        private readonly IAuditTrailRepository _auditTrailRepository;
        private static readonly HashSet<string> AllowedCourses = new(StringComparer.OrdinalIgnoreCase)
        {
            "BSIT", "BSA", "BSBA", "BSCS", "BSHM"
        };
        private static readonly HashSet<string> AllowedYears = new(StringComparer.OrdinalIgnoreCase)
        {
            "1", "2", "3", "4"
        };

        public AdmissionController(IAdmissionRepository admissionRepository, IAuditTrailRepository auditTrailRepository)
        {
            _admissionRepository = admissionRepository;
            _auditTrailRepository = auditTrailRepository;
        }

        private async Task RecordAuditAsync(string action, string entityType, string entityId, string? studentNo, string? newValue, string? remarks)
        {
            await _auditTrailRepository.RecordAsync(new AuditTrailEntry
            {
                Action = action,
                EntityType = entityType,
                EntityID = entityId,
                StudentNo = studentNo,
                NewValue = newValue,
                Remarks = remarks,
                ActorUsername = User.FindFirstValue(ClaimTypes.Name) ?? "unknown",
                ActorRole = User.FindFirstValue(ClaimTypes.Role) ?? "Admission",
                CreatedAtUtc = DateTime.UtcNow
            });
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
                errors["email"] = new[] { "Email address is required." };
            else if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                errors["email"] = new[] { "Enter a valid email address, such as name@example.com." };

            if (!AllowedCourses.Contains(model.Course))
                errors["course"] = new[] { "Choose one of the listed courses: BSIT, BSA, BSBA, BSCS, or BSHM." };

            if (!AllowedYears.Contains(model.Year))
                errors["year"] = new[] { "Choose a year level from 1 to 4." };

            if (errors.Count > 0)
                return BadRequest(new { status = 400, message = "Please correct the highlighted enrollment fields.", errors });

            var result = await _admissionRepository.CreateEnrollment(model);
            if (result.Status == 200)
                await RecordAuditAsync("EnrollmentCreated", "Student", model.StudentNo, model.StudentNo,
                    $"{model.Course} · Year {model.Year}", $"Enrollment record created for {model.FirstName} {model.LastName}.");
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
            if (result.Status == 200)
                await RecordAuditAsync("RegistrationApproved", "RegistrationRequest", requestId.ToString(),
                    result.Data?.StudentNo, result.Data?.Username, "Student account registration approved by Admissions.");
            return StatusCode(result.Status, new { status = result.Status, message = result.Message, data = result.Data });
        }

        [HttpPost("registration-requests/{requestId:int}/reject")]
        public async Task<IActionResult> RejectRegistration(int requestId)
        {
            var result = await _admissionRepository.RejectRegistration(requestId);
            if (result.Status == 200)
                await RecordAuditAsync("RegistrationRejected", "RegistrationRequest", requestId.ToString(),
                    null, null, "Student account registration rejected by Admissions.");
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
            if (result.Status == 200)
                await RecordAuditAsync("ClearanceSigned", "Student", studentNo, studentNo, "Signed", "Student clearance signed by Admissions.");
            return StatusCode(result.Status, new { status = result.Status, message = result.Message, data = result.Data });
        }
    }
}
