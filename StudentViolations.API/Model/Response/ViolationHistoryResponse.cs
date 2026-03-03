namespace StudentViolations.API.Model.Response
{
    public class ViolationHistoryResponse
    {
        public string Status { get; set; }
        public string StudentId { get; set; }
        public List<ViolationDetail> Violations { get; set; }
    }

    public class ViolationDetail
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Details { get; set; }
        public string RecordedBy { get; set; }
    }
}