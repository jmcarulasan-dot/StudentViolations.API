using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface IAdmissionRepository
    {
        Task<ServiceResponse<UserModel>> RegisterStudent(UserModel user);
    }
}