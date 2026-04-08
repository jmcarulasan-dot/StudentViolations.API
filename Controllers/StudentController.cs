using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.Helpers;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Security.Claims;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student,student")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IViolationRepository _violationRepository;
        public StudentController(
            IStudentRepository studentRepository,
            IViolationRepository violationRepository)
        {
            _studentRepository = studentRepository;
            _violationRepository = violationRepository;
        }
        private string GetLoggedInStudentNo()
        {
            var studentNo = User.FindFirstValue("studentNo");
            return string.IsNullOrWhiteSpace(studentNo) ? string.Empty : studentNo;
        }
        // GET api/student/violations
        [HttpGet("violations")]
        public async Task<IActionResult> GetMyViolations()
        {
            string studentNo = GetLoggedInStudentNo();
            if (string.IsNullOrEmpty(studentNo))
                return Unauthorized(new { status = 401, message = "Student number not found in token. Please login again." });

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            var violationsResult = await _violationRepository.GetViolationsByStudentId(studentNo);
            var violations = violationsResult.Data ?? new List<ViolationModel>();

            int pending = violations.Count(v => v.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
            int approved = violations.Count(v => v.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase));
            int rejected = violations.Count(v => v.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase));

            return Ok(new
            {
                status = 200,
                message = "Violations retrieved successfully.",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}",
                    total_violations = violations.Count,
                    pending,
                    approved,
                    rejected,
                    warning_level = ViolationHelper.GetWarningLevel(violations.Count),
                    violations = violations.Select(v => new
                    {
                        id = v.ViolationID,
                        type = v.ViolationName,
                        details = v.Description,
                        severity = v.Severity,
                        date = v.ViolationDate,
                        status = v.Status,
                        recorded_by = v.GuardName
                    })
                }
            });
        }
        // GET api/student/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            string studentNo = GetLoggedInStudentNo();
            if (string.IsNullOrEmpty(studentNo))
                return Unauthorized(new { status = 401, message = "Student number not found in token. Please login again." });

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            var violationsResult = await _violationRepository.GetViolationsByStudentId(studentNo);
            var violations = violationsResult.Data ?? new List<ViolationModel>();

            return Ok(new
            {
                status = 200,
                message = "Profile retrieved successfully.",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}",
                    email = studentResult.Data.Email,
                    gender = studentResult.Data.Gender,
                    course = studentResult.Data.Course,
                    year = studentResult.Data.Year,
                    contact_number = studentResult.Data.ContactNumber,
                    address = studentResult.Data.Address,
                    total_violations = violations.Count,
                    warning_level = ViolationHelper.GetWarningLevel(violations.Count)
                }
            });
        }
        // GET api/student/qrcode
        [HttpGet("qrcode")]
        public async Task<IActionResult> GetMyQrCode()
        {
            string studentNo = GetLoggedInStudentNo();
            if (string.IsNullOrEmpty(studentNo))
                return Unauthorized(new { status = 401, message = "Student number not found in token. Please login again." });

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            if (studentResult.Data.QRCode == null)
                return NotFound(new { status = 404, message = "QR code not found for this student." });

            return Ok(new
            {
                status = 200,
                message = "QR code retrieved successfully.",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}",
                    qr_code = studentResult.Data.QRCode
                }
            });
        }
        // POST api/student/violations/{id}/appeal
        [HttpPost("violations/{id}/appeal")]
        public async Task<IActionResult> SubmitAppeal(int id, [FromBody] string appealText)
        {
            if (string.IsNullOrWhiteSpace(appealText))
                return BadRequest(new { status = 400, message = "Appeal text is required." });

            string studentNo = GetLoggedInStudentNo();
            if (string.IsNullOrEmpty(studentNo))
                return Unauthorized(new { status = 401, message = "Student number not found in token. Please login again." });

            // Check if violation belongs to this student
            var violationsResult = await _violationRepository.GetViolationsByStudentId(studentNo);
            var violations = violationsResult.Data ?? new List<ViolationModel>();
            var violation = violations.FirstOrDefault(v => v.ViolationID == id);

            if (violation == null)
                return NotFound(new { status = 404, message = "Violation not found." });

            if (violation.AppealStatus == "Pending" || violation.AppealStatus == "Approved")
                return BadRequest(new { status = 400, message = "You have already submitted an appeal for this violation." });

            var result = await _violationRepository.SubmitAppeal(id, appealText);
            return StatusCode(result.Status, result);
        }
    }
}