namespace StudentViolations.API.IRepository
{
    // Defines all student data operations the StudentClass must implement
    public interface IStudentRepository
    {
        // Gets one student's data using their StudentNo — returns null if not found
        Task<dynamic?> GetStudentByStudentId(string studentId);

        // Updates an existing student's information
        Task UpdateStudent(dynamic student);

        // Gets all students from the database
        Task<List<dynamic>> GetAllStudents();
    }
}