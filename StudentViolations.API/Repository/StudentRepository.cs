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

        public async Task<Student> GetStudentByQrCode(string qrCode)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.QrCode == qrCode);
        }

        public async Task UpdateStudent(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }
    }
}