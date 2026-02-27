using Microsoft.EntityFrameworkCore;
using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Violation> Violations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships, constraints, etc.
            base.OnModelCreating(modelBuilder);
        }
    }
}