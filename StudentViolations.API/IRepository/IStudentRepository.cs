using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.IRepository
{
    public interface IStudentRepository
    {
        Task<Student> GetStudentByQrCode(string qrCode);
        Task UpdateStudent(Student student);
    }
}