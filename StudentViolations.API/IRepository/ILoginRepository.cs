using StudentViolations.API.Model;

namespace StudentViolations.API.IRepository
{
    public interface ILoginRepository
    {
        Task<ServiceResponse<object>> GetLogin(string username, string password);
        public object GetLogin(object username, object password)
        {
            throw new NotImplementedException();
        }
    }
}
