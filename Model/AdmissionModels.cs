namespace StudentViolations.API.Model
{
    public class EnrollmentModel
    {
        public int EnrollmentId { get; set; }
        public string StudentNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string Course { get; set; }
        public string Year { get; set; }
        public string EnrollmentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StudentRegistrationRequestModel
    {
        public string StudentNo { get; set; }
        public string DateOfBirth { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegistrationRequestModel
    {
        public int RequestId { get; set; }
        public string StudentNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string RequestStatus { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
