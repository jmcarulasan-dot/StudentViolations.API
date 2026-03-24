using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.Model;
using StudentViolationsAPI.IRepository;
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

        // GET api/sao/violations/{id}
        // Returns a single violation by ID with student number and guard name attached
        [HttpGet("violations/{id}")]
        public async Task<IActionResult> GetViolationById(int id)
        {
            if (id <= 0)
                return BadRequest(new { status = 0, message = "Violation ID must be a positive number." });

            try
            {
                dynamic violation = await _violationRepository.GetViolationById(id);
                if (violation == null)
                    return NotFound(new { status = 0, message = $"Violation with ID {id} not found." });

                List<dynamic> students = await _studentRepository.GetAllStudents();

                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    data = new
                    {
                        id = violation.ViolationID,
                        student_no = students
                            .FirstOrDefault(s => s.StudentID == violation.StudentId)?.StudentNo,
                        type = violation.ViolationName,
                        details = violation.Description,
                        severity = violation.Severity,
                        date = violation.ViolationDate,
                        recorded_by = violation.GuardName,
                        status = violation.Status
                    }
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
        // Permanently removes a violation from the database
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

                await _violationRepository.DeleteViolation(id);
                return Ok(new { status = 1, message = "Violation deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        // GET api/sao/violations/summary
        // Returns total violation counts grouped by status, severity, and type
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
                        pending = violations.Count(v => v.Status == "Pending"),
                        approved = violations.Count(v => v.Status == "Approved"),
                        rejected = violations.Count(v => v.Status == "Rejected"),
                        by_severity = violations
                            .GroupBy(v => (string)v.Severity)
                            .Select(g => new { severity = g.Key, count = g.Count() }),
                        by_type = violations
                            .GroupBy(v => (string)v.ViolationName)
                            .Select(g => new { type = g.Key, count = g.Count() })
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

        // PUT api/sao/users/{id}
        // Updates a user's info — role cannot be changed here
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

            try
            {
                // Get the current user data so we can fall back to existing values if a field is null
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
                    Course = request.Course?.Trim() ?? (string)user.Course,
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
                        role = updated.Role,
                        email = updated.Email
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