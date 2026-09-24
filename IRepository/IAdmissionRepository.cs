using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface IAdmissionRepository
    {
        Task<ServiceResponse<List<UserModel>>> GetAllUsers();
        Task<ServiceResponse<UserModel>> GetUserById(int id);
        Task<ServiceResponse<bool>> UpdateUser(UserModel user);
        Task<ServiceResponse<bool>> DeleteUser(int id);
        Task<ServiceResponse<string>> GetStudentQrCode(string studentNo);
    }
}
