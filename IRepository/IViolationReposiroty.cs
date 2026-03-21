namespace StudentViolationsAPI.IRepository
{
    // Defines all violation-related operations the ViolationClass must implement
    public interface IViolationRepository
    {
        // Saves a new violation record to the database
        Task RecordViolation(dynamic violation);

        // Gets all violations belonging to a specific student using their StudentNo
        Task<List<dynamic>> GetViolationsByStudentId(string studentId);

        // Gets every violation in the database
        Task<List<dynamic>> GetAllViolations();

        // Gets one violation by its ID — returns null if not found
        Task<dynamic?> GetViolationById(int id);

        // Updates the status of a violation — Pending, Approved, or Rejected
        Task UpdateViolationStatus(int id, string status);

        // Permanently deletes a violation from the database
        Task DeleteViolation(int id);
    }
}