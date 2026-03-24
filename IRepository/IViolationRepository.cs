namespace StudentViolationsAPI.IRepository
{
    // Defines all violation-related operations the ViolationClass must implement
    public interface IViolationRepository
    {
        Task RecordViolation(dynamic violation);
        Task<List<dynamic>> GetViolationsByStudentId(string studentId);
        Task<List<dynamic>> GetAllViolations();
        Task<dynamic?> GetViolationById(int id);
        Task UpdateViolationStatus(int id, string status);
        Task DeleteViolation(int id);
    }
}