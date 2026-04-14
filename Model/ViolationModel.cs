namespace StudentViolations.API.Model
{
    public class ViolationModel
    {
        public int ViolationID { get; set; }
        public int StudentId { get; set; }
        public string StudentNo { get; set; }
        public string StudentName { get; set; }
        public string ViolationName { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public string GuardId { get; set; }
        public string GuardName { get; set; }
        public DateTime ViolationDate { get; set; }
        public string Status { get; set; }
        public string? AppealText { get; set; }
        public string AppealStatus { get; set; }
        public string? AppealRemarks { get; set; }
    }
    public class AppealReviewModel
    {
        public string AppealStatus { get; set; }
        public string? AppealRemarks { get; set; }
    }
    public class SubmitAppealModel
    {
        public string AppealText { get; set; }
    }
}