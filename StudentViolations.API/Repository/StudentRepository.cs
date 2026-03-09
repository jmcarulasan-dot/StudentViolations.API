using Microsoft.EntityFrameworkCore;
using StudentViolationsAPI.IRepository;
using StudentViolationsAPI.Model.Entities;
using StudentViolationsAPI.Data;

namespace StudentViolationsAPI.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Student> GetStudentByStudentId(string studentId)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.Id.ToString() == studentId);
        }

        public async Task UpdateStudent(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Student>> GetAllStudents()
        {
            return await _context.Students.ToListAsync();
        }
    }
}