using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface IAdmissionRepository
    {
        Task<ServiceResponse<EnrollmentModel>> CreateEnrollment(EnrollmentModel enrollment);
        Task<ServiceResponse<List<EnrollmentModel>>> GetEnrollments();
        Task<ServiceResponse<List<RegistrationRequestModel>>> GetRegistrationRequests();
        Task<ServiceResponse<UserModel>> ApproveRegistration(int requestId);
        Task<ServiceResponse<bool>> RejectRegistration(int requestId);
        Task<ServiceResponse<bool>> SubmitRegistrationRequest(StudentRegistrationRequestModel request);
        Task<ServiceResponse<bool>> SignClearance(string studentNo);
        Task<ServiceResponse<bool>> CanSignClearance(string studentNo);
    }
}