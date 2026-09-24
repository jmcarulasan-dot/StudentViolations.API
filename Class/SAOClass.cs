using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using System.Data;

namespace StudentViolations.API.Class
{
    public class SAOClass : ISAORepository
    {
        private readonly string _connectionString;
        public SAOClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }        public async Task<ServiceResponse<List<PendingDismissalModel>>> GetPendingDismissals()
        {
            var service = new ServiceResponse<List<PendingDismissalModel>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETPENDINGDISMISSAL");
                var result = await connection.QueryAsync<PendingDismissalModel>("SP_SAO", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetPendingDismissals error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }

        public async Task<ServiceResponse<List<PendingDismissalModel>>> GetDismissedStudents()
        {
            var service = new ServiceResponse<List<PendingDismissalModel>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETDISMISSED");
                var result = await connection.QueryAsync<PendingDismissalModel>("SP_SAO", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetDismissedStudents error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
    }
}