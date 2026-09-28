using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;

namespace StudentViolations.API.Class
{
    public sealed class AuditTrailClass : IAuditTrailRepository
    {
        private readonly string _connectionString;

        public AuditTrailClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb")
                ?? throw new InvalidOperationException("StudentViolationsdb connection string is missing.");
        }

        public async Task<ServiceResponse<bool>> RecordAsync(AuditTrailEntry entry)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.ExecuteAsync(@"
INSERT INTO dbo.AuditTrail
    (Action, EntityType, EntityID, StudentNo, PreviousValue, NewValue, Remarks, ActorUsername, ActorRole, CreatedAtUtc)
VALUES
    (@Action, @EntityType, @EntityID, @StudentNo, @PreviousValue, @NewValue, @Remarks, @ActorUsername, @ActorRole, SYSUTCDATETIME());", entry);
                response.Status = 200;
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Status = 500;
                response.Message = $"Audit record failed: {ex.Message}";
            }
            return response;
        }

        public async Task<ServiceResponse<List<AuditTrailEntry>>> GetRecentAsync(int take, string? studentNo = null)
        {
            var response = new ServiceResponse<List<AuditTrailEntry>>();
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                var rows = await connection.QueryAsync<AuditTrailEntry>(@"
SELECT TOP (@Take) AuditID, Action, EntityType, EntityID, StudentNo, PreviousValue, NewValue,
       Remarks, ActorUsername, ActorRole, CreatedAtUtc
FROM dbo.AuditTrail
WHERE (@StudentNo IS NULL OR StudentNo = @StudentNo)
ORDER BY AuditID DESC;", new { Take = take, StudentNo = studentNo });
                response.Status = 200;
                response.Data = rows.ToList();
            }
            catch (Exception ex)
            {
                response.Status = 500;
                response.Message = $"Audit history could not be loaded: {ex.Message}";
            }
            return response;
        }
    }
}
