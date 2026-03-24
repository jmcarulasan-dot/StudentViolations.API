using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        // Day 11 — Finds a student by QR code and returns warning level + full violation list with dates
        [HttpGet("student/validate")]
        public async Task<IActionResult> ValidateStudent([FromQuery] string studentNo)
        {
            try
            {
                dynamic student = await _guardRepository.GetStudentByQrCode(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

                // Get violations to calculate warning level and return full list
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
                    warning_level = GetWarningLevel(violations.Count),
                    // Full violation list with date so guard can see history on scan
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
        // Records a new violation for a student and returns their updated warning level
        [HttpPost("student/violation")]
        public async Task<IActionResult> RecordViolation([FromBody] RecordViolationModel request)
        {
            // Validate severity before doing anything else
            if (string.IsNullOrEmpty(request.Severity) ||
                !ValidSeverities.Contains(request.Severity.ToLower()))
            {
                return BadRequest(new
                {
                    status = 0,
                    message = $"Invalid severity value '{request.Severity}'. Accepted values are: minor, moderate, major, critical."
                });
            }

            try
            {
                // Find the student first to get their internal StudentID
                dynamic student = await _guardRepository.GetStudentByQrCode(request.StudentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

                var violation = new
                {
                    StudentId = (int)student.StudentID,
                    Type = request.ViolationType,
                    Details = request.Details,
                    GuardId = request.GuardId,
                    Severity = request.Severity.ToLower()
                };

                await _guardRepository.RecordViolation(violation);

                // Get updated violations after recording to return the new warning level
                List<dynamic> violations = await _guardRepository
                    .GetViolationsByStudentId((string)student.StudentNo);

                return Ok(new
                {
                    status = 1,
                    message = "Violation recorded successfully.",
                    student_no = student.StudentNo,
                    name = $"{student.FirstName} {student.LastName}",
                    new_violation_count = violations.Count,
                    new_warning_level = GetWarningLevel(violations.Count)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/guard/violations/summary?StartDate=xxx&EndDate=xxx
        // Returns a summary of violations within a date range including the most common violation type
        [HttpGet("violations/summary")]
        public async Task<IActionResult> GetViolationSummary([FromQuery] GetSummaryModel request)
        {
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

                // Find the most frequently occurring violation type
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

        // GET api/guard/violations/my-records
        // Day 12 — Returns all violations that the currently logged in guard personally recorded
        [HttpGet("violations/my-records")]
        public async Task<IActionResult> GetMyViolations()
        {
            try
            {
                // Read the guard's ID from the "nameid" claim in the JWT token
                string? guardId = User.FindFirstValue("nameid");
                if (string.IsNullOrEmpty(guardId))
                    return Unauthorized(new { status = 0, message = "Guard ID not found in token." });

                List<dynamic> violations = await _guardRepository.GetViolationsByGuardId(guardId);

                if (violations == null || violations.Count == 0)
                    return NotFound(new { status = 0, message = "No violations recorded by this guard." });

                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    total = violations.Count,
                    data = violations.Select(v => new
                    {
                        id = v.ViolationID,
                        student_no = v.StudentNo,
                        student_name = v.StudentName,
                        type = v.ViolationName,
                        details = v.Description,
                        severity = v.Severity,
                        date = v.ViolationDate,
                        status = v.Status
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // Returns a color-coded warning level based on the number of violations
        private string GetWarningLevel(int violationCount)
        {
            if (violationCount >= 3) return "red";
            else if (violationCount == 2) return "orange";
            else if (violationCount == 1) return "yellow";
            else return "green";
        }

        // GET api/guard/students
        // Returns all registered students
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            try
            {
                List<dynamic> students = await _guardRepository.GetAllStudents();
                return Ok(new
                {
                    status = 1,
                    message = "Success",
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

        // GET api/guard/students/exist?studentNo=xxx
        // Returns a specific student by their StudentNo
        [HttpGet("students/exist")]
        public async Task<IActionResult> GetStudentByStudentNo(string studentNo)
        {
            try
            {
                dynamic student = await _guardRepository.GetStudentByStudentNo(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    data = new
                    {
                        student_no = student.StudentNo,
                        name = $"{student.FirstName} {student.LastName}",
                        course = student.Course,
                        year = student.Year
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }
    }
}