using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolationsAPI.IRepository;

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

                if (students == null || students.Count == 0)
                    return NotFound(new { status = 0, message = "No students found." });

                List<object> result = new List<object>();

                foreach (dynamic student in students)
                {
                    // Get each student's violations to calculate their warning level
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
                        warning_level = GetWarningLevel(violations.Count)
                    });
                }

                return Ok(new { status = 1, message = "Success", total = result.Count, data = result });
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
            // Validate studentNo is not empty
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 0, message = "Student number is required." });

            // Normalize to uppercase for consistent matching
            studentNo = studentNo.Trim().ToUpper();

            try
            {
                dynamic student = await _studentRepository.GetStudentByStudentId(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = $"Student '{studentNo}' not found." });

                // Fetch all violations for this student
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
                        violation_count = violations.Count,
                        warning_level = GetWarningLevel(violations.Count),
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

        // GET api/guidance/violations/pending
        // Returns all violations that are still waiting for SAO approval
        [HttpGet("violations/pending")]
        public async Task<IActionResult> GetPendingViolations()
        {
            try
            {
                List<dynamic> violations = await _violationRepository.GetAllViolations();
                List<dynamic> students = await _studentRepository.GetAllStudents();

                // Filter only Pending violations and match each one to a student number
                var pending = violations
                    .Where(v => ((string)v.Status).Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    .Select(v => new
                    {
                        id = v.ViolationID,
                        student_no = students
                            .FirstOrDefault(s => s.StudentID == v.StudentId)?.StudentNo,
                        type = v.ViolationName,
                        details = v.Description,
                        severity = v.Severity,
                        date = v.ViolationDate,
                        status = v.Status,
                        recorded_by = v.GuardName
                    }).ToList();

                if (pending.Count == 0)
                    return NotFound(new { status = 0, message = "No pending violations found." });

                return Ok(new { status = 1, message = "Success", total = pending.Count, data = pending });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/guidance/violations/by-severity
        // Returns all violations grouped by severity level (minor, moderate, major, critical)
        [HttpGet("violations/by-severity")]
        public async Task<IActionResult> GetViolationsBySeverity()
        {
            try
            {
                List<dynamic> violations = await _violationRepository.GetAllViolations();

                if (violations == null || violations.Count == 0)
                    return NotFound(new { status = 0, message = "No violations found." });

                List<dynamic> students = await _studentRepository.GetAllStudents();

                // Group violations by severity and attach the student number to each one
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

        // Returns a color-coded warning level based on the number of violations
        private string GetWarningLevel(int violationCount)
        {
            if (violationCount >= 3) return "red";
            else if (violationCount == 2) return "orange";
            else if (violationCount == 1) return "yellow";
            else return "green";
        }
    }
}