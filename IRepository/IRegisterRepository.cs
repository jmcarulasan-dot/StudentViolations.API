using StudentViolations.API.Model;

namespace StudentViolations.API.IRepository
{
    // Defines the registration operation the RegisterClass must implement
    public interface IRegisterRepository
    {
        // Saves a new user to the database — students also get a StudentNo and QR code saved
        Task<ServiceResponse<object>> RegisterUser(User user);
    }
}