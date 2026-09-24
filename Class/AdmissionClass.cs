using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.Class
{
    public class AdmissionClass : IAdmissionRepository
    {
        private readonly IRegisterRepository _registerRepository;

        public AdmissionClass(IRegisterRepository registerRepository)
        {
            _registerRepository = registerRepository;
        }

        public async Task<ServiceResponse<UserModel>> RegisterStudent(UserModel user)
        {
            if (!string.Equals(user.Role, "Student", StringComparison.OrdinalIgnoreCase))
            {
                return new ServiceResponse<UserModel>
                {
                    Status = 400,
                    Message = "Admission can only register Student accounts."
                };
            }

            return await _registerRepository.RegisterUser(user);
        }
    }
}