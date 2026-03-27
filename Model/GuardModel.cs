namespace StudentViolations.API.Model
{
    // Model for the GET violations/summary endpoint — defines the date range to search
    public class GetSummaryModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // Model for the POST student/violation endpoint
    public class RecordViolationModel
    {
        public string StudentNo { get; set; }
        public string ViolationType { get; set; }
        public string Details { get; set; }
        public string Severity { get; set; }
    }
}