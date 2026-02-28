using System.ComponentModel.DataAnnotations;

namespace StudentViolationsAPI.Model.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Number { get; set; }
        public string Salt { get; set; }
    }
}