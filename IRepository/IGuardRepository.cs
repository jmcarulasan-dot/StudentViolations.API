namespace StudentViolations.API.IRepository
{
    // Defines all guard-related operations the GuardClass must implement
    public interface IGuardRepository
    {
        // Gets all violations that were recorded between two dates
        Task<List<dynamic>> GetViolationsInDateRange(DateTime startDate, DateTime endDate);

        // Gets all violations belonging to a specific student using their StudentNo
        Task<List<dynamic>> GetViolationsByStudentId(string studentId);

        // Finds a student by scanning their QR code (StudentNo) — returns null if not found
        Task<dynamic?> GetStudentByQrCode(string qrCode);

        // Saves a new violation record to the database
        Task RecordViolation(dynamic violation);

        // Gets all registered students
        Task<List<dynamic>> GetAllStudents();

        // Gets a specific student by their StudentNo
        Task<dynamic?> GetStudentByStudentNo(string studentNo);
    }
}