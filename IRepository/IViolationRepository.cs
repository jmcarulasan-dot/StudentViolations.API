using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface IViolationRepository
    {
        Task<ServiceResponse<bool>> RecordViolation(ViolationModel violation);
        Task<ServiceResponse<List<ViolationModel>>> GetViolationsByStudentId(string studentNo);
        Task<ServiceResponse<List<ViolationModel>>> GetAllViolations();
        Task<ServiceResponse<ViolationModel>> GetViolationById(int id);
        Task<ServiceResponse<bool>> UpdateViolationStatus(int id, string status);
        Task<ServiceResponse<bool>> DeleteViolation(int id);
        Task<ServiceResponse<bool>> SubmitAppeal(int violationId, string appealText);
        Task<ServiceResponse<bool>> UpdateAppealStatus(int violationId, string appealStatus);
    }
}