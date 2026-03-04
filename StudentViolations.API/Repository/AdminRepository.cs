using Microsoft.EntityFrameworkCore;
using StudentViolationsAPI.Data;
using StudentViolationsAPI.IRepository;
using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Violation>> GetViolationsInDateRange(DateTime startDate, DateTime endDate)
        {
            return await _context.Violations
                .Where(v => v.Date >= startDate && v.Date <= endDate)
                .ToListAsync();
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