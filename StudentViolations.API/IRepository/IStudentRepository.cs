using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.IRepository
{
    public interface IStudentRepository
    {
        Task<Student> GetStudentByStudentId(string studentId);
        Task UpdateStudent(Student student);
    }
}