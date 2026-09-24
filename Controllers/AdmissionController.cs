using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/admission")]
    [Authorize(Roles = "Admission")]
    [ApiExplorerSettings(GroupName = "Admission")]
    public class AdmissionController : ControllerBase
    {
        private readonly IAdmissionRepository _admissionRepository;

        public AdmissionController(IAdmissionRepository admissionRepository)
        {
            _admissionRepository = admissionRepository;
        }

        // GET api/admission/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _admissionRepository.GetAllUsers();

            if (result.Status != 200)
                return StatusCode(result.Status, new
                {
                    status = result.Status,
                    message = result.Message
                });

            return Ok(new
            {
                status = 200,
                message = result.Message,
                data = result.Data
            });
        }

        // GET api/admission/users/{id}
        [HttpGet("users/{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await _admissionRepository.GetUserById(id);

            if (result.Status != 200)
                return StatusCode(result.Status, new
                {
                    status = result.Status,
                    message = result.Message
                });

            return Ok(new
            {
                status = 200,
                message = result.Message,
                data = result.Data
            });
        }

        // PUT api/admission/users/{id}
        [HttpPut("users/{id:int}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            [FromBody] UpdateUserModel request)
        {
            if (request == null)
                return BadRequest(new
                {
                    status = 400,
                    message = "Request body is required."
                });

            var validRoles = new[] { "Student", "Guard", "SAO", "Admission" };

            if (string.IsNullOrWhiteSpace(request.Role) ||
                !validRoles.Contains(request.Role.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    status = 400,
                    message = "Role must be Student, Guard, SAO, or Admission."
                });
            }

            var existing = await _admissionRepository.GetUserById(id);

            if (existing.Status != 200 || existing.Data == null)
                return StatusCode(existing.Status, new
                {
                    status = existing.Status,
                    message = existing.Message
                });

            existing.Data.FirstName = request.FirstName ?? existing.Data.FirstName;
            existing.Data.LastName = request.LastName ?? existing.Data.LastName;
            existing.Data.Email = request.Email ?? existing.Data.Email;
            existing.Data.ContactNumber = request.ContactNumber ?? existing.Data.ContactNumber;
            existing.Data.Gender = request.Gender ?? existing.Data.Gender;
            existing.Data.Address = request.Address ?? existing.Data.Address;
            existing.Data.Role = request.Role.Trim();

            var result = await _admissionRepository.UpdateUser(existing);

            if (result.Status != 200)
                return StatusCode(result.Status, new
                {
                    status = result.Status,
                    message = result.Message
                });

            return Ok(new
            {
                status = 200,
                message = result.Message
            });
        }

        // DELETE api/admission/users/{id}
        [HttpDelete("users/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _admissionRepository.DeleteUser(id);

            if (result.Status != 200)
                return StatusCode(result.Status, new
                {
                    status = result.Status,
                    message = result.Message
                });

            return Ok(new
            {
                status = 200,
                message = result.Message
            });
        }

        // GET api/admission/students/{studentNo}/qrcode
        [HttpGet("students/{studentNo}/qrcode")]
        public async Task<IActionResult> GetStudentQrCode(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new
                {
                    status = 400,
                    message = "Student number is required."
                });

            var result = await _admissionRepository.GetStudentQrCode(
                studentNo.Trim().ToUpper());

            if (result.Status != 200)
                return StatusCode(result.Status, new
                {
                    status = result.Status,
                    message = result.Message
                });

            return Ok(new
            {
                status = 200,
                message = result.Message,
                student_no = studentNo.Trim().ToUpper(),
                qr_code = result.Data
            });
        }
    }
}
