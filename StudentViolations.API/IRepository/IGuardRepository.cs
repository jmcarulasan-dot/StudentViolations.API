using StudentViolationsAPI.Model.Entities;

namespace StudentViolations.API.IRepository
{
    public interface IGuardRepository
    {
        Task<List<Violation>> GetViolationsInDateRange(DateTime startDate, DateTime endDate);
        Task<List<Violation>> GetViolationsByStudentId(string studentId);

        // ✅ Added for validate and record violation
        Task<Student?> GetStudentByQrCode(string qrCode);
        Task RecordViolation(Violation violation);
    }
}