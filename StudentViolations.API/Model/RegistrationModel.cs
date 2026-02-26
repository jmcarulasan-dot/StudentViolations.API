using System.ComponentModel.DataAnnotations;

namespace StudentViolations.API.Model
{
    public class RegistrationModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        public string Gender { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string Number { get; set; }
    }
}