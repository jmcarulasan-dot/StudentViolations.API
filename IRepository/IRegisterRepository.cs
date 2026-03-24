using StudentViolations.API.Model;

namespace StudentViolations.API.IRepository
{
    // Defines the registration operation the RegisterClass must implement
    public interface IRegisterRepository
    {
        Task<ServiceResponse<object>> RegisterUser(User user);
    }
}