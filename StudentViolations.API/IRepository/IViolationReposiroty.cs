using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.IRepository
{
    public interface IViolationRepository
    {
        Task RecordViolation(Violation violation);
        Task<List<Violation>> GetViolationsByStudentId(string studentId);
        Task<List<Violation>> GetAllViolations();
        Task<Violation?> GetViolationById(int id);
        Task UpdateViolationStatus(int id, string status);
        Task DeleteViolation(int id);
    }
}