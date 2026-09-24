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
    [ApiExplorerSettings(GroupName = "SAO")]
    [Authorize(Roles = "SAO")]
    public class SAOController : ControllerBase
    {
        private readonly IViolationRepository _violationRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ISAORepository _saoRepository;
        private readonly INotificationRepository _notificationRepository;
        private static readonly string[] ValidGenders = { "male", "female" };

        public SAOController(
            IViolationRepository violationRepository,
            IStudentRepository studentRepository,
            ISAORepository saoRepository,
            INotificationRepository notificationRepository)
        {
            _violationRepository = violationRepository;
            _studentRepository = studentRepository;
            _saoRepository = saoRepository;
            _notificationRepository = notificationRepository;
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

            return Ok(new
            {
                status = 200,
                message = "Success",
                total = violations.Count,
                data = violations.Select(v => new
                {
                    id = v.ViolationID,
                    student_no = v.StudentNo,
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

            var filtered = (result.Data ?? new List<ViolationModel>())
                .Where(v => v.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .Select(v => new
                {
                    id = v.ViolationID,
                    student_no = v.StudentNo,
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
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var usernameResult = await _studentRepository.GetUsernameByStudentNo(violationResult.Data.StudentNo);
            string studentUsername = usernameResult.Status == 200 ? usernameResult.Data : violationResult.Data.StudentNo;

            await _notificationRepository.SendToUser(
                targetUsername: studentUsername,
                title: "Violation Approved",
                message: $"Your violation record for '{violationResult.Data.ViolationName}' has been approved by the SAO office."
            );
            await _notificationRepository.SendPushNotification(
                targetUsername: studentUsername,
                title: "Violation Approved",
                message: $"Your violation record for '{violationResult.Data.ViolationName}' has been approved by the SAO office."
            );

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
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var usernameResult = await _studentRepository.GetUsernameByStudentNo(violationResult.Data.StudentNo);
            string studentUsername = usernameResult.Status == 200 ? usernameResult.Data : violationResult.Data.StudentNo;

            await _notificationRepository.SendToUser(
                targetUsername: studentUsername,
                title: "Violation Rejected",
                message: $"Your violation record for '{violationResult.Data.ViolationName}' has been rejected by the SAO office."
            );
            await _notificationRepository.SendPushNotification(
                targetUsername: studentUsername,
                title: "Violation Rejected",
                message: $"Your violation record for '{violationResult.Data.ViolationName}' has been rejected by the SAO office."
            );

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
            var activeViolations = violations
                .Where(v => !v.IsArchived &&
                            (v.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                             v.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)))
                .ToList();

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
                    violation_count = activeViolations.Count,
                    warning_level = ViolationHelper.GetWarningLevel(activeViolations.Count),
                    recommended_action = ViolationHelper.GetRecommendedAction(activeViolations.Count),
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





        // PUT api/sao/students/{studentNo}/recommend-dismiss
        [HttpPut("students/{studentNo}/recommend-dismiss")]
        public async Task<IActionResult> RecommendDismissal(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);

            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new
                {
                    status = studentResult.Status,
                    message = studentResult.Message
                });

            var violationsResult =
                await _violationRepository.GetViolationsByStudentId(studentNo);

            var violations = violationsResult.Data ?? new List<ViolationModel>();

            var activeViolations = violations
                .Where(v => !v.IsArchived &&
                            (v.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                             v.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (activeViolations.Count < 3)
                return BadRequest(new
                {
                    status = 400,
                    message = "Student must have at least 3 active violations before dismissal can be recommended."
                });

            var result = await _studentRepository.UpdateStudentStatus(
                studentResult.Data.StudentID,
                "PendingDismissal"
            );

            if (result.Status != 200)
                return StatusCode(result.Status, new
                {
                    status = result.Status,
                    message = result.Message
                });

            var usernameResult =
                await _studentRepository.GetUsernameByStudentNo(studentNo);

            string studentUsername =
                usernameResult.Status == 200
                    ? usernameResult.Data
                    : studentNo;

            await _notificationRepository.SendToUser(
                targetUsername: studentUsername,
                title: "Dismissal Recommended",
                message: "You have reached 3 or more active violations. Your case has been recommended for dismissal and is awaiting SAO review."
            );

            await _notificationRepository.SendPushNotification(
                targetUsername: studentUsername,
                title: "Dismissal Recommended",
                message: "You have reached 3 or more active violations. Your case has been recommended for dismissal and is awaiting SAO review."
            );

            return Ok(new
            {
                status = 200,
                message = "Student recommended for dismissal successfully."
            });
        }
        // PUT api/sao/students/{studentNo}/counsel
        [HttpPut("students/{studentNo}/counsel")]
        public async Task<IActionResult> CounselStudent(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);

            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new
                {
                    status = studentResult.Status,
                    message = studentResult.Message
                });

            var violationsResult =
                await _violationRepository.GetViolationsByStudentId(studentNo);

            var violations = violationsResult.Data ?? new List<ViolationModel>();

            var activeViolations = violations
                .Where(v => !v.IsArchived &&
                            (v.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                             v.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (activeViolations.Count != 2)
                return BadRequest(new
                {
                    status = 400,
                    message = "Counseling applies when the student has exactly 2 active violations."
                });

            var result = await _studentRepository.UpdateStudentStatus(
                studentResult.Data.StudentID,
                "Counseling"
            );

            if (result.Status != 200)
                return StatusCode(result.Status, new
                {
                    status = result.Status,
                    message = result.Message
                });

            var usernameResult =
                await _studentRepository.GetUsernameByStudentNo(studentNo);

            string username =
                usernameResult.Status == 200
                    ? usernameResult.Data
                    : studentNo;

            await _notificationRepository.SendToUser(
                username,
                "Counseling Required",
                "You have reached the Orange violation level. Please attend counseling with the SAO office."
            );

            await _notificationRepository.SendPushNotification(
                username,
                "Counseling Required",
                "You have reached the Orange violation level. Please attend counseling with the SAO office."
            );

            return Ok(new
            {
                status = 200,
                message = "Student marked for counseling successfully."
            });
        }
        // GET api/sao/students/{studentNo}/violation-level
        [HttpGet("students/{studentNo}/violation-level")]
        public async Task<IActionResult> GetViolationLevel(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            var violationsResult =
                await _violationRepository.GetViolationsByStudentId(studentNo);

            if (violationsResult.Status != 200)
                return StatusCode(violationsResult.Status, new
                {
                    status = violationsResult.Status,
                    message = violationsResult.Message
                });

            var violations = violationsResult.Data ?? new List<ViolationModel>();

            var activeCount = violations.Count(v =>
                !v.IsArchived &&
                (v.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                 v.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)));

            string level = activeCount switch
            {
                0 => "Green",
                1 => "Yellow",
                2 => "Orange",
                _ => "Red"
            };

            string action = activeCount switch
            {
                0 => "No Action",
                1 => "Warning",
                2 => "Counseling",
                _ => "Dismissal Recommendation"
            };

            return Ok(new
            {
                status = 200,
                studentNo,
                activeViolations = activeCount,
                violationLevel = level,
                recommendedAction = action
            });
        }
        // PUT api/sao/students/{studentNo}/warn
        [HttpPut("students/{studentNo}/warn")]
        public async Task<IActionResult> WarnStudent(string studentNo)
        {
            if (string.IsNullOrWhiteSpace(studentNo))
                return BadRequest(new { status = 400, message = "Student number is required." });

            studentNo = studentNo.Trim().ToUpper();

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);

            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new
                {
                    status = studentResult.Status,
                    message = studentResult.Message
                });

            var violationsResult =
                await _violationRepository.GetViolationsByStudentId(studentNo);

            var violations = violationsResult.Data ?? new List<ViolationModel>();

            var activeViolations = violations
                .Where(v => !v.IsArchived &&
                            (v.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                             v.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (activeViolations.Count != 1)
                return BadRequest(new
                {
                    status = 400,
                    message = "Warning applies when the student has exactly 1 active violation."
                });

            var result = await _studentRepository.UpdateStudentStatus(
                studentResult.Data.StudentID,
                "Warned"
            );

            if (result.Status != 200)
                return StatusCode(result.Status, new
                {
                    status = result.Status,
                    message = result.Message
                });

            var usernameResult =
                await _studentRepository.GetUsernameByStudentNo(studentNo);

            string username =
                usernameResult.Status == 200
                    ? usernameResult.Data
                    : studentNo;

            await _notificationRepository.SendToUser(
                username,
                "Violation Warning",
                "You have reached the Yellow violation level. This is an official warning."
            );

            await _notificationRepository.SendPushNotification(
                username,
                "Violation Warning",
                "You have reached the Yellow violation level. This is an official warning."
            );

            return Ok(new
            {
                status = 200,
                message = "Student warned successfully."
            });
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
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var usernameResult = await _studentRepository.GetUsernameByStudentNo(studentNo);
            string studentUsername = usernameResult.Status == 200 ? usernameResult.Data : studentNo;

            await _notificationRepository.SendToUser(
                targetUsername: studentUsername,
                title: "Dismissal Cancelled",
                message: "Your dismissal has been cancelled by the SAO office. Your account is now active again."
            );
            await _notificationRepository.SendPushNotification(
                targetUsername: studentUsername,
                title: "Dismissal Cancelled",
                message: "Your dismissal has been cancelled by the SAO office. Your account is now active again."
            );

            return StatusCode(result.Status, new { status = result.Status, message = result.Message });
        }

        // GET api/sao/students/dismissed
        [HttpGet("students/dismissed")]
        public async Task<IActionResult> GetDismissedStudents()
        {
            var result = await _saoRepository.GetDismissedStudents();
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            return Ok(new { status = 200, message = "Success", data = result.Data });
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
                return BadRequest(new { status = 400, message = "Student has not been recommended for dismissal." });

            var result = await _studentRepository.UpdateStudentStatus(studentResult.Data.StudentID, "Dismissed");
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            // Archive all violations so count resets
            await _violationRepository.ArchiveViolations(studentResult.Data.StudentID);

            string studentName = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}";

            var usernameResult = await _studentRepository.GetUsernameByStudentNo(studentNo);
            string studentUsername = usernameResult.Status == 200 ? usernameResult.Data : studentNo;

            await _notificationRepository.SendToUser(
                targetUsername: studentUsername,
                title: "Account Dismissed",
                message: "Your account has been dismissed by the SAO office. You are no longer permitted to use the system. Please contact the school for further information."
            );
            await _notificationRepository.SendPushNotification(
                targetUsername: studentUsername,
                title: "Account Dismissed",
                message: "Your account has been dismissed by the SAO office. You are no longer permitted to use the system. Please contact the school for further information."
            );

            await _notificationRepository.SendToRole(
                targetRole: "SAO",
                title: "Dismissal Finalized",
                message: $"SAO has finalized the dismissal of {studentName} ({studentNo})."
            );

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
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            string outcomeMessage = request.AppealStatus.Trim() == "Approved"
                ? $"Your appeal for violation '{violationResult.Data.ViolationName}' has been approved by the SAO office."
                : $"Your appeal for violation '{violationResult.Data.ViolationName}' has been rejected. Remarks: {request.AppealRemarks ?? "None"}";

            var usernameResult = await _studentRepository.GetUsernameByStudentNo(violationResult.Data.StudentNo);
            string studentUsername = usernameResult.Status == 200 ? usernameResult.Data : violationResult.Data.StudentNo;

            await _notificationRepository.SendToUser(
                targetUsername: studentUsername,
                title: $"Appeal {request.AppealStatus.Trim()}",
                message: outcomeMessage
            );
            await _notificationRepository.SendPushNotification(
                targetUsername: studentUsername,
                title: $"Appeal {request.AppealStatus.Trim()}",
                message: outcomeMessage
            );

            return StatusCode(result.Status, new { status = result.Status, message = result.Message });
        }

        // GET api/sao/violations/appeals
        [HttpGet("violations/appeals")]
        public async Task<IActionResult> GetPendingAppeals()
        {
            var result = await _violationRepository.GetAllViolations();
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            var violations = result.Data ?? new List<ViolationModel>();

            var pendingAppeals = violations
                .Where(v => v.AppealStatus == "Pending")
                .Select(v => new
                {
                    id = v.ViolationID,
                    student_no = v.StudentNo,
                    type = v.ViolationName,
                    severity = v.Severity,
                    date = v.ViolationDate,
                    status = v.Status,
                    appeal_text = v.AppealText,
                    appeal_status = v.AppealStatus,
                    recorded_by = v.GuardName
                }).ToList();

            if (pendingAppeals.Count == 0)
                return NotFound(new { status = 404, message = "No pending appeals found." });

            return Ok(new
            {
                status = 200,
                message = "Success",
                total = pendingAppeals.Count,
                data = pendingAppeals
            });
        }
    }
}