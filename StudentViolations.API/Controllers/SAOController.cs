using Microsoft.AspNetCore.Mvc;
using StudentViolationsAPI.IRepository;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/sao")]
    [ApiExplorerSettings(GroupName = "Admin")]
    public class SAOController : ControllerBase
    {
        private readonly IViolationRepository _violationRepository;
        private readonly IStudentRepository _studentRepository;

        public SAOController(IViolationRepository violationRepository, IStudentRepository studentRepository)
        {
            _violationRepository = violationRepository;
            _studentRepository = studentRepository;
        }

        [HttpGet("violations")]
        public async Task<IActionResult> GetAllViolations()
        {
            try
            {
                var violations = await _violationRepository.GetAllViolations();
                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    data = violations.Select(v => new
                    {
                        id = v.Id,
                        studentId = v.StudentId,
                        type = v.Type,
                        details = v.Details,
                        severity = v.Severity,
                        date = v.Date,
                        guardId = v.GuardId,
                        status = v.Status
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpGet("violations/status/{status}")]
        public async Task<IActionResult> GetViolationsByStatus(string status)
        {
            try
            {
                var violations = await _violationRepository.GetAllViolations();
                var filtered = violations
                    .Where(v => v.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    .Select(v => new
                    {
                        id = v.Id,
                        studentId = v.StudentId,
                        type = v.Type,
                        details = v.Details,
                        severity = v.Severity,
                        date = v.Date,
                        guardId = v.GuardId,
                        status = v.Status
                    });

                return Ok(new { status = 1, message = "Success", data = filtered });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpGet("violations/summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var violations = await _violationRepository.GetAllViolations();
                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    data = new
                    {
                        total = violations.Count,
                        pending = violations.Count(v => v.Status == "Pending"),
                        approved = violations.Count(v => v.Status == "Approved"),
                        rejected = violations.Count(v => v.Status == "Rejected"),
                        bySeverity = violations
                            .GroupBy(v => v.Severity)
                            .Select(g => new { severity = g.Key, count = g.Count() }),
                        byType = violations
                            .GroupBy(v => v.Type)
                            .Select(g => new { type = g.Key, count = g.Count() })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpPut("violations/{id}/approve")]
        public async Task<IActionResult> ApproveViolation(int id)
        {
            try
            {
                var violation = await _violationRepository.GetViolationById(id);
                if (violation == null)
                    return NotFound(new { status = 0, message = "Violation not found." });

                await _violationRepository.UpdateViolationStatus(id, "Approved");
                return Ok(new { status = 1, message = "Violation approved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpPut("violations/{id}/reject")]
        public async Task<IActionResult> RejectViolation(int id)
        {
            try
            {
                var violation = await _violationRepository.GetViolationById(id);
                if (violation == null)
                    return NotFound(new { status = 0, message = "Violation not found." });

                await _violationRepository.UpdateViolationStatus(id, "Rejected");
                return Ok(new { status = 1, message = "Violation rejected successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpDelete("violations/{id}")]
        public async Task<IActionResult> DeleteViolation(int id)
        {
            try
            {
                var violation = await _violationRepository.GetViolationById(id);
                if (violation == null)
                    return NotFound(new { status = 0, message = "Violation not found." });

                await _violationRepository.DeleteViolation(id);
                return Ok(new { status = 1, message = "Violation deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }
    }
}