using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using System.Data;

namespace StudentViolations.API.Class
{
    public class ViolationClass : IViolationRepository
    {
        private readonly string _connectionString;
        public ViolationClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }
        public async Task<ServiceResponse<bool>> RecordViolation(ViolationModel violation)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "RECORDVIOLATION");
                param.Add("@StudentId", violation.StudentId);
                param.Add("@ViolationName", violation.ViolationName);
                param.Add("@Description", violation.Description);
                param.Add("@Severity", violation.Severity);
                param.Add("@GuardId", violation.GuardId);
                await connection.ExecuteAsync("SP_VIOLATION", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Violation recorded successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"RecordViolation error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
        public async Task<ServiceResponse<List<ViolationModel>>> GetViolationsByStudentId(string studentNo)
        {
            var service = new ServiceResponse<List<ViolationModel>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETBYSTUDENT");
                param.Add("@StudentNo", studentNo);
                var result = await connection.QueryAsync<ViolationModel>("SP_VIOLATION", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetViolationsByStudentId error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
        public async Task<ServiceResponse<List<ViolationModel>>> GetAllViolations()
        {
            var service = new ServiceResponse<List<ViolationModel>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETALL");
                var result = await connection.QueryAsync<ViolationModel>("SP_VIOLATION", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetAllViolations error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
        public async Task<ServiceResponse<ViolationModel>> GetViolationById(int id)
        {
            var service = new ServiceResponse<ViolationModel>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETBYID");
                param.Add("@ViolationID", id);
                var result = await connection.QueryFirstOrDefaultAsync<ViolationModel>("SP_VIOLATION", param, commandType: CommandType.StoredProcedure);
                if (result == null)
                {
                    service.Status = 404;
                    service.Message = "Violation not found.";
                    return service;
                }
                service.Status = 200;
                service.Data = result;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetViolationById error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
        public async Task<ServiceResponse<bool>> UpdateViolationStatus(int id, string status)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "UPDATESTATUS");
                param.Add("@ViolationID", id);
                param.Add("@Status", status);
                await connection.ExecuteAsync("SP_VIOLATION", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = $"Violation {status.ToLower()} successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"UpdateViolationStatus error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
        public async Task<ServiceResponse<bool>> DeleteViolation(int id)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "DELETE");
                param.Add("@ViolationID", id);
                await connection.ExecuteAsync("SP_VIOLATION", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Violation deleted successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"DeleteViolation error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
    }
}