namespace StudentViolationsAPI.Model.Requests
{
    public class RecordViolationRequest
    {
        public string StudentId { get; set; }
        public string GuardId { get; set; }
        public string ViolationType { get; set; }
        public string Details { get; set; }
    }
}