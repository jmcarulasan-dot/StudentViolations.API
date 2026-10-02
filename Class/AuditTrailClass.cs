using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using System.Data;

namespace StudentViolations.API.Class
{
    public class AuditTrailClass : IAuditTrailRepository
    {
        private readonly string _connectionString;

        public AuditTrailClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        public async Task<ServiceResponse<bool>> RecordAsync(AuditTrailEntry entry)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "INSERT");
                param.Add("@Action", entry.Action);
                param.Add("@EntityType", entry.EntityType);
                param.Add("@EntityID", entry.EntityID);
                param.Add("@StudentNo", entry.StudentNo);
                param.Add("@PreviousValue", entry.PreviousValue);
                param.Add("@NewValue", entry.NewValue);
                param.Add("@Remarks", entry.Remarks);
                param.Add("@ActorUsername", entry.ActorUsername);
                param.Add("@ActorRole", entry.ActorRole);
                await connection.ExecuteAsync(
                    "SP_AUDIT_TRAIL", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Audit record saved.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"RecordAsync error: {ex.Message}";
                service.Data = false;
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        public async Task<ServiceResponse<List<AuditTrailEntry>>> GetRecentAsync(int take, string? studentNo = null)
        {
            var service = new ServiceResponse<List<AuditTrailEntry>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETRECENT");
                param.Add("@Take", take);
                param.Add("@StudentNo", studentNo);
                var result = await connection.QueryAsync<AuditTrailEntry>(
                    "SP_AUDIT_TRAIL", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Audit history retrieved.";
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetRecentAsync error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }
    }
}
