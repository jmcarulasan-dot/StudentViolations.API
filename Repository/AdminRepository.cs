using Microsoft.AspNetCore.Mvc;
using StudentViolationsAPI.Data; // Add this
using Microsoft.EntityFrameworkCore; // Add this
using StudentViolationsAPI.Model.Requests; // Add this
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudentViolationsAPI.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context; // Inject AppDbContext

        public AdminController(AppDbContext context) // Update constructor
        {
            _context = context;
        }

        [HttpGet("violations/{studentId}")]
        public async Task<IActionResult> GetViolationHistory(string studentId)
        {
            try
            {
                var violations = await _context.Violations // Access violations through context
                    .Where(v => v.StudentId == studentId)
                    .ToListAsync();

                if (violations == null || violations.Count == 0)
                {
                    return NotFound(new { status = "error", message = "No violations found for this student." });
                }

                return Ok(new
                {
                    status = "success",
                    student_id = studentId,
                    violations = violations.Select(v => new
                    {
                        date = v.Date,
                        type = v.Type,
                        details = v.Details,
                        recorded_by = v.GuardId
                    })
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { status = "error", message = "An error occurred." });
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetViolationSummary([FromQuery] GetSummaryRequest request)
        {
            try
            {
                var violations = await _context.Violations // Access violations through context
                    .Where(v => v.Date >= request.StartDate && v.Date <= request.EndDate)
                    .ToListAsync();

                if (violations == null || violations.Count == 0)
                {
                    return NotFound(new { status = "success", message = "No violations found in the specified date range." });
                }

                // Calculate the top violation type
                var topViolation = violations
                    .GroupBy(v => v.Type)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A";

                return Ok(new
                {
                    status = "success",
                    total_violations = violations.Count,
                    top_violation = topViolation,
                    date_range = new
                    {
                        start = request.StartDate,
                        end = request.EndDate
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { status = "error", message = "An error occurred." });
            }
        }
    }
}