using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface ILoginRepository
    {
        Task<ServiceResponse<AuthenticationFlow>> Authenticate(string username, string password);
        Task<ServiceResponse<AuthenticationFlow>> VerifyAuthenticatorCode(string challengeId, string code);
        Task<ServiceResponse<bool>> UserExists(string username, string email);
    }
}
