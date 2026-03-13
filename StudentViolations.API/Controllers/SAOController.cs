using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolationsAPI.IRepository;
using StudentViolationsAPI.Model.Entities;
using StudentViolationsAPI.Model.Requests;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/sao")]
    [ApiExplorerSettings(GroupName = "Admin")]
    [Authorize(Roles = "sao")]
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

        [HttpGet("violations")]
        public async Task<IActionResult> GetAllViolations()
        {
            try
            {
                var violations = await _violationRepository.GetAllViolations();
                return Ok(new
                {
                    status = 1,
                    message = "Success",
                    data = violations.Select(v => new
                    {
                        id = v.Id,
                        studentId = v.StudentId,
                        type = v.Type,
                        details = v.Details,
                        severity = v.Severity,
                        date = v.Date,
                        guardId = v.GuardId,
                        status = v.Status
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpGet("violations/status/{status}")]
        public async Task<IActionResult> GetViolationsByStatus(string status)
        {
            try
            {
                var violations = await _violationRepository.GetAllViolations();
                var filtered = violations
                    .Where(v => v.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    .Select(v => new
                    {
                        id = v.Id,
                        studentId = v.StudentId,
                        type = v.Type,
                        details = v.Details,
                        severity = v.Severity,
                        date = v.Date,
                        guardId = v.GuardId,
                        status = v.Status
                    });

                return Ok(new { status = 1, message = "Success", data = filtered });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpGet("violations/summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var violations = await _violationRepository.GetAllViolations();
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
                        bySeverity = violations
                            .GroupBy(v => v.Severity)
                            .Select(g => new { severity = g.Key, count = g.Count() }),
                        byType = violations
                            .GroupBy(v => v.Type)
                            .Select(g => new { type = g.Key, count = g.Count() })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpPut("violations/{id}/approve")]
        public async Task<IActionResult> ApproveViolation(int id)
        {
            try
            {
                var violation = await _violationRepository.GetViolationById(id);
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

        [HttpPut("violations/{id}/reject")]
        public async Task<IActionResult> RejectViolation(int id)
        {
            try
            {
                var violation = await _violationRepository.GetViolationById(id);
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

        [HttpDelete("violations/{id}")]
        public async Task<IActionResult> DeleteViolation(int id)
        {
            try
            {
                var violation = await _violationRepository.GetViolationById(id);
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

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _saoRepository.GetAllUsers();
                return Ok(new
                {
                    status = 1,
                    message = "Users retrieved successfully.",
                    data = users.Select(u => new
                    {
                        id = u.Id,
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

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _saoRepository.GetUserById(id);
                if (user == null)
                    return NotFound(new { status = 0, message = "User not found." });

                user.FirstName = request.FirstName ?? user.FirstName;
                user.LastName = request.LastName ?? user.LastName;
                user.Email = request.Email ?? user.Email;
                user.ContactNumber = request.ContactNumber ?? user.ContactNumber;
                user.Gender = request.Gender ?? user.Gender;
                user.Address = request.Address ?? user.Address;
                user.Course = request.Course ?? user.Course;
                user.Year = request.Year ?? user.Year;
                user.Role = request.Role ?? user.Role;

                await _saoRepository.UpdateUser(user);

                return Ok(new
                {
                    status = 1,
                    message = "User updated successfully.",
                    data = new
                    {
                        id = user.Id,
                        username = user.Username,
                        name = $"{user.FirstName} {user.LastName}",
                        role = user.Role,
                        email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 0, message = ex.Message });
            }
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _saoRepository.GetUserById(id);
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
    }
}