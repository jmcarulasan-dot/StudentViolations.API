using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.Model;
using StudentViolationsAPI.IRepository;

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

                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    data = violations.Select(v => new
                    {
                        id = v.ViolationID,
                        // Match the violation's StudentId to the Students list to get their StudentNo
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
            try
            {
                dynamic violation = await _violationRepository.GetViolationById(id);
                if (violation == null)
                    return NotFound(new { status = 0, message = "Violation not found." });

                List<dynamic> students = await _studentRepository.GetAllStudents();

                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    data = new
                    {
                        id = violation.ViolationID,
                        // Match the violation's StudentId to the Students list to get their StudentNo
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
            // Validate status value before querying
            var validStatuses = new[] { "pending", "approved", "rejected" };
            if (!validStatuses.Contains(status.ToLower()))
            {
                return BadRequest(new
                {
                    status = 0,
                    message = $"Invalid status value '{status}'. Accepted values are: Pending, Approved, Rejected."
                });
            }

            try
            {
                List<dynamic> violations = await _violationRepository.GetAllViolations();
                List<dynamic> students = await _studentRepository.GetAllStudents();

                // Filter by status (case-insensitive) and attach student number to each result
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
                    });

                return Ok(new { status = 1, message = "Success", data = filtered });
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
            try
            {
                // Check if the violation exists before trying to update it
                dynamic violation = await _violationRepository.GetViolationById(id);
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

        // PUT api/sao/violations/{id}/reject
        // Changes a violation's status to Rejected
        [HttpPut("violations/{id}/reject")]
        public async Task<IActionResult> RejectViolation(int id)
        {
            try
            {
                // Check if the violation exists before trying to update it
                dynamic violation = await _violationRepository.GetViolationById(id);
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

        // DELETE api/sao/violations/{id}
        // Permanently removes a violation from the database
        [HttpDelete("violations/{id}")]
        public async Task<IActionResult> DeleteViolation(int id)
        {
            try
            {
                // Check if the violation exists before trying to delete it
                dynamic violation = await _violationRepository.GetViolationById(id);
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

        // GET api/sao/violations/summary
        // Returns total violation counts grouped by status, severity, and type
        [HttpGet("violations/summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                List<dynamic> violations = await _violationRepository.GetAllViolations();

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
                        // Group by severity to show how many minor/moderate/major/critical violations exist
                        by_severity = violations
                            .GroupBy(v => (string)v.Severity)
                            .Select(g => new { severity = g.Key, count = g.Count() }),
                        // Group by type to show the most common violations
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
            try
            {
                dynamic student = await _studentRepository.GetStudentByStudentId(studentNo);
                if (student == null)
                    return NotFound(new { status = 0, message = "Student not found." });

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

                return Ok(new
                {
                    status = 1,
                    message = "Users retrieved successfully.",
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
            try
            {
                // Get the current user data so we can fall back to existing values if a field is null
                dynamic user = await _saoRepository.GetUserById(id);
                if (user == null)
                    return NotFound(new { status = 0, message = "User not found." });

                var updated = new
                {
                    Id = (int)user.StudentID,
                    FirstName = request.FirstName ?? (string)user.FirstName,
                    LastName = request.LastName ?? (string)user.LastName,
                    Email = request.Email ?? (string)user.Email,
                    ContactNumber = request.ContactNumber ?? (string)user.ContactNumber,
                    Gender = request.Gender ?? (string)user.Gender,
                    Address = request.Address ?? (string)user.Address,
                    Course = request.Course ?? (string)user.Course,
                    Year = request.Year ?? (string)user.Year,
                    // Role is never changed — always keep the existing role
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
            try
            {
                // Check if the user exists before trying to delete
                dynamic user = await _saoRepository.GetUserById(id);
                if (user == null)
                    return NotFound(new { status = 0, message = "User not found." });

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