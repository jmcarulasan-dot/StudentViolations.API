using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.Helpers;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;

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
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var result = await _studentRepository.GetAllStudents();
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var students = result.Data ?? new List<StudentModel>();
            if (students.Count == 0)
                return NotFound(new { status = 404, message = "No students found." });

            // Get violations for each student to calculate warning level
            var studentList = new List<object>();
            foreach (var student in students)
            {
                var violationsResult = await _violationRepository.GetViolationsByStudentId(student.StudentNo);
                var violations = violationsResult.Data ?? new List<ViolationModel>();

                studentList.Add(new
                {
                    student_no = student.StudentNo,
                    name = $"{student.FirstName} {student.LastName}",
                    email = student.Email,
                    contact_number = student.ContactNumber,
                    gender = student.Gender,
                    violation_count = violations.Count,
                    warning_level = ViolationHelper.GetWarningLevel(violations.Count),
                    recommended_action = ViolationHelper.GetRecommendedAction(violations.Count),
                });
            }
            return Ok(new { status = 200, message = "Success", total = studentList.Count, data = studentList });
        }
        // GET api/guidance/students/{studentNo}/report
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
        // GET api/guidance/violations/by-status
        [HttpGet("violations/by-status")]
        public async Task<IActionResult> GetViolationsByStatus()
        {
            var result = await _violationRepository.GetAllViolations();
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var violations = result.Data ?? new List<ViolationModel>();
            if (violations.Count == 0)
                return NotFound(new { status = 404, message = "No violations found." });

            var studentsResult = await _studentRepository.GetAllStudents();
            var students = studentsResult.Data ?? new List<StudentModel>();

            var statuses = new[] { "Pending", "Approved", "Rejected" };
            var grouped = statuses.Select(s => new
            {
                status = s,
                count = violations.Count(v => v.Status.Equals(s, StringComparison.OrdinalIgnoreCase)),
                violations = violations
                    .Where(v => v.Status.Equals(s, StringComparison.OrdinalIgnoreCase))
                    .Select(v => new
                    {
                        id = v.ViolationID,
                        student_no = students.FirstOrDefault(st => st.StudentID == v.StudentId)?.StudentNo,
                        type = v.ViolationName,
                        details = v.Description,
                        severity = v.Severity,
                        date = v.ViolationDate,
                        recorded_by = v.GuardName
                    })
            });
            return Ok(new { status = 200, message = "Success", total = violations.Count, data = grouped });
        }
        // GET api/guidance/violations/by-severity
        [HttpGet("violations/by-severity")]
        public async Task<IActionResult> GetViolationsBySeverity()
        {
            var result = await _violationRepository.GetAllViolations();
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var violations = result.Data ?? new List<ViolationModel>();
            if (violations.Count == 0)
                return NotFound(new { status = 404, message = "No violations found." });

            var studentsResult = await _studentRepository.GetAllStudents();
            var students = studentsResult.Data ?? new List<StudentModel>();

            var grouped = violations
                .GroupBy(v => v.Severity)
                .Select(g => new
                {
                    severity = g.Key,
                    count = g.Count(),
                    violations = g.Select(v => new
                    {
                        id = v.ViolationID,
                        student_no = students.FirstOrDefault(s => s.StudentID == v.StudentId)?.StudentNo,
                        type = v.ViolationName,
                        details = v.Description,
                        date = v.ViolationDate,
                        status = v.Status,
                        recorded_by = v.GuardName
                    })
                });
            return Ok(new { status = 200, message = "Success", data = grouped });
        }

        // POST api/guidance/violations/{id}/appeal/review
        [HttpPost("violations/{id}/appeal/review")]
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
        
        // PUT api/guidance/students/{studentNo}/warn
        [HttpPut("students/{studentNo}/warn")]
        public async Task<IActionResult> WarnStudent(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo.Trim().ToUpper());
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            var result = await _studentRepository.UpdateStudentStatus(studentResult.Data.StudentID, "Warned");
            return StatusCode(result.Status, new { status = result.Status, message = result.Message });
        }

        // PUT api/guidance/students/{studentNo}/recommend-dismiss
        [HttpPut("students/{studentNo}/recommend-dismiss")]
        public async Task<IActionResult> RecommendDismissal(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo.Trim().ToUpper());
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            var violationsResult = await _violationRepository.GetViolationsByStudentId(studentNo.Trim().ToUpper());
            var violations = violationsResult.Data ?? new List<ViolationModel>();

            if (violations.Count < 3)
                return BadRequest(new { status = 400, message = "Student must have at least 3 violations to recommend dismissal." });

            var result = await _studentRepository.UpdateStudentStatus(studentResult.Data.StudentID, "PendingDismissal");
            return StatusCode(result.Status, new { status = result.Status, message = result.Message });
        }
    }
}