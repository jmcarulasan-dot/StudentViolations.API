using StudentViolations.API.Model;

namespace StudentViolations.API.IRepository
{
    // Defines all login and user existence check operations the LoginClass must implement
    public interface ILoginRepository
    {
        Task<ServiceResponse<object>> GetLogin(string username, string password);
        Task<bool> UserExists(string username, string email);
    }
}