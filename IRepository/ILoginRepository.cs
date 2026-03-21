using StudentViolations.API.Model;

namespace StudentViolations.API.IRepository
{
    // Defines all login and user existence check operations the LoginClass must implement
    public interface ILoginRepository
    {
        // Verifies username and password — returns a ServiceResponse with user data if successful
        Task<ServiceResponse<object>> GetLogin(string username, string password);

        // Checks if a username or email is already registered — used during registration
        Task<bool> UserExists(string username, string email);

        // Saves a new user to the Users table
        Task<ServiceResponse<object>> RegisterUser(User user);
    }
}