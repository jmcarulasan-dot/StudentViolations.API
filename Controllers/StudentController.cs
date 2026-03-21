using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolationsAPI.IRepository;

namespace StudentViolationsAPI.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student,student")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IViolationRepository _violationRepository;

        public StudentController(IStudentRepository studentRepository, IViolationRepository violationRepository)
        {
            _studentRepository = studentRepository;
            _violationRepository = violationRepository;
        }

        // GET api/student/{studentNo}/violations
        // Returns all violations belonging to a specific student
        [HttpGet("{studentNo}/violations")]
        public async Task<IActionResult> GetStudentViolations(string studentNo)
        {
            try
            {
                // Find the student first to confirm they exist
                var student = await _studentRepository.GetStudentByStudentId(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

                var violations = await _violationRepository.GetViolationsByStudentId(studentNo);

                return Ok(new
                {
                    status = 1,
                    message = "Violations retrieved successfully.",
                    data = new
                    {
                        student_no = student.StudentNo,
                        name = $"{student.FirstName} {student.LastName}",
                        total_violations = violations.Count,
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

        // GET api/student/{studentNo}/profile
        // Returns a student's personal info along with their current warning level
        [HttpGet("{studentNo}/profile")]
        public async Task<IActionResult> GetStudentProfile(string studentNo)
        {
            try
            {
                var student = await _studentRepository.GetStudentByStudentId(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

                // Get violations to calculate the warning level shown on the profile
                var violations = await _violationRepository.GetViolationsByStudentId(studentNo);

                return Ok(new
                {
                    status = 1,
                    message = "Profile retrieved successfully.",
                    data = new
                    {
                        student_no = student.StudentNo,
                        name = $"{student.FirstName} {student.LastName}",
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

        // GET api/student/{studentNo}/qrcode
        // Returns the student's QR code as a Base64 string for display or printing
        [HttpGet("{studentNo}/qrcode")]
        public async Task<IActionResult> GetStudentQrCode(string studentNo)
        {
            try
            {
                var student = await _studentRepository.GetStudentByStudentId(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

                // QR code might be null if student was registered without a StudentNo
                if (student.QRCode == null)
                    return NotFound(new { status = 0, message = "QR code not found for this student." });

                return Ok(new
                {
                    status = 1,
                    message = "QR code retrieved successfully.",
                    data = new
                    {
                        student_no = student.StudentNo,
                        name = $"{student.FirstName} {student.LastName}",
                        qr_code = student.QRCode
                    }
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
    }
}