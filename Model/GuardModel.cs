namespace StudentViolations.API.Model
{
    // Model for the GET violations/summary endpoint — defines the date range to search
    // Request model for POST /api/guard/student/violation
    public class RecordViolationModel
    {
        public string StudentNo { get; set; }
        public string ViolationType { get; set; }
        public string Details { get; set; }
        public string Severity { get; set; }
        public string GuardId { get; set; }
    }

    // Request model for GET /api/guard/violations/summary
    public class GetSummaryModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}