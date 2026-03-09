using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentViolationsAPI.Model.Entities
{
    [Table("Violations")]
    public class Violation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ViolationID")]
        public int Id { get; set; }

        [Column("StudentId")]
        public int StudentId { get; set; }

        [Column("ViolationName")]
        public string Type { get; set; }

        [Column("Description")]
        public string Details { get; set; }

        [Column("Severity")]
        public string Severity { get; set; }

        [Column("ViolationDate")]
        public DateTime Date { get; set; }

        [Column("GuardId")]
        public string GuardId { get; set; }

        [Column("Status")]
        public string Status { get; set; } = "Pending";
    }
}