namespace StudentViolationsAPI.Model.Requests
{
    public class GetSummaryRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}