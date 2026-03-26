using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.Helpers;
using StudentViolations.API.IRepository;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/guidance")]
    [Authorize(Roles = "guidance,Guidance")]
    public class GuidanceController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IViolationRepository _violationRepository;

        public GuidanceController(
            IStudentRepository studentRepository,
            IViolationRepository violationRepository)
        {
            _studentRepository = studentRepository;
            _violationRepository = violationRepository;
        }

        // GET api/guidance/students
        // Returns all students with their violation count and warning level
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            try
            {
                List<dynamic> students = await _studentRepository.GetAllStudents();
                List<object> result = new List<object>();

                foreach (dynamic student in students)
                {
                    List<dynamic> violations = await _violationRepository
                        .GetViolationsByStudentId((string)student.StudentNo);

                    result.Add(new
                    {
                        student_no = student.StudentNo,
                        name = $"{student.FirstName} {student.LastName}",
                        email = student.Email,
                        contact_number = student.ContactNumber,
                        gender = student.Gender,
                        violation_count = violations.Count,
                        warning_level = ViolationHelper.GetWarningLevel(violations.Count)
                    });
                }

                return Ok(new { status = 1, message = "Success", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/guidance/students/{studentNo}/report
        // Returns full profile and all violations of a specific student
        [HttpGet("students/{studentNo}/report")]
        public async Task<IActionResult> GetStudentReport(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 0, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            try
            {
                dynamic student = await _studentRepository.GetStudentByStudentId(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = $"Student '{studentNo}' not found." });

                List<dynamic> violations = await _violationRepository
                    .GetViolationsByStudentId((string)student.StudentNo);

                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    data = new
                    {
                        student_no = student.StudentNo,
                        name = $"{student.FirstName} {student.LastName}",
                        email = student.Email,
                        contact_number = student.ContactNumber,
                        gender = student.Gender,
                        address = student.Address,
                        date_of_birth = student.DateOfBirth,
                        course = student.Course,
                        year = student.Year,
                        violation_count = violations.Count,
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
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/guidance/violations/by-status
        // Returns all violations grouped by status (Pending, Approved, Rejected) with counts
        [HttpGet("violations/by-status")]
        public async Task<IActionResult> GetViolationsByStatus()
        {
            try
            {
                List<dynamic> violations = await _violationRepository.GetAllViolations();
                List<dynamic> students = await _studentRepository.GetAllStudents();

                var statuses = new[] { "Pending", "Approved", "Rejected" };

                var grouped = statuses.Select(s => new
                {
                    status = s,
                    count = violations.Count(v => ((string)v.Status).Equals(s, StringComparison.OrdinalIgnoreCase)),
                    violations = violations
                        .Where(v => ((string)v.Status).Equals(s, StringComparison.OrdinalIgnoreCase))
                        .Select(v => new
                        {
                            id = v.ViolationID,
                            student_no = students
                                .FirstOrDefault(s2 => s2.StudentID == v.StudentId)?.StudentNo,
                            type = v.ViolationName,
                            details = v.Description,
                            severity = v.Severity,
                            date = v.ViolationDate,
                            recorded_by = v.GuardName
                        })
                });

                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    total = violations.Count,
                    data = grouped
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/guidance/violations/by-severity
        // Returns all violations grouped by severity level
        [HttpGet("violations/by-severity")]
        public async Task<IActionResult> GetViolationsBySeverity()
        {
            try
            {
                List<dynamic> violations = await _violationRepository.GetAllViolations();
                List<dynamic> students = await _studentRepository.GetAllStudents();

                var grouped = violations
                    .GroupBy(v => (string)v.Severity)
                    .Select(g => new
                    {
                        severity = g.Key,
                        count = g.Count(),
                        violations = g.Select(v => new
                        {
                            id = v.ViolationID,
                            student_no = students
                                .FirstOrDefault(s => s.StudentID == v.StudentId)?.StudentNo,
                            type = v.ViolationName,
                            details = v.Description,
                            date = v.ViolationDate,
                            status = v.Status,
                            recorded_by = v.GuardName
                        })
                    });

                return Ok(new { status = 1, message = "Success", data = grouped });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }
    }
}