namespace StudentViolationsAPI.Model.Entities
{
    public class Violation
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 15);
        public string StudentId { get; set; }
        public string Type { get; set; }
        public string Details { get; set; }
        public DateTime Date { get; set; }
        public string GuardId { get; set; }
    }
}