using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.IRepository
{
    public interface IAuditTrailRepository
    {
        Task<ServiceResponse<bool>> RecordAsync(AuditTrailEntry entry);
        Task<ServiceResponse<List<AuditTrailEntry>>> GetRecentAsync(int take, string? studentNo = null);
    }
}
