using StudentViolations.API.Model;

namespace StudentViolations.API.IRepository
{
    public interface INotificationRepository
    {
        Task<List<NotificationModel>> GetByUserAndRole(string username, string role);
        Task<int> GetUnreadCount(string username, string role);
        Task SendToUser(string targetUsername, string title, string message);
        Task SendToRole(string targetRole, string title, string message);
        Task MarkAsRead(int id);
        Task MarkAllAsRead(string username, string role);
    }
}