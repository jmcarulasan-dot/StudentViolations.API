using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.Helpers;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/sao")]
    [ApiExplorerSettings(GroupName = "Admin")]
    [Authorize(Roles = "sao,Sao")]
    public class SAOController : ControllerBase
    {
        private readonly IViolationRepository _violationRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISAORepository _saoRepository;
        private static readonly string[] ValidGenders = { "male", "female" };
        public SAOController(
            IViolationRepository violationRepository,
            IStudentRepository studentRepository,
            ISAORepository saoRepository)
        {
            _violationRepository = violationRepository;
            _studentRepository = studentRepository;
            _saoRepository = saoRepository;
        }
        // GET api/sao/violations
        [HttpGet("violations")]
        public async Task<IActionResult> GetAllViolations()
        {
            var result = await _violationRepository.GetAllViolations();
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var violations = result.Data ?? new List<ViolationModel>();
            if (violations.Count == 0)
                return NotFound(new { status = 404, message = "No violations found." });

            var studentsResult = await _studentRepository.GetAllStudents();
            var students = studentsResult.Data ?? new List<StudentModel>();

            return Ok(new
            {
                status = 200,
                message = "Success",
                total = violations.Count,
                data = violations.Select(v => new
                {
                    id = v.ViolationID,
                    student_no = students.FirstOrDefault(s => s.StudentID == v.StudentId)?.StudentNo,
                    type = v.ViolationName,
                    details = v.Description,
                    severity = v.Severity,
                    date = v.ViolationDate,
                    recorded_by = v.GuardName,
                    status = v.Status
                })
            });
        }
        // GET api/sao/violations/by-status/{status}
        [HttpGet("violations/by-status/{status}")]
        public async Task<IActionResult> GetViolationsByStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(new { status = 400, message = "Status is required." });

            status = status.Trim().ToLower();
            var validStatuses = new[] { "pending", "approved", "rejected" };
            if (!validStatuses.Contains(status))
                return BadRequest(new { status = 400, message = "Status must be: pending, approved, or rejected." });

            var result = await _violationRepository.GetAllViolations();
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var studentsResult = await _studentRepository.GetAllStudents();
            var students = studentsResult.Data ?? new List<StudentModel>();

            var filtered = (result.Data ?? new List<ViolationModel>())
                .Where(v => v.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .Select(v => new
                {
                    id = v.ViolationID,
                    student_no = students.FirstOrDefault(s => s.StudentID == v.StudentId)?.StudentNo,
                    type = v.ViolationName,
                    details = v.Description,
                    severity = v.Severity,
                    date = v.ViolationDate,
                    recorded_by = v.GuardName,
                    status = v.Status
                }).ToList();

            if (filtered.Count == 0)
                return NotFound(new { status = 404, message = $"No {status} violations found." });

            return Ok(new { status = 200, message = "Success", total = filtered.Count, data = filtered });
        }
        // PUT api/sao/violations/{id}/approve
        [HttpPut("violations/{id}/approve")]
        public async Task<IActionResult> ApproveViolation(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 400, message = "Violation ID must be a positive number." });

            var violationResult = await _violationRepository.GetViolationById(id);
            if (violationResult.Status != 200)
                return StatusCode(violationResult.Status, new { status = violationResult.Status, message = violationResult.Message });

            if (violationResult.Data.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { status = 400, message = "Violation is already approved." });

            var result = await _violationRepository.UpdateViolationStatus(id, "Approved");
            return Ok(new { status = 200, message = result.Message });
        }

        // PUT api/sao/violations/{id}/reject
        [HttpPut("violations/{id}/reject")]
        public async Task<IActionResult> RejectViolation(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 400, message = "Violation ID must be a positive number." });

            var violationResult = await _violationRepository.GetViolationById(id);
            if (violationResult.Status != 200)
                return StatusCode(violationResult.Status, new { status = violationResult.Status, message = violationResult.Message });

            if (violationResult.Data.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { status = 400, message = "Violation is already rejected." });

            var result = await _violationRepository.UpdateViolationStatus(id, "Rejected");
            return Ok(new { status = 200, message = result.Message });
        }
        // DELETE api/sao/violations/{id}
        [HttpDelete("violations/{id}")]
        public async Task<IActionResult> DeleteViolation(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 400, message = "Violation ID must be a positive number." });

            var violationResult = await _violationRepository.GetViolationById(id);
            if (violationResult.Status != 200)
                return StatusCode(violationResult.Status, new { status = violationResult.Status, message = violationResult.Message });

            // Capture deletion history before deleting
            var deletionRecord = new
            {
                deleted_violation_id = violationResult.Data.ViolationID,
                student_id = violationResult.Data.StudentId,
                type = violationResult.Data.ViolationName,
                details = violationResult.Data.Description,
                severity = violationResult.Data.Severity,
                original_date = violationResult.Data.ViolationDate,
                original_status = violationResult.Data.Status,
                recorded_by = violationResult.Data.GuardName,
                deleted_by = User.FindFirstValue(ClaimTypes.Name) ?? "SAO",
                deleted_at = DateTime.UtcNow
            };

            var result = await _violationRepository.DeleteViolation(id);
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            return Ok(new { status = 200, message = "Violation deleted successfully.", deletion_history = deletionRecord });
        }
        // GET api/sao/violations/summary
        [HttpGet("violations/summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _violationRepository.GetAllViolations();
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var violations = result.Data ?? new List<ViolationModel>();
            if (violations.Count == 0)
                return NotFound(new { status = 404, message = "No violations found." });

            return Ok(new
            {
                status = 200,
                message = "Success",
                data = new
                {
                    total = violations.Count,
                    pending = violations.Count(v => v.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)),
                    approved = violations.Count(v => v.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)),
                    rejected = violations.Count(v => v.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase)),
                    by_severity = violations
                        .GroupBy(v => v.Severity.ToLower())
                        .Select(g => new { severity = g.Key, count = g.Count() }),
                    by_type = violations
                        .GroupBy(v => v.ViolationName.ToLower())
                        .Select(g => new { type = g.First().ViolationName, count = g.Count() })
                }
            });
        }
        // GET api/sao/students/{studentNo}/report
        [HttpGet("students/{studentNo}/report")]
        public async Task<IActionResult> GetStudentReport(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            var violationsResult = await _violationRepository.GetViolationsByStudentId(studentNo);
            var violations = violationsResult.Data ?? new List<ViolationModel>();

            return Ok(new
            {
                status = 200,
                message = "Success",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}",
                    email = studentResult.Data.Email,
                    contact_number = studentResult.Data.ContactNumber,
                    gender = studentResult.Data.Gender,
                    address = studentResult.Data.Address,
                    date_of_birth = studentResult.Data.DateOfBirth,
                    course = studentResult.Data.Course,
                    year = studentResult.Data.Year,
                    violation_count = violations.Count,
                    warning_level = ViolationHelper.GetWarningLevel(violations.Count),
                    recommended_action = ViolationHelper.GetRecommendedAction(violations.Count),
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
        // GET api/sao/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _saoRepository.GetAllUsers();
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var users = result.Data ?? new List<UserModel>();
            if (users.Count == 0)
                return NotFound(new { status = 404, message = "No users found." });

            return Ok(new
            {
                status = 200,
                message = "Success",
                total = users.Count,
                data = users.Select(u => new
                {
                    id = u.StudentID,
                    username = u.Username,
                    name = $"{u.FirstName} {u.LastName}",
                    email = u.Email,
                    role = u.Role,
                    gender = u.Gender,
                    course = u.Course,
                    year = u.Year,
                    contact_number = u.ContactNumber,
                    registration_date = u.RegistrationDate
                })
            });
        }
        // GET api/sao/users/{id}
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 400, message = "User ID must be a positive number." });

            var result = await _saoRepository.GetUserById(id);
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            return Ok(new
            {
                status = 200,
                message = "Success. Copy the fields below into the update request and change only what you need.",
                data = new
                {
                    id = result.Data.StudentID,
                    username = result.Data.Username,
                    first_name = result.Data.FirstName,
                    last_name = result.Data.LastName,
                    email = result.Data.Email,
                    role = result.Data.Role,
                    gender = result.Data.Gender,
                    course = result.Data.Course,
                    year = result.Data.Year,
                    address = result.Data.Address,
                    contact_number = result.Data.ContactNumber
                }
            });
        }
        // PUT api/sao/users/{id}
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserModel request)
        {
            if (id <= 0)
                return BadRequest(new { status = 400, message = "User ID must be a positive number." });
            if (request == null)
                return BadRequest(new { status = 400, message = "Request body is required." });

            if (request.FirstName != null && !Regex.IsMatch(request.FirstName.Trim(), @"^[a-zA-Z\s\-]+$"))
                return BadRequest(new { status = 400, message = "First name must contain letters only." });
            if (request.LastName != null && !Regex.IsMatch(request.LastName.Trim(), @"^[a-zA-Z\s\-]+$"))
                return BadRequest(new { status = 400, message = "Last name must contain letters only." });
            if (request.Email != null && !Regex.IsMatch(request.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest(new { status = 400, message = "Invalid email format." });
            if (request.ContactNumber != null && !Regex.IsMatch(request.ContactNumber.Trim(), @"^09\d{9}$"))
                return BadRequest(new { status = 400, message = "Contact number must be 11 digits starting with 09." });
            if (request.Gender != null && !ValidGenders.Contains(request.Gender.Trim().ToLower()))
                return BadRequest(new { status = 400, message = "Gender must be 'male' or 'female'." });

            var userResult = await _saoRepository.GetUserById(id);
            if (userResult.Status != 200)
                return StatusCode(userResult.Status, new { status = userResult.Status, message = userResult.Message });

            var updated = new UserModel
            {
                StudentID = userResult.Data.StudentID,
                FirstName = request.FirstName?.Trim() ?? userResult.Data.FirstName,
                LastName = request.LastName?.Trim() ?? userResult.Data.LastName,
                Email = request.Email?.Trim().ToLower() ?? userResult.Data.Email,
                ContactNumber = request.ContactNumber?.Trim() ?? userResult.Data.ContactNumber,
                Gender = request.Gender?.Trim().ToLower() ?? userResult.Data.Gender,
                Address = request.Address?.Trim() ?? userResult.Data.Address,
            };

            var result = await _saoRepository.UpdateUser(updated);
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            return Ok(new
            {
                status = 200,
                message = "User updated successfully.",
                data = new
                {
                    id = updated.StudentID,
                    name = $"{updated.FirstName} {updated.LastName}",
                    email = updated.Email,

                }
            });
        }
        // DELETE api/sao/users/{id}
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 400, message = "User ID must be a positive number." });

            var userResult = await _saoRepository.GetUserById(id);
            if (userResult.Status != 200)
                return StatusCode(userResult.Status, new { status = userResult.Status, message = userResult.Message });

            var result = await _saoRepository.DeleteUser(id);
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            return Ok(new { status = 200, message = $"User {userResult.Data.Username} deleted successfully." });
        }

        // PUT api/sao/students/{studentNo}/dismiss
        [HttpPut("students/{studentNo}/dismiss")]
        public async Task<IActionResult> DismissStudent(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            if (studentResult.Data.Status == "Dismissed")
                return BadRequest(new { status = 400, message = "Student is already dismissed." });

            if (studentResult.Data.Status != "PendingDismissal")
                return BadRequest(new { status = 400, message = "Student has not been recommended for dismissal by Guidance." });

            var result = await _studentRepository.UpdateStudentStatus(studentResult.Data.StudentID, "Dismissed");
            return StatusCode(result.Status, new { status = result.Status, message = result.Message });
        }

        // PUT api/sao/students/{studentNo}/cancel-dismiss
        [HttpPut("students/{studentNo}/cancel-dismiss")]
        public async Task<IActionResult> CancelDismissal(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            if (studentResult.Data.Status != "PendingDismissal" && studentResult.Data.Status != "Dismissed")
                return BadRequest(new { status = 400, message = "Student is not pending or dismissed." });

            var result = await _studentRepository.UpdateStudentStatus(studentResult.Data.StudentID, "Active");
            return StatusCode(result.Status, new { status = result.Status, message = result.Message });
        }

        // PUT api/sao/violations/{id}/appeal/review
        [HttpPut("violations/{id}/appeal/review")]
        public async Task<IActionResult> ReviewAppeal(int id, [FromBody] AppealReviewModel request)
        {
            if (request == null)
                return BadRequest(new { status = 400, message = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.AppealStatus))
                return BadRequest(new { status = 400, message = "Appeal status is required." });

            var validStatuses = new[] { "Approved", "Rejected" };
            if (!validStatuses.Contains(request.AppealStatus.Trim()))
                return BadRequest(new { status = 400, message = "Appeal status must be Approved or Rejected." });

            var violationResult = await _violationRepository.GetViolationById(id);
            if (violationResult.Status != 200)
                return StatusCode(violationResult.Status, new { status = violationResult.Status, message = violationResult.Message });

            if (violationResult.Data.AppealStatus == "None")
                return BadRequest(new { status = 400, message = "No appeal has been submitted for this violation." });

            var result = await _violationRepository.UpdateAppealStatus(id, request.AppealStatus.Trim(), request.AppealRemarks?.Trim());
            return StatusCode(result.Status, new { status = result.Status, message = result.Message });
        }
    }
}