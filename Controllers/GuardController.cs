using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.Helpers;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Security.Claims;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/guard")]
    [Authorize(Roles = "guard,Guard")]
    public class GuardController : ControllerBase
    {
        private readonly IGuardRepository _guardRepository;

        // Valid severity values — anything outside this list is rejected
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
                return BadRequest(new { status = 0, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            try
            {
                dynamic student = await _guardRepository.GetStudentByQrCode(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = $"Student '{studentNo}' not found." });

                List<dynamic> violations = await _guardRepository
                    .GetViolationsByStudentId((string)student.StudentNo);

                return Ok(new
                {
                    status = 1,
                    student_no = student.StudentNo,
                    name = $"{student.FirstName} {student.LastName}",
                    course = student.Course,
                    year = student.Year,
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
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // POST api/guard/student/violation
        [HttpPost("student/violation")]
        public async Task<IActionResult> RecordViolation([FromBody] RecordViolationModel request)
        {
            if (request == null)
                return BadRequest(new { status = 0, message = "Request body is required." });

            if (string.IsNullOrWhiteSpace(request.StudentNo))
                return BadRequest(new { status = 0, message = "Student number is required." });

            if (string.IsNullOrWhiteSpace(request.ViolationType))
                return BadRequest(new { status = 0, message = "Violation type is required." });

            if (string.IsNullOrWhiteSpace(request.Details))
                return BadRequest(new { status = 0, message = "Violation details are required." });

            if (string.IsNullOrWhiteSpace(request.Severity) ||
                !ValidSeverities.Contains(request.Severity.Trim().ToLower()))
                return BadRequest(new
                {
                    status = 0,
                    message = $"Invalid severity '{request.Severity}'. Accepted values are: minor, moderate, major, critical."
                });

            request.StudentNo = request.StudentNo.Trim().ToUpper();
            request.Severity = request.Severity.Trim().ToLower();
            request.ViolationType = request.ViolationType.Trim();
            request.Details = request.Details.Trim();

            try
            {
                string? guardId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(guardId))
                    return Unauthorized(new { status = 0, message = "Guard ID not found in token." });

                dynamic student = await _guardRepository.GetStudentByQrCode(request.StudentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = $"Student '{request.StudentNo}' not found." });

                var violation = new
                {
                    StudentId = (int)student.StudentID,
                    Type = request.ViolationType,
                    Details = request.Details,
                    GuardId = guardId,
                    Severity = request.Severity
                };

                await _guardRepository.RecordViolation(violation);

                List<dynamic> violations = await _guardRepository
                    .GetViolationsByStudentId((string)student.StudentNo);

                return Ok(new
                {
                    status = 1,
                    message = "Violation recorded successfully.",
                    student_no = student.StudentNo,
                    name = $"{student.FirstName} {student.LastName}",
                    new_violation_count = violations.Count,
                    new_warning_level = ViolationHelper.GetWarningLevel(violations.Count)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/guard/violations/summary?StartDate=xxx&EndDate=xxx
        // Returns a summary of violations within a date range
        [HttpGet("violations/summary")]
        public async Task<IActionResult> GetViolationSummary([FromQuery] GetSummaryModel request)
        {
            if (request.StartDate == default)
                return BadRequest(new { status = 0, message = "Start date is required." });

            if (request.EndDate == default)
                return BadRequest(new { status = 0, message = "End date is required." });

            if (request.StartDate > request.EndDate)
                return BadRequest(new { status = 0, message = "Start date cannot be after end date." });

            if ((request.EndDate - request.StartDate).TotalDays > 365)
                return BadRequest(new { status = 0, message = "Date range cannot exceed 1 year." });

            try
            {
                List<dynamic> violations = await _guardRepository
                    .GetViolationsInDateRange(request.StartDate, request.EndDate);

                if (violations == null || violations.Count == 0)
                    return NotFound(new
                    {
                        status = 0,
                        message = "No violations found in this date range.",
                        totalViolations = 0,
                        topViolation = "N/A",
                        startDate = request.StartDate,
                        endDate = request.EndDate
                    });

                string topViolation = violations
                    .GroupBy(v => (string)v.ViolationName)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A";

                return Ok(new
                {
                    status = 1,
                    totalViolations = violations.Count,
                    topViolation,
                    startDate = request.StartDate,
                    endDate = request.EndDate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/guard/students
        // Returns all registered students
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            try
            {
                List<dynamic> students = await _guardRepository.GetAllStudents();

                if (students == null || students.Count == 0)
                    return NotFound(new { status = 0, message = "No students found." });

                return Ok(new
                {
                    status = 1,
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
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }
    }
}