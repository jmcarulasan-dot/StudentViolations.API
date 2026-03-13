using Microsoft.EntityFrameworkCore;
using StudentViolationsAPI.Data;
using StudentViolations.API.IRepository;
using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.Repository
{
    public class GuardRepository : IGuardRepository
    {
        private readonly AppDbContext _context;

        public GuardRepository(AppDbContext context)
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

        public async Task<Student?> GetStudentByQrCode(string qrCode)
        {
            int id;
            if (int.TryParse(qrCode, out id))
            {
                return await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == id);
            }
            return null;
        }

      
        public async Task RecordViolation(Violation violation)
        {
            violation.Status = "Pending";
            _context.Violations.Add(violation);
            await _context.SaveChangesAsync();
        }
    }
}