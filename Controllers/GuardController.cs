using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.Helpers;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/guard")]
    [Authorize(Roles = "guard,Guard")]
    public class GuardController : ControllerBase
    {
        private readonly IGuardRepository _guardRepository;
        private static readonly string[] ValidSeverities = { "minor", "moderate", "major", "critical" };
        public GuardController(IGuardRepository guardRepository)
        {
            _guardRepository = guardRepository;
        }

        // GET api/guard/student/validate?studentNo=xxx
        [HttpGet("student/validate")]
        public async Task<IActionResult> ValidateStudent([FromQuery] string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            var studentResult = await _guardRepository.GetStudentByQrCode(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            var violationsResult = await _guardRepository.GetViolationsByStudentId(studentNo);
            var violations = violationsResult.Data ?? new List<ViolationModel>();

            return Ok(new
            {
                status = 200,
                message = "Success",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}",
                    course = studentResult.Data.Course,
                    year = studentResult.Data.Year,
                    violation_count = violations.Count,
                    warning_level = ViolationHelper.GetWarningLevel(violations.Count),
                    violations = violations.Select(v => new
                    {
                        date = v.ViolationDate,
                        type = v.ViolationName,
                        details = v.Description,
                        severity = v.Severity,
                        status = v.Status,
                        recorded_by = v.GuardName
                    })
                }
            });
        }

        // POST api/guard/student/violation
        [HttpPost("student/violation")]
        public async Task<IActionResult> RecordViolation([FromBody] RecordViolationModel request)
        {
            if (request == null)
                return BadRequest(new { status = 400, message = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.StudentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });
            if (string.IsNullOrWhiteSpace(request.ViolationType))
                return BadRequest(new { status = 400, message = "Violation type is required." });
            if (string.IsNullOrWhiteSpace(request.Details))
                return BadRequest(new { status = 400, message = "Details are required." });
            if (string.IsNullOrWhiteSpace(request.Severity) ||
                !ValidSeverities.Contains(request.Severity.Trim().ToLower()))
                return BadRequest(new { status = 400, message = "Severity must be: minor, moderate, major, or critical." });

            // Get GuardId from JWT token — never trust the request body for this
            var guardId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(guardId))
                return Unauthorized(new { status = 401, message = "Guard ID not found in token. Please login again." });

            request.StudentNo = request.StudentNo.Trim().ToUpper();
            request.Severity = request.Severity.Trim().ToLower();

            var studentResult = await _guardRepository.GetStudentByQrCode(request.StudentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            var violation = new ViolationModel
            {
                StudentId = studentResult.Data.StudentID,
                ViolationName = request.ViolationType.Trim(),
                Description = request.Details.Trim(),
                Severity = request.Severity,
                GuardId = guardId  // from JWT, not request body
            };

            var recordResult = await _guardRepository.RecordViolation(violation);
            if (recordResult.Status != 200)
                return StatusCode(recordResult.Status, recordResult);

            var violationsResult = await _guardRepository.GetViolationsByStudentId(request.StudentNo);
            var violations = violationsResult.Data ?? new List<ViolationModel>();

            return Ok(new
            {
                status = 200,
                message = "Violation recorded successfully.",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}",
                    new_violation_count = violations.Count,
                    new_warning_level = ViolationHelper.GetWarningLevel(violations.Count)
                }
            });
        }

        // GET api/guard/violations/summary?StartDate=xxx&EndDate=xxx
        [HttpGet("violations/summary")]
        public async Task<IActionResult> GetViolationSummary([FromQuery] GetSummaryModel request)
        {
            if (request.StartDate == default)
                return BadRequest(new { status = 400, message = "Start date is required." });
            if (request.EndDate == default)
                return BadRequest(new { status = 400, message = "End date is required." });
            if (request.StartDate > request.EndDate)
                return BadRequest(new { status = 400, message = "Start date cannot be after end date." });
            if ((request.EndDate - request.StartDate).TotalDays > 365)
                return BadRequest(new { status = 400, message = "Date range cannot exceed 1 year." });

            var result = await _guardRepository.GetViolationsInDateRange(request.StartDate, request.EndDate);
            if (result.Status != 200)
                return StatusCode(result.Status, result);

            var violations = result.Data ?? new List<ViolationModel>();
            if (violations.Count == 0)
                return NotFound(new { status = 404, message = "No violations found in this date range." });

            string topViolation = violations
                .GroupBy(v => v.ViolationName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "N/A";

            return Ok(new
            {
                status = 200,
                message = "Success",
                data = new
                {
                    totalViolations = violations.Count,
                    topViolation,
                    startDate = request.StartDate,
                    endDate = request.EndDate
                }
            });
        }

        // GET api/guard/students
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var result = await _guardRepository.GetAllStudents();
            if (result.Status != 200)
                return StatusCode(result.Status, result);

            var students = result.Data ?? new List<StudentModel>();
            if (students.Count == 0)
                return NotFound(new { status = 404, message = "No students found." });

            return Ok(new
            {
                status = 200,
                message = "Success",
                total = students.Count,
                data = students.Select(s => new
                {
                    student_no = s.StudentNo,
                    name = $"{s.FirstName} {s.LastName}",
                    course = s.Course,
                    year = s.Year
                })
            });
        }

        // GET api/guard/students/exist?studentNo=xxx
        [HttpGet("students/exist")]
        public async Task<IActionResult> GetStudentByStudentNo([FromQuery] string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            var result = await _guardRepository.GetStudentByStudentNo(studentNo);
            if (result.Status != 200)
                return StatusCode(result.Status, result);

            return Ok(new
            {
                status = 200,
                message = "Success",
                data = new
                {
                    student_no = result.Data.StudentNo,
                    name = $"{result.Data.FirstName} {result.Data.LastName}",
                    course = result.Data.Course,
                    year = result.Data.Year
                }
            });
        }
    }
}