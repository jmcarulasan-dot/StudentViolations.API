namespace StudentViolations.API.Model
{
    // Model for the PUT api/sao/users/{id} endpoint — all fields are optional
    // Only the fields provided will be updated, the rest keep their existing values
    // Note: Role is intentionally excluded — user roles cannot be changed after registration
    public class UpdateUserModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }

        // Only applies to students
        public string? Course { get; set; }

        // Only applies to students
        public string? Year { get; set; }
    }
}