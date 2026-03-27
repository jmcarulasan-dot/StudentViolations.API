namespace StudentViolations.API.Model
{
    // Model for the GET violations/summary endpoint — defines the date range to search
    public class GetSummaryModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // Model for the POST student/violation endpoint
    // GuardId is NOT included here — it is read automatically from the JWT token
    public class RecordViolationModel
    {
        public string StudentNo { get; set; }
        public string ViolationType { get; set; }
        public string Details { get; set; }
        public string Severity { get; set; }
    }
}