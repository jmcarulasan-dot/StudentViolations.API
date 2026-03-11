namespace StudentViolationsAPI.Model.Requests
{
    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Course { get; set; }
        public string? Year { get; set; }
        public string? Role { get; set; }
    }
}