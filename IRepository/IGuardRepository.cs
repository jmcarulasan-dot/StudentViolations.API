namespace StudentViolations.API.IRepository
{
    // Defines all guard-related operations the GuardClass must implement
    public interface IGuardRepository
    {
        Task<List<dynamic>> GetViolationsInDateRange(DateTime startDate, DateTime endDate);
        Task<List<dynamic>> GetViolationsByStudentId(string studentId);
        Task<dynamic?> GetStudentByQrCode(string qrCode);
        Task RecordViolation(dynamic violation);
        Task<List<dynamic>> GetAllStudents();
        Task<dynamic?> GetStudentByStudentNo(string studentNo);
    }
}