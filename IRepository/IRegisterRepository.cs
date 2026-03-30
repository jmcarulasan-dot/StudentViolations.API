using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface IRegisterRepository
    {
        Task<ServiceResponse<UserModel>> RegisterUser(UserModel user);
    }
}