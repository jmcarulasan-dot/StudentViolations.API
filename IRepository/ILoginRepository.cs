using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface ILoginRepository
    {
        Task<ServiceResponse<UserModel>> Authenticate(string username, string password);

        Task<ServiceResponse<bool>> UserExists(string username, string email);
    }
}