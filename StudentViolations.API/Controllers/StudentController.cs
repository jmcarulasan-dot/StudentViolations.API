using Microsoft.AspNetCore.Mvc;
using StudentViolationsAPI.IRepository;
using StudentViolationsAPI.Model.Entities;
using StudentViolationsAPI.Model.Requests;

namespace StudentViolationsAPI.Controllers
{
    [ApiController]
    [Route("students")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IViolationRepository _violationRepository;

        public StudentController(IStudentRepository studentRepository, IViolationRepository violationRepository)
        {
            _studentRepository = studentRepository;
            _violationRepository = violationRepository;
        }
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateStudent([FromBody] ValidateStudentRequest request)
        {
            try
            {
                var student = await _studentRepository.GetStudentByQrCode(request.QrCode);
                if (student == null)
                {
                    return NotFound(new { status = "error", message = "Student not found." });
                }

                return Ok(new
                {
                    status = "success",
                    student_id = student.Id,
                    name = student.Name,
                    violation_count = student.ViolationCount,
                    warning_level = GetWarningLevel(student.ViolationCount)
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { status = "error", message = "An error occurred." });
            }
        }

        [HttpPost("violations")]
        public async Task<IActionResult> RecordViolation([FromBody] RecordViolationRequest request)
        {
            try
            {
                var student = await _studentRepository.GetStudentByQrCode(request.GuardId);
                if (student == null)
                {
                    return BadRequest(new { status = "error", message = "Invalid Student." });
                }

                var violation = new Violation
                {
                    StudentId = request.StudentId,
                    Type = request.ViolationType,
                    Details = request.Details,
                    Date = DateTime.Now,
                    GuardId = request.GuardId
                };

                await _violationRepository.RecordViolation(violation);

                student.ViolationCount++;
                await _studentRepository.UpdateStudent(student);

                return Ok(new
                {
                    status = "success",
                    message = "Violation recorded.",
                    new_violation_count = student.ViolationCount,
                    new_warning_level = GetWarningLevel(student.ViolationCount)
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { status = "error", message = "An error occurred." });
            }
        }

        private string GetWarningLevel(int violationCount)
        {
            if (violationCount >= 3)
            {
                return "red";
            }
            else if (violationCount == 2)
            {
                return "orange";
            }
            else if (violationCount == 1)
            {
                return "yellow";
            }
            else
            {
                return "green";
            }
        }
    }
}