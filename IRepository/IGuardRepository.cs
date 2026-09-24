using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface IGuardRepository
    {
        Task<ServiceResponse<StudentModel>> GetStudentByQrCode(string qrCode);
        Task<ServiceResponse<bool>> RecordViolation(ViolationModel violation);
        Task<ServiceResponse<string>> GetUsernameByStudentNo(string studentNo);
    }
}