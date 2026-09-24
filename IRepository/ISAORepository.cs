using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface ISAORepository
    {
        Task<ServiceResponse<List<PendingDismissalModel>>> GetPendingDismissals();
        Task<ServiceResponse<List<PendingDismissalModel>>> GetDismissedStudents();
    }
}