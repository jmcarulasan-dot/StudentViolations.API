using Microsoft.EntityFrameworkCore;
using StudentViolations.API.Controllers;
using StudentViolations.API.Model;
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
        public DbSet<User> Users { get; set; } 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
