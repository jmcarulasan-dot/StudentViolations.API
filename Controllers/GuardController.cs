using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Security.Claims;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/guard")]
    [Authorize(Roles = "Guard")]
    [ApiExplorerSettings(GroupName = "Guard")]
    public class GuardController : ControllerBase
    {
        private readonly IGuardRepository _guardRepository;
        private readonly INotificationRepository _notificationRepository;
        private static readonly Dictionary<string, string> ViolationSeverityMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["No ID"] = "minor",
            ["No Uniform"] = "minor",
            ["Piercing"] = "minor",
            ["Colored Hair"] = "minor",
            ["Disruptive Behavior"] = "moderate",
            ["Vandalism"] = "major",
            ["Prohibited Items"] = "critical",
            ["Other"] = "minor"
        };

        public GuardController(
            IGuardRepository guardRepository,
            INotificationRepository notificationRepository)
        {
            _guardRepository = guardRepository;
            _notificationRepository = notificationRepository;
        }

        // GET api/guard/student/validate?studentNo=xxx
        [HttpGet("student/validate")]
        public async Task<IActionResult> ValidateStudent([FromQuery] string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            var studentResult = await _guardRepository.GetStudentByQrCode(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            return Ok(new
            {
                status = 200,
                message = "Success",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}",
                    course = studentResult.Data.Course,
                    year = studentResult.Data.Year,
                    profile_photo = studentResult.Data.ProfilePhoto
                }
            });
        }

        // POST api/guard/student/violation
        [HttpPost("student/violation")]
        public async Task<IActionResult> RecordViolation([FromBody] RecordViolationModel request)
        {
            if (request == null)
                return BadRequest(new { status = 400, message = "Request body is required." });
            if (string.IsNullOrWhiteSpace(request.StudentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });
            if (string.IsNullOrWhiteSpace(request.ViolationType))
                return BadRequest(new { status = 400, message = "Violation type is required." });
            if (string.IsNullOrWhiteSpace(request.Details))
                return BadRequest(new { status = 400, message = "Details are required." });

            var guardId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(guardId))
                return Unauthorized(new { status = 401, message = "Guard ID not found in token. Please login again." });

            request.StudentNo = request.StudentNo.Trim().ToUpper();
            request.ViolationType = request.ViolationType.Trim();

            if (!ViolationSeverityMap.TryGetValue(request.ViolationType, out var severity))
                return BadRequest(new
                {
                    status = 400,
                    message = "Invalid violation type."
                });

            var studentResult = await _guardRepository.GetStudentByStudentNo(request.StudentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            var violation = new ViolationModel
            {
                StudentId = studentResult.Data.StudentID,
                ViolationName = request.ViolationType.Trim(),
                Description = request.Details.Trim(),
                Severity = severity,
                GuardId = guardId
            };

            var recordResult = await _guardRepository.RecordViolation(violation);
            if (recordResult.Status != 200)
                return StatusCode(recordResult.Status, recordResult);

            var violationsResult = await _guardRepository.GetViolationsByStudentId(request.StudentNo);
            var violations = violationsResult.Data ?? new List<ViolationModel>();
            int violationCount = violations.Count(v => !v.IsArchived);
            string studentName = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}";

            var usernameResult = await _guardRepository.GetUsernameByStudentNo(request.StudentNo);
            string studentUsername = usernameResult.Status == 200 ? usernameResult.Data : request.StudentNo;
            var guardUsername = User.FindFirstValue(ClaimTypes.Name);

            await _notificationRepository.SendToUser(
                targetUsername: studentUsername,
                title: "New Violation Recorded",
                message: $"A {severity} violation has been recorded against you: {request.ViolationType}."
            );
            await _notificationRepository.SendPushNotification(
                targetUsername: studentUsername,
                title: "New Violation Recorded",
                message: $"A {severity} violation has been recorded against you: {request.ViolationType}."
            );
            await _notificationRepository.SendToUser(
                targetUsername: guardUsername,
                title: "Violation Recorded",
                message: $"You have successfully recorded a {severity} violation for {studentName} ({request.StudentNo})."
            );

            if (violationCount == 1)
            {
                await _notificationRepository.SendToRole(
                    targetRole: "SAO",
                    title: "Student First Violation",
                    message: $"{studentName} ({request.StudentNo}) has received their first violation: {request.ViolationType}."
                );
            }
            else if (violationCount == 2)
            {
                await _notificationRepository.SendToRole(
                    targetRole: "SAO",
                    title: "Student Second Violation",
                    message: $"{studentName} ({request.StudentNo}) now has 2 violations. Consider scheduling counseling."
                );
            }
            else if (violationCount >= 3)
            {
                await _notificationRepository.SendToRole(
                    targetRole: "SAO",
                    title: "Student At Risk — 3+ Violations",
                    message: $"{studentName} ({request.StudentNo}) now has {violationCount} violations. Dismissal may be recommended."
                );
            }

            return Ok(new
            {
                status = 200,
                message = "Violation recorded successfully.",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = studentName,
                    new_violation_count = violationCount,
                    new_warning_level = ViolationHelper.GetWarningLevel(violationCount)
                }
            });
        }




    }
}