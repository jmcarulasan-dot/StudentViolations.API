using StudentViolations.API.Model;

namespace StudentViolations.API.IRepository
{
    public interface ILoginRepository
    {
        Task<ServiceResponse<object>> GetLogin(string username, string password);
        object GetLogin(object username, object password);
    }
}
