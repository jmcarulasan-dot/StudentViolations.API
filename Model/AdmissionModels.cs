namespace StudentViolations.API.Model
{
    public class EnrollmentModel
    {
        public string StudentNo { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Course { get; set; }
        public string Year { get; set; }
    }

    public class StudentRegistrationRequestModel
    {
        public string StudentNo { get; set; }
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
    }
}
