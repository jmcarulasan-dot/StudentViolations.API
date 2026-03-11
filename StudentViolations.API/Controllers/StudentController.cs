using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolationsAPI.IRepository;

namespace StudentViolationsAPI.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IViolationRepository _violationRepository;

        public StudentController(IStudentRepository studentRepository, IViolationRepository violationRepository)
        {
            _studentRepository = studentRepository;
            _violationRepository = violationRepository;
        }

        // ✅ Student views own violations
        [HttpGet("{studentId}/violations")]
        public async Task<IActionResult> GetStudentViolations(string studentId)
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
                    message = "Violations retrieved successfully.",
                    data = new
                    {
                        student_id = student.Id,
                        name = student.Name,
                        total_violations = violations.Count,
                        warning_level = GetWarningLevel(violations.Count),
                        violations = violations.Select(v => new
                        {
                            id = v.Id,
                            type = v.Type,
                            details = v.Details,
                            severity = v.Severity,
                            date = v.Date,
                            status = v.Status
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // ✅ Student views own profile
        [HttpGet("{studentId}/profile")]
        public async Task<IActionResult> GetStudentProfile(string studentId)
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
                    message = "Profile retrieved successfully.",
                    data = new
                    {
                        id = student.Id,
                        name = student.Name,
                        email = student.Email,
                        gender = student.Gender,
                        course = student.Course,
                        year = student.Year,
                        contact_number = student.ContactNumber,
                        address = student.Address,
                        total_violations = violations.Count,
                        warning_level = GetWarningLevel(violations.Count)
                    }
                });
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