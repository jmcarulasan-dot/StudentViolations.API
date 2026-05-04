using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.Helpers;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Security.Claims;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/student")]
    [Authorize(Roles = "Student")]
    [ApiExplorerSettings(GroupName = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IViolationRepository _violationRepository;
        private readonly INotificationRepository _notificationRepository;

        public StudentController(
            IStudentRepository studentRepository,
            IViolationRepository violationRepository,
            INotificationRepository notificationRepository)
        {
            _studentRepository = studentRepository;
            _violationRepository = violationRepository;
            _notificationRepository = notificationRepository;
        }

        private string GetLoggedInStudentNo()
        {
            var studentNo = User.FindFirstValue("studentNo");
            return string.IsNullOrWhiteSpace(studentNo) ? string.Empty : studentNo;
        }

        // GET api/student/violations
        [HttpGet("violations")]
        public async Task<IActionResult> GetMyViolations()
        {
            string studentNo = GetLoggedInStudentNo();
            if (string.IsNullOrEmpty(studentNo))
                return Unauthorized(new { status = 401, message = "Student number not found in token. Please login again." });

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            var violationsResult = await _violationRepository.GetViolationsByStudentId(studentNo);
            var violations = violationsResult.Data ?? new List<ViolationModel>();

            var activeViolations = violations.Where(v => !v.IsArchived).ToList();

            int pending = activeViolations.Count(v => v.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
            int approved = activeViolations.Count(v => v.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase));
            int rejected = activeViolations.Count(v => v.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase));

            return Ok(new
            {
                status = 200,
                message = "Violations retrieved successfully.",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}",
                    total_violations = activeViolations.Count,
                    pending,
                    approved,
                    rejected,
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
                        recorded_by = v.GuardName,
                        appeal_text = v.AppealText,
                        appeal_status = v.AppealStatus,
                        appeal_remarks = v.AppealRemarks,
                        is_archived = v.IsArchived
                    })
                }
            });
        }

        // GET api/student/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            string studentNo = GetLoggedInStudentNo();
            if (string.IsNullOrEmpty(studentNo))
                return Unauthorized(new { status = 401, message = "Student number not found in token. Please login again." });

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            var violationsResult = await _violationRepository.GetViolationsByStudentId(studentNo);
            var violations = violationsResult.Data ?? new List<ViolationModel>();

            return Ok(new
            {
                status = 200,
                message = "Profile retrieved successfully.",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}",
                    email = studentResult.Data.Email,
                    gender = studentResult.Data.Gender,
                    course = studentResult.Data.Course,
                    year = studentResult.Data.Year,
                    contact_number = studentResult.Data.ContactNumber,
                    address = studentResult.Data.Address,
                    status = studentResult.Data.Status,
                    total_violations = violations.Count,
                    warning_level = ViolationHelper.GetWarningLevel(violations.Count),
                    profile_photo = studentResult.Data.ProfilePhoto
                }
            });
        }

        // GET api/student/qrcode
        [HttpGet("qrcode")]
        public async Task<IActionResult> GetMyQrCode()
        {
            string studentNo = GetLoggedInStudentNo();
            if (string.IsNullOrEmpty(studentNo))
                return Unauthorized(new { status = 401, message = "Student number not found in token. Please login again." });

            var studentResult = await _studentRepository.GetStudentByStudentId(studentNo);
            if (studentResult.Status != 200)
                return StatusCode(studentResult.Status, new { status = studentResult.Status, message = studentResult.Message });

            if (studentResult.Data.QRCode == null)
                return NotFound(new { status = 404, message = "QR code not found for this student." });

            return Ok(new
            {
                status = 200,
                message = "QR code retrieved successfully.",
                data = new
                {
                    student_no = studentResult.Data.StudentNo,
                    name = $"{studentResult.Data.FirstName} {studentResult.Data.LastName}",
                    qr_code = studentResult.Data.QRCode
                }
            });
        }

        // POST api/student/violations/{id}/appeal
        [HttpPost("violations/{id}/appeal")]
        public async Task<IActionResult> SubmitAppeal(int id, [FromBody] SubmitAppealModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AppealText))
                return BadRequest(new { status = 400, message = "Appeal text is required." });

            string studentNo = GetLoggedInStudentNo();
            if (string.IsNullOrEmpty(studentNo))
                return Unauthorized(new { status = 401, message = "Student number not found in token. Please login again." });

            var violationsResult = await _violationRepository.GetViolationsByStudentId(studentNo);
            var violations = violationsResult.Data ?? new List<ViolationModel>();
            var violation = violations.FirstOrDefault(v => v.ViolationID == id);

            if (violation == null)
                return NotFound(new { status = 404, message = "Violation not found." });

            if (violation.AppealStatus == "Pending" || violation.AppealStatus == "Approved")
                return BadRequest(new { status = 400, message = "You have already submitted an appeal for this violation." });

            var result = await _violationRepository.SubmitAppeal(id, request.AppealText.Trim());
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            await _notificationRepository.SendToRole(
                targetRole: "guidance",
                title: "New Appeal Submitted",
                message: $"Student {studentNo} has submitted an appeal for violation #{id}: {violation.ViolationName}."
            );

            await _notificationRepository.SendToRole(
                targetRole: "sao",
                title: "New Appeal Submitted",
                message: $"Student {studentNo} has submitted an appeal for violation #{id}: {violation.ViolationName}."
            );

            return StatusCode(result.Status, new { status = result.Status, message = result.Message });
        }

        // PUT api/student/profile/photo
        [HttpPut("profile/photo")]
        public async Task<IActionResult> UpdateProfilePhoto([FromBody] UploadPhotoModel request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Base64Photo))
                return BadRequest(new { status = 400, message = "Photo data is required." });

            if (request.Base64Photo.Length > 3_000_000)
                return BadRequest(new { status = 400, message = "Photo is too large. Maximum size is ~2MB." });

            // Validate it's actually a Base64 image (JPEG or PNG)
            if (!request.Base64Photo.StartsWith("/9j/") &&   
                !request.Base64Photo.StartsWith("iVBOR"))     
                return BadRequest(new { status = 400, message = "Only JPEG and PNG images are accepted." });

            string studentNo = GetLoggedInStudentNo();
            if (string.IsNullOrEmpty(studentNo))
                return Unauthorized(new { status = 401, message = "Student number not found in token. Please login again." });

            var result = await _studentRepository.UpdateProfilePhoto(studentNo, request.Base64Photo);
            if (result.Status != 200)
                return StatusCode(result.Status, new { status = result.Status, message = result.Message });

            return Ok(new { status = 200, message = "Profile photo updated successfully." });
        }
    }
}