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
        private static readonly string[] ValidCourses = { "bsit", "bscs", "bsba", "bsa", "bshm" };
        private static readonly string[] ValidYears = { "1", "2", "3", "4" };

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
        // Returns all violations with the matching student number and guard name attached
        [HttpGet("violations")]
        public async Task<IActionResult> GetAllViolations()
        {
            try
            {
                List<dynamic> violations = await _violationRepository.GetAllViolations();
                List<dynamic> students = await _studentRepository.GetAllStudents();

                if (violations == null || violations.Count == 0)
                    return NotFound(new { status = 0, message = "No violations found." });

                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    total = violations.Count,
                    data = violations.Select(v => new
                    {
                        id = v.ViolationID,
                        student_no = students
                            .FirstOrDefault(s => s.StudentID == v.StudentId)?.StudentNo,
                        type = v.ViolationName,
                        details = v.Description,
                        severity = v.Severity,
                        date = v.ViolationDate,
                        recorded_by = v.GuardName,
                        status = v.Status
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/sao/violations/by-status/{status}
        // Filters violations by status — accepts Pending, Approved, or Rejected
        [HttpGet("violations/by-status/{status}")]
        public async Task<IActionResult> GetViolationsByStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(new { status = 0, message = "Status is required." });

            status = status.Trim().ToLower();
            var validStatuses = new[] { "pending", "approved", "rejected" };
            if (!validStatuses.Contains(status))
                return BadRequest(new
                {
                    status = 0,
                    message = $"Invalid status '{status}'. Accepted values are: Pending, Approved, Rejected."
                });

            try
            {
                List<dynamic> violations = await _violationRepository.GetAllViolations();
                List<dynamic> students = await _studentRepository.GetAllStudents();

                var filtered = violations
                    .Where(v => ((string)v.Status).Equals(status, StringComparison.OrdinalIgnoreCase))
                    .Select(v => new
                    {
                        id = v.ViolationID,
                        student_no = students
                            .FirstOrDefault(s => s.StudentID == v.StudentId)?.StudentNo,
                        type = v.ViolationName,
                        details = v.Description,
                        severity = v.Severity,
                        date = v.ViolationDate,
                        recorded_by = v.GuardName,
                        status = v.Status
                    }).ToList();

                if (filtered.Count == 0)
                    return NotFound(new { status = 0, message = $"No {status} violations found." });

                return Ok(new { status = 1, message = "Success", total = filtered.Count, data = filtered });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // PUT api/sao/violations/{id}/approve
        // Changes a violation's status to Approved
        [HttpPut("violations/{id}/approve")]
        public async Task<IActionResult> ApproveViolation(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 0, message = "Violation ID must be a positive number." });

            try
            {
                dynamic violation = await _violationRepository.GetViolationById(id);
                if (violation == null)
                    return NotFound(new { status = 0, message = $"Violation with ID {id} not found." });

                if (((string)violation.Status).Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { status = 0, message = "Violation is already approved." });

                await _violationRepository.UpdateViolationStatus(id, "Approved");
                return Ok(new { status = 1, message = "Violation approved successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // PUT api/sao/violations/{id}/reject
        // Changes a violation's status to Rejected
        [HttpPut("violations/{id}/reject")]
        public async Task<IActionResult> RejectViolation(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 0, message = "Violation ID must be a positive number." });

            try
            {
                dynamic violation = await _violationRepository.GetViolationById(id);
                if (violation == null)
                    return NotFound(new { status = 0, message = $"Violation with ID {id} not found." });

                if (((string)violation.Status).Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { status = 0, message = "Violation is already rejected." });

                await _violationRepository.UpdateViolationStatus(id, "Rejected");
                return Ok(new { status = 1, message = "Violation rejected successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // DELETE api/sao/violations/{id}
        // Permanently removes a violation and returns a deletion history record in the response
        [HttpDelete("violations/{id}")]
        public async Task<IActionResult> DeleteViolation(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 0, message = "Violation ID must be a positive number." });

            try
            {
                dynamic violation = await _violationRepository.GetViolationById(id);
                if (violation == null)
                    return NotFound(new { status = 0, message = $"Violation with ID {id} not found." });

                // Capture violation details before deleting — returned as the deletion history record
                var deletionRecord = new
                {
                    deleted_violation_id = violation.ViolationID,
                    student_id = violation.StudentId,
                    type = violation.ViolationName,
                    details = violation.Description,
                    severity = violation.Severity,
                    original_date = violation.ViolationDate,
                    original_status = violation.Status,
                    recorded_by = violation.GuardName,
                    deleted_by = User.FindFirstValue(ClaimTypes.Name) ?? "SAO",
                    deleted_at = DateTime.UtcNow
                };

                await _violationRepository.DeleteViolation(id);

                return Ok(new
                {
                    status = 1,
                    message = "Violation deleted successfully.",
                    deletion_history = deletionRecord
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/sao/violations/summary
        // Returns total counts grouped by status, severity, and type
        // Violation types are grouped case-insensitively — "No ID" and "no id" count as one
        [HttpGet("violations/summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                List<dynamic> violations = await _violationRepository.GetAllViolations();

                if (violations == null || violations.Count == 0)
                    return NotFound(new { status = 0, message = "No violations found." });

                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    data = new
                    {
                        total = violations.Count,
                        pending = violations.Count(v => ((string)v.Status).Equals("Pending", StringComparison.OrdinalIgnoreCase)),
                        approved = violations.Count(v => ((string)v.Status).Equals("Approved", StringComparison.OrdinalIgnoreCase)),
                        rejected = violations.Count(v => ((string)v.Status).Equals("Rejected", StringComparison.OrdinalIgnoreCase)),
                        by_severity = violations
                            .GroupBy(v => ((string)v.Severity).ToLower())
                            .Select(g => new { severity = g.Key, count = g.Count() }),
                        // Case-insensitive — "No ID" and "no id" are treated as the same violation type
                        by_type = violations
                            .GroupBy(v => ((string)v.ViolationName).ToLower())
                            .Select(g => new
                            {
                                type = g.First().ViolationName,
                                count = g.Count()
                            })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/sao/students/{studentNo}/report
        // Returns full profile and complete violation history of a specific student
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

        // GET api/sao/users
        // Returns all registered users in the system regardless of role
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                List<dynamic> users = await _saoRepository.GetAllUsers();

                if (users == null || users.Count == 0)
                    return NotFound(new { status = 0, message = "No users found." });

                return Ok(new
                {
                    status = 1,
                    message = "Users retrieved successfully.",
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
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/sao/users/{id}
        // Returns one user by ID — call this first to see current values before updating
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 0, message = "User ID must be a positive number." });

            try
            {
                dynamic user = await _saoRepository.GetUserById(id);
                if (user == null)
                    return NotFound(new { status = 0, message = $"User with ID {id} not found." });

                return Ok(new
                {
                    status = 1,
                    message = "Success. Copy the fields below into the update request body and change only what you need.",
                    data = new
                    {
                        id = user.StudentID,
                        username = user.Username,
                        first_name = user.FirstName,
                        last_name = user.LastName,
                        email = user.Email,
                        role = user.Role,
                        gender = user.Gender,
                        course = user.Course,
                        year = user.Year,
                        address = user.Address,
                        contact_number = user.ContactNumber
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // PUT api/sao/users/{id}
        // Updates a user's info — only send the fields you want to change, the rest stay the same
        // Tip: call GET /api/sao/users/{id} first to see current values
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserModel request)
        {
            if (id <= 0)
                return BadRequest(new { status = 0, message = "User ID must be a positive number." });

            if (request == null)
                return BadRequest(new { status = 0, message = "Request body is required." });

            if (request.FirstName != null)
            {
                if (request.FirstName.Trim().Length < 2)
                    return BadRequest(new { status = 0, message = "First name must be at least 2 characters." });
                if (!Regex.IsMatch(request.FirstName.Trim(), @"^[a-zA-Z\s\-]+$"))
                    return BadRequest(new { status = 0, message = "First name must contain letters only." });
            }

            if (request.LastName != null)
            {
                if (request.LastName.Trim().Length < 2)
                    return BadRequest(new { status = 0, message = "Last name must be at least 2 characters." });
                if (!Regex.IsMatch(request.LastName.Trim(), @"^[a-zA-Z\s\-]+$"))
                    return BadRequest(new { status = 0, message = "Last name must contain letters only." });
            }

            if (request.Email != null &&
                !Regex.IsMatch(request.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest(new { status = 0, message = "Invalid email format." });

            if (request.ContactNumber != null &&
                !Regex.IsMatch(request.ContactNumber.Trim(), @"^09\d{9}$"))
                return BadRequest(new { status = 0, message = "Contact number must be 11 digits and start with 09 (e.g. 09123456789)." });

            if (request.Gender != null &&
                !ValidGenders.Contains(request.Gender.Trim().ToLower()))
                return BadRequest(new { status = 0, message = "Gender must be either 'male' or 'female'." });

            if (request.Course != null &&
                !ValidCourses.Contains(request.Course.Trim().ToLower()))
                return BadRequest(new
                {
                    status = 0,
                    message = "Invalid course. Accepted values are: BSIT, BSCS, BSBA, BSA, BSHM."
                });

            if (request.Year != null &&
                !ValidYears.Contains(request.Year.Trim()))
                return BadRequest(new
                {
                    status = 0,
                    message = "Invalid year. Accepted values are: 1, 2, 3, 4."
                });

            try
            {
                // Fetch current data — fields not in request body keep their existing values
                dynamic user = await _saoRepository.GetUserById(id);
                if (user == null)
                    return NotFound(new { status = 0, message = $"User with ID {id} not found." });

                var updated = new
                {
                    Id = (int)user.StudentID,
                    FirstName = request.FirstName?.Trim() ?? (string)user.FirstName,
                    LastName = request.LastName?.Trim() ?? (string)user.LastName,
                    Email = request.Email?.Trim().ToLower() ?? (string)user.Email,
                    ContactNumber = request.ContactNumber?.Trim() ?? (string)user.ContactNumber,
                    Gender = request.Gender?.Trim().ToLower() ?? (string)user.Gender,
                    Address = request.Address?.Trim() ?? (string)user.Address,
                    Course = request.Course?.Trim().ToUpper() ?? (string)user.Course,
                    Year = request.Year?.Trim() ?? (string)user.Year,
                    Role = (string)user.Role
                };

                await _saoRepository.UpdateUser(updated);

                return Ok(new
                {
                    status = 1,
                    message = "User updated successfully.",
                    data = new
                    {
                        id = updated.Id,
                        name = $"{updated.FirstName} {updated.LastName}",
                        email = updated.Email,
                        role = updated.Role,
                        course = updated.Course,
                        year = updated.Year
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // DELETE api/sao/users/{id}
        // Permanently removes a user from the system
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 0, message = "User ID must be a positive number." });

            try
            {
                dynamic user = await _saoRepository.GetUserById(id);
                if (user == null)
                    return NotFound(new { status = 0, message = $"User with ID {id} not found." });

                await _saoRepository.DeleteUser(id);

                return Ok(new
                {
                    status = 1,
                    message = $"User {user.Username} deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }
    }
}