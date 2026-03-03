using System;

namespace StudentViolationsAPI.Model.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }

        public string Address { get; set; }
        public string ContactNumber { get; set; }

        public string Email { get; set; }

        public string Number { get; set; }
        public DateTime RegistrationDate { get; set; }

        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
    }
}