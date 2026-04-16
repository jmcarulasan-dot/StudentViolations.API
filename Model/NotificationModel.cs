namespace StudentViolations.API.Model
{
    public class NotificationModel
    {
        public int Id { get; set; }
        public string? TargetUsername { get; set; }
        public string? TargetRole { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class FCMRequest
    {
        public string FCMToken { get; set; }
    }
}