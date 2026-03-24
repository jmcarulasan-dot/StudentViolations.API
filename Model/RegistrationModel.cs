using System.ComponentModel.DataAnnotations;

namespace StudentViolations.API.Model
{
    // Model for the POST /register endpoint — contains all fields needed to create a new user
    public class RegistrationModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(2, ErrorMessage = "Username must be at least 2 characters.")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s\-]+$", ErrorMessage = "First name must contain letters only.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MinLength(2, ErrorMessage = "Last name must be at least 2 characters.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s\-]+$", ErrorMessage = "Last name must contain letters only.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        public string DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [RegularExpression(@"^(male|female|Male|Female|MALE|FEMALE)$",
            ErrorMessage = "Gender must be either 'male' or 'female'.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [MinLength(5, ErrorMessage = "Address must be at least 5 characters.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^09\d{9}$",
            ErrorMessage = "Contact number must be 11 digits and start with 09 (e.g. 09123456789).")]
        public string Number { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [RegularExpression(@"^(guard|student|guidance|sao|Guard|Student|Guidance|Sao|SAO|GUARD|STUDENT|GUIDANCE)$",
            ErrorMessage = "Role must be one of: guard, student, guidance, sao.")]
        public string Role { get; set; }

        // Only required if the role is student
        public string? Course { get; set; }
        public string? Year { get; set; }

        [RegularExpression(@"^[A-Za-z0-9]{3}-\d{2}-\d{4}-[A-Za-z0-9]{6}$",
            ErrorMessage = "Student number format must be like C26-01-0001-MAN121.")]
        public string? StudentNo { get; set; }
    }
}