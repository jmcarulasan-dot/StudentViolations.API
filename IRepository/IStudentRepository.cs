using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface IStudentRepository
    {
        Task<ServiceResponse<StudentModel>> GetStudentByStudentId(string studentNo);
        Task<ServiceResponse<bool>> UpdateStudent(StudentModel student);
        Task<ServiceResponse<List<StudentModel>>> GetAllStudents();
        Task<ServiceResponse<bool>> UpdateStudentStatus(int studentId, string status);
        Task<ServiceResponse<string>> GetUsernameByStudentNo(string studentNo);
    }
}