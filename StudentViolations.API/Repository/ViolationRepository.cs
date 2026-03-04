using Microsoft.EntityFrameworkCore;
using StudentViolationsAPI.Data;
using StudentViolationsAPI.IRepository;
using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.Repository
{
    public class ViolationRepository : IViolationRepository
    {
        private readonly AppDbContext _context;

        public ViolationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task RecordViolation(Violation violation)
        {
            _context.Violations.Add(violation);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Violation>> GetViolationsByStudentId(string studentId)
        {
            int id = int.Parse(studentId);
            return await _context.Violations
                .Where(v => v.StudentId == id)
                .ToListAsync();
        }
    }
}