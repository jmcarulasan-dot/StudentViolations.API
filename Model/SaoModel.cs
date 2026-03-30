namespace StudentViolations.API.Model
{
    // Model for the PUT api/sao/users/{id} endpoint — all fields are optional
    public class UpdateUserModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Course { get; set; }
        public string? Year { get; set; }
    }
}