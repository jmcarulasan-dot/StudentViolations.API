using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentViolations.API.IRepository;
using System.Security.Claims;

namespace StudentViolations.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "Notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        // GET /api/notifications
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role)?.ToLower();

            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { status = 401, message = "User not found in token." });

            var notifications = await _notificationRepository.GetByUserAndRole(username, role);

            return Ok(new
            {
                status = 200,
                message = "Notifications retrieved successfully.",
                total = notifications.Count,
                unread = notifications.Count(n => !n.IsRead),
                data = notifications
            });
        }

        // GET /api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role)?.ToLower();

            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { status = 401, message = "User not found in token." });

            var count = await _notificationRepository.GetUnreadCount(username, role);

            return Ok(new
            {
                status = 200,
                unread_count = count
            });
        }

        // PUT /api/notifications/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationRepository.MarkAsRead(id);
            return Ok(new { status = 200, message = "Notification marked as read." });
        }

        // PUT /api/notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role)?.ToLower();

            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { status = 401, message = "User not found in token." });

            await _notificationRepository.MarkAllAsRead(username, role);
            return Ok(new { status = 200, message = "All notifications marked as read." });
        }

        [HttpPost("fcm-token")]
        public async Task<IActionResult> SaveFCMToken([FromBody] FCMRequest request)
        {
            var username = User.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { status = 401, message = "User not found." });

            await _notificationRepository.SaveFCMToken(username, request.FCMToken);
            return Ok(new { status = 200, message = "Token saved." });
        }

        public class FCMRequest
        {
            public string FCMToken { get; set; }
        }
    }
}