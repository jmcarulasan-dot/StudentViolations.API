using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.IRepository
{
    public interface IViolationRepository
    {
        Task RecordViolation(Violation violation);
        Task<List<Violation>> GetViolationsByStudentId(string studentId);
    }
}