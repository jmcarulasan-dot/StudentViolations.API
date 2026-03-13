using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentViolationsAPI.Model.Entities
{
    [Table("Users")] 
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("StudentID")]
        public int Id { get; set; }

        [Column("Username")]
        public string Username { get; set; }

        [Column("PasswordHash")]
        public string PasswordHash { get; set; }

        [Column("Salt")]
        public string Salt { get; set; }

        [Column("Email")]
        public string Email { get; set; }

        [Column("FirstName")]
        public string FirstName { get; set; }

        [Column("LastName")]
        public string LastName { get; set; }

        [Column("DateOfBirth")]
        public DateTime? DateOfBirth { get; set; } 

        [Column("Gender")]
        public string Gender { get; set; }

        [Column("Address")]
        public string Address { get; set; }

        [Column("ContactNumber")]
        public string ContactNumber { get; set; }

        [Column("RegistrationDate")]
        public DateTime? RegistrationDate { get; set; }

        [Column("Role")]
        public string Role { get; set; }

        [Column("Course")]
        public string? Course { get; set; }

        [Column("Year")]
        public string? Year { get; set; }

        [NotMapped]
        public string? Number { get; set; }
    }
}