namespace StudentViolationsAPI.Model.Requests
{
    public class ValidateStudentRequest
    {
        public string QrCode { get; set; }
        public string GuardId { get; set; }
    }
}