namespace StudentViolations.API.Model
{
    // Model for the POST /register endpoint — contains all fields needed to create a new user
    public class RegistrationModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Number { get; set; }
        public string Role { get; set; }

        // Only required if role is student
        public string? Course { get; set; }
        public string? Year { get; set; }
        public string? StudentNo { get; set; }
    }
}