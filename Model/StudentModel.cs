namespace StudentViolations.API.Model
{
    public class StudentModel
    {
        public int StudentID { get; set; }
        public string StudentNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string ContactNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Address { get; set; }
        public string Course { get; set; }
        public string Year { get; set; }
        public string? QRCode { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public int ViolationCount { get; set; }
        public string WarningLevel { get; set; }
        public string Status { get; set; }
    }
}