using System.ComponentModel.DataAnnotations;

namespace StudentViolationsAPI.Model.Entities
{
    public class Student
    {
        [Key]
        public int Id { get; set; } 
        public string QrCode { get; set; }
        public string Name { get; set; }
        public int ViolationCount { get; set; }
    }
}