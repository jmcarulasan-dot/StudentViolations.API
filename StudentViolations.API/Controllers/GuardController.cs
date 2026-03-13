using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model.Response;
using StudentViolationsAPI.IRepository;
using StudentViolationsAPI.Model.Entities;
using StudentViolationsAPI.Model.Requests;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/guard")]
    [Authorize(Roles = "guard")]
    public class GuardController : ControllerBase
    {
        private readonly IGuardRepository _guardRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IViolationRepository _violationRepository;

        public GuardController(
            IGuardRepository guardRepository,
            IStudentRepository studentRepository,
            IViolationRepository violationRepository)
        {
            _guardRepository = guardRepository;
            _studentRepository = studentRepository;
            _violationRepository = violationRepository;
        }

        // Get violation summary by date range
        [HttpGet("violations/summary")]
        public async Task<IActionResult> GetViolationSummary([FromQuery] GetSummaryRequest request)
        {
            try
            {
                var violations = await _guardRepository.GetViolationsInDateRange(request.StartDate, request.EndDate);
                if (violations == null || violations.Count == 0)
                {
                    return NotFound(new ViolationSummaryResponse
                    {
                        Status = "error",
                        TotalViolations = 0,
                        TopViolation = "N/A",
                        StartDate = request.StartDate,
                        EndDate = request.EndDate
                    });
                }

                var topViolation = violations
                    .GroupBy(v => v.Type)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A";

                return Ok(new ViolationSummaryResponse
                {
                    Status = "success",
                    TotalViolations = violations.Count,
                    TopViolation = topViolation,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        //  Get violation history of a student
        [HttpGet("violations/{studentId}")]
        public async Task<IActionResult> GetViolationHistory(string studentId)
        {
            try
            {
                var violations = await _guardRepository.GetViolationsByStudentId(studentId);
                if (violations == null || violations.Count == 0)
                    return NotFound(new { status = "error", message = "No violations found for this student." });

                return Ok(new ViolationHistoryResponse
                {
                    Status = "success",
                    StudentId = studentId,
                    Violations = violations.Select(v => new ViolationDetail
                    {
                        Date = v.Date,
                        Type = v.Type,
                        Details = v.Details,
                        RecordedBy = v.GuardId
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("student/validate")]
        public async Task<IActionResult> ValidateStudent([FromQuery] string qrCode)
        {
            try
            {
                var student = await _studentRepository.GetStudentByStudentId(qrCode);
                if (student == null)
                    return NotFound(new { status = "error", message = "Student not found." });

                var violations = await _violationRepository.GetViolationsByStudentId(student.Id.ToString());
                return Ok(new
                {
                    status = "success",
                    student_id = student.Id,
                    name = student.Name,
                    violation_count = violations.Count,
                    warning_level = GetWarningLevel(violations.Count)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("student/violation")]
        public async Task<IActionResult> RecordViolation([FromBody] RecordViolationRequest request)
        {
            try
            {
                var student = await _studentRepository.GetStudentByStudentId(request.StudentId.ToString());
                if (student == null)
                    return BadRequest(new { status = "error", message = "Invalid Student." });

                var violation = new Violation
                {
                    StudentId = request.StudentId,
                    Type = request.ViolationType,
                    Details = request.Details,
                    Date = DateTime.Now,
                    GuardId = request.GuardId,
                    Severity = request.Severity
                };

                await _violationRepository.RecordViolation(violation);
                var violations = await _violationRepository.GetViolationsByStudentId(request.StudentId.ToString());

                return Ok(new
                {
                    status = "success",
                    message = "Violation recorded.",
                    new_violation_count = violations.Count,
                    new_warning_level = GetWarningLevel(violations.Count)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        private string GetWarningLevel(int violationCount)
        {
            if (violationCount >= 3) return "red";
            else if (violationCount == 2) return "orange";
            else if (violationCount == 1) return "yellow";
            else return "green";
        }
    }
}