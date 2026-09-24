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
            if (model == null || string.IsNullOrWhiteSpace(model.StudentNo))
                return BadRequest(new { status = 400, message = "Enrollment information is required." });

            model.StudentNo = model.StudentNo.Trim().ToUpperInvariant();
            model.FirstName = model.FirstName?.Trim();
            model.LastName = model.LastName?.Trim();
            model.Email = model.Email?.Trim().ToLowerInvariant();
            model.Gender = model.Gender?.Trim().ToLowerInvariant();
            model.Course = model.Course?.Trim().ToUpperInvariant();
            model.Year = model.Year?.Trim();

            if (!Regex.IsMatch(model.StudentNo, @"^[A-Za-z0-9]{3}-\d{2}-\d{4}-[A-Za-z0-9]{6}$"))
                return BadRequest(new { status = 400, message = "Invalid student number format." });

            if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName) ||
                string.IsNullOrWhiteSpace(model.Email) || model.DateOfBirth == null ||
                string.IsNullOrWhiteSpace(model.Course) || string.IsNullOrWhiteSpace(model.Year))
                return BadRequest(new { status = 400, message = "Complete enrollment information is required." });

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