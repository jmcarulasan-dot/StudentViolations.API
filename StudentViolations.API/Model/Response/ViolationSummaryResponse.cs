namespace StudentViolations.API.Model.Response
{
    public class ViolationSummaryResponse
    {
        public string Status { get; set; }
        public int TotalViolations { get; set; }
        public string TopViolation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}