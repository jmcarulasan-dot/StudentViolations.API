namespace StudentViolationsAPI.Model.Requests
{
    public class RecordViolationRequest
    {
        public int StudentId { get; set; }
        public string GuardId { get; set; }
        public string ViolationType { get; set; }
        public string Details { get; set; }
        public string Severity { get; set; }
    }
}