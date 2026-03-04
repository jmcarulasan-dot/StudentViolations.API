using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.IRepository
{
    public interface IAdminRepository
    {
        Task<List<Violation>> GetViolationsInDateRange(DateTime startDate, DateTime endDate);
        Task<List<Violation>> GetViolationsByStudentId(string studentId);
    }
}