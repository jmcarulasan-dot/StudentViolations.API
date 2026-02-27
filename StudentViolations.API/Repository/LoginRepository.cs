using StudentViolations.API.Controllers;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Threading.Tasks;

namespace StudentViolationsAPI.Repository
{
    public class LoginRepository : ILoginRepository
    {
        // Implement the methods from ILoginRepository
        public async Task<ServiceResponse<object>> GetLogin(string username, string password)
        {
            // Your implementation here (e.g., query the database)
            // Replace with your actual data access code
            var loginModel = new LoginModel { Username = username, Password = password }; // Replace with actual data

            // Wrap the result in a ServiceResponse
            return new ServiceResponse<object> { Data = loginModel, Message = "Login successful" };
        }

        public async Task<bool> UserExists(string username, string email)
        {
            // Your implementation here (e.g., check if the user exists in the database)
            return true; // Replace with actual data
        }

        public async Task<ServiceResponse<object>> RegisterUser(User user)
        {
            // Your implementation here (e.g., add the user to the database)

            // Wrap the result in a ServiceResponse
            return new ServiceResponse<object> { Data = user, Message = "Registration successful" };
        }
    }
}