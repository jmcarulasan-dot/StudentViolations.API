using StudentViolationsAPI.Model.Entities;

namespace StudentViolations.API.IRepository
{
    public interface IRegisterRepository
    {
        Task<ServiceResponse<object>> RegisterUser(User user);
    }
}
