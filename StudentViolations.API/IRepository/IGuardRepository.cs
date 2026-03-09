using StudentViolationsAPI.Model.Entities;

namespace StudentViolations.API.IRepository
{
    public interface IGuardRepository
    {
        Task<List<Violation>> GetViolationsInDateRange(DateTime startDate, DateTime endDate);
        Task<List<Violation>> GetViolationsByStudentId(string studentId);
    }
}