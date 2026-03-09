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
                .OrderByDescending(v => v.Date)
                .ToListAsync();
        }

        public async Task<List<Violation>> GetAllViolations()
        {
            return await _context.Violations
                .OrderByDescending(v => v.Date)
                .ToListAsync();
        }

        public async Task<Violation?> GetViolationById(int id)
        {
            return await _context.Violations.FindAsync(id);
        }

        public async Task UpdateViolationStatus(int id, string status)
        {
            var violation = await _context.Violations.FindAsync(id);
            if (violation != null)
            {
                violation.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteViolation(int id)
        {
            var violation = await _context.Violations.FindAsync(id);
            if (violation != null)
            {
                _context.Violations.Remove(violation);
                await _context.SaveChangesAsync();
            }
        }
    }
}