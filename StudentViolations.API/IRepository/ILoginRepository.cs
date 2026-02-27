using StudentViolations.API.Controllers;
using StudentViolations.API.Model;
using System.Threading.Tasks;

namespace StudentViolations.API.IRepository
{
    public interface ILoginRepository
    {
        Task<ServiceResponse<object>> GetLogin(string username, string password);
        Task<bool> UserExists(string username, string email);
        Task<ServiceResponse<object>> RegisterUser(User user);
    }
}