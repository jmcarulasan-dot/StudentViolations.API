using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolationsAPI.IRepository;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/guidance")]
    [Authorize(Roles = "guidance")] // ✅ Only guidance can access
    public class GuidanceController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IViolationRepository _violationRepository;

        public GuidanceController(IStudentRepository studentRepository, IViolationRepository violationRepository)
        {
            _studentRepository = studentRepository;
            _violationRepository = violationRepository;
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            try
            {
                var students = await _studentRepository.GetAllStudents();
                var result = new List<object>();

                foreach (var student in students)
                {
                    var violations = await _violationRepository.GetViolationsByStudentId(student.Id.ToString());
                    result.Add(new
                    {
                        id = student.Id,
                        name = student.Name,
                        email = student.Email,
                        contactNumber = student.ContactNumber,
                        gender = student.Gender,
                        violationCount = violations.Count,
                        warningLevel = GetWarningLevel(violations.Count)
                    });
                }

                return Ok(new { status = 1, message = "Success", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpGet("students/{studentId}/report")]
        public async Task<IActionResult> GetStudentReport(string studentId)
        {
            try
            {
                var student = await _studentRepository.GetStudentByStudentId(studentId);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

                var violations = await _violationRepository.GetViolationsByStudentId(studentId);

                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    data = new
                    {
                        id = student.Id,
                        name = student.Name,
                        email = student.Email,
                        contactNumber = student.ContactNumber,
                        gender = student.Gender,
                        address = student.Address,
                        dateOfBirth = student.DateOfBirth,
                        violationCount = violations.Count,
                        warningLevel = GetWarningLevel(violations.Count),
                        violations = violations.Select(v => new
                        {
                            id = v.Id,
                            type = v.Type,
                            details = v.Details,
                            severity = v.Severity,
                            date = v.Date,
                            status = v.Status,
                            guardId = v.GuardId
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpGet("violations/severity")]
        public async Task<IActionResult> GetViolationsBySeverity()
        {
            try
            {
                var violations = await _violationRepository.GetAllViolations();
                var grouped = violations
                    .GroupBy(v => v.Severity)
                    .Select(g => new
                    {
                        severity = g.Key,
                        count = g.Count(),
                        violations = g.Select(v => new
                        {
                            id = v.Id,
                            studentId = v.StudentId,
                            type = v.Type,
                            details = v.Details,
                            date = v.Date,
                            status = v.Status
                        })
                    });

                return Ok(new { status = 1, message = "Success", data = grouped });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpGet("violations/pending")]
        public async Task<IActionResult> GetPendingViolations()
        {
            try
            {
                var violations = await _violationRepository.GetAllViolations();
                var pending = violations
                    .Where(v => v.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    .Select(v => new
                    {
                        id = v.Id,
                        studentId = v.StudentId,
                        type = v.Type,
                        details = v.Details,
                        severity = v.Severity,
                        date = v.Date,
                        status = v.Status,
                        guardId = v.GuardId
                    });

                return Ok(new { status = 1, message = "Success", data = pending });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
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