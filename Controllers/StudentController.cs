using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolationsAPI.IRepository;
using System.Security.Claims;

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

        // Helper: Gets the StudentNo of the currently logged-in student from JWT token
        private string GetLoggedInStudentNo()
        {
            // StudentNo is stored as "studentNo" claim in the JWT token during login
            return User.FindFirstValue("studentNo") ?? string.Empty;
        }

        // GET api/student/violations
        // Returns violations for the currently logged-in student ONLY
        // Security fix (Day 2): StudentNo comes from JWT token — student cannot access other students' data
        [HttpGet("violations")]
        public async Task<IActionResult> GetMyViolations()
        {
            try
            {
                string studentNo = GetLoggedInStudentNo();
                if (string.IsNullOrEmpty(studentNo))
                    return Unauthorized(new { status = 0, message = "Invalid token. Please login again." });

                var student = await _studentRepository.GetStudentByStudentId(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

                var violations = await _violationRepository.GetViolationsByStudentId(studentNo);

                // Day 14 — Count violations by status
                int pendingCount = violations.Count(v => ((string)v.Status).Equals("Pending", StringComparison.OrdinalIgnoreCase));
                int approvedCount = violations.Count(v => ((string)v.Status).Equals("Approved", StringComparison.OrdinalIgnoreCase));
                int rejectedCount = violations.Count(v => ((string)v.Status).Equals("Rejected", StringComparison.OrdinalIgnoreCase));

                return Ok(new
                {
                    status = 1,
                    message = "Violations retrieved successfully.",
                    data = new
                    {
                        student_no = student.StudentNo,
                        name = $"{student.FirstName} {student.LastName}",
                        total_violations = violations.Count,
                        pending = pendingCount,
                        approved = approvedCount,
                        rejected = rejectedCount,
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

        // GET api/student/profile
        // Returns the logged-in student's own profile only
        // Security fix (Day 2): uses JWT token — student cannot view other students' profiles
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                string studentNo = GetLoggedInStudentNo();
                if (string.IsNullOrEmpty(studentNo))
                    return Unauthorized(new { status = 0, message = "Invalid token. Please login again." });

                var student = await _studentRepository.GetStudentByStudentId(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

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

        // GET api/student/qrcode
        // Returns the logged-in student's own QR code only
        // Security fix (Day 2): uses JWT token — student cannot view other students' QR codes
        [HttpGet("qrcode")]
        public async Task<IActionResult> GetMyQrCode()
        {
            try
            {
                string studentNo = GetLoggedInStudentNo();
                if (string.IsNullOrEmpty(studentNo))
                    return Unauthorized(new { status = 0, message = "Invalid token. Please login again." });

                var student = await _studentRepository.GetStudentByStudentId(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

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