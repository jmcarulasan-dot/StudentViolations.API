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
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users"); // ✅ changed
            modelBuilder.Entity<Student>().ToTable("Students");
            modelBuilder.Entity<Violation>().ToTable("Violations");
            base.OnModelCreating(modelBuilder);
        }
    }
}