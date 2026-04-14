namespace StudentViolations.API.Model
{
    public class RecordViolationModel
    {
        public string StudentNo { get; set; }
        public string ViolationType { get; set; }
        public string Details { get; set; }
        public string Severity { get; set; }
    }
    public class GetSummaryModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}