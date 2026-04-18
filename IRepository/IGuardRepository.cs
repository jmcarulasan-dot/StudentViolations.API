using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface IGuardRepository
    {
        Task<ServiceResponse<List<ViolationModel>>> GetViolationsInDateRange(DateTime startDate, DateTime endDate);
        Task<ServiceResponse<List<ViolationModel>>> GetViolationsByStudentId(string studentNo);
        Task<ServiceResponse<StudentModel>> GetStudentByQrCode(string qrCode);
        Task<ServiceResponse<bool>> RecordViolation(ViolationModel violation);
        Task<ServiceResponse<List<StudentModel>>> GetAllStudents();
        Task<ServiceResponse<StudentModel>> GetStudentByStudentNo(string studentNo);
        Task<ServiceResponse<string>> GetUsernameByStudentNo(string studentNo);
    }
}