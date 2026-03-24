namespace StudentViolationsAPI.IRepository
{
    // Defines all student data operations the StudentClass must implement
    public interface IStudentRepository
    {
        Task<dynamic?> GetStudentByStudentId(string studentId);
        Task UpdateStudent(dynamic student);
        Task<List<dynamic>> GetAllStudents();
    }
}