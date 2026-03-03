using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentViolations.API.Model.Response;
using StudentViolationsAPI.Data;
using StudentViolationsAPI.Model.Requests;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ summary FIRST to avoid route conflict with {studentId}
        [HttpGet("violations/summary")]
        public async Task<IActionResult> GetViolationSummary([FromQuery] GetSummaryRequest request)
        {
            try
            {
                var violations = await _context.Violations
                    .Where(v => v.Date >= request.StartDate && v.Date <= request.EndDate)
                    .ToListAsync();

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

        // ✅ parameterized route SECOND
        [HttpGet("violations/{studentId}")]
        public async Task<IActionResult> GetViolationHistory(string studentId)
        {
            try
            {
                var violations = await _context.Violations
                    .Where(v => v.StudentId == studentId)
                    .ToListAsync();

                if (violations == null || violations.Count == 0)
                {
                    return NotFound(new { status = "error", message = "No violations found for this student." });
                }

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
    }
}