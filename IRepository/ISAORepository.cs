using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface ISAORepository
    {
        Task<ServiceResponse<List<UserModel>>> GetAllUsers();
        Task<ServiceResponse<UserModel>> GetUserById(int id);
        Task<ServiceResponse<bool>> UpdateUser(UserModel user);
        Task<ServiceResponse<bool>> DeleteUser(int id);
        Task<ServiceResponse<List<PendingDismissalModel>>> GetPendingDismissals();
        Task<ServiceResponse<List<PendingDismissalModel>>> GetDismissedStudents();
    }
}