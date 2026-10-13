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

        private sealed class StoredProcedureResult
        {
            public string? ErrorMessage { get; set; }
        }
        public SAOClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }        public async Task<ServiceResponse<List<SaoUserRecord>>> GetAllUsers()
        {
            var service = new ServiceResponse<List<SaoUserRecord>>();
            using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                var param = new DynamicParameters();
                param.Add("@statementType", "GETALLUSERS");
                var result = await connection.QueryAsync<SaoUserRecord>("SP_SAO", param, commandType: CommandType.StoredProcedure);
                var users = result.ToList();
                var databaseError = users.FirstOrDefault(user => !string.IsNullOrWhiteSpace(user.ErrorMessage));
                if (databaseError != null)
                {
                    service.Status = 500;
                    service.Message = databaseError.ErrorMessage;
                    return service;
                }
                service.Status = 200;
                service.Data = users;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetAllUsers error: {ex.Message}";
            }
            return service;
        }

        public async Task<ServiceResponse<SaoUserRecord>> GetUserById(int studentId)
        {
            var service = new ServiceResponse<SaoUserRecord>();
            using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                var param = new DynamicParameters();
                param.Add("@statementType", "GETUSERBYID");
                param.Add("@StudentID", studentId);
                var result = await connection.QueryFirstOrDefaultAsync<SaoUserRecord>("SP_SAO", param, commandType: CommandType.StoredProcedure);
                if (!string.IsNullOrWhiteSpace(result?.ErrorMessage))
                {
                    service.Status = 500;
                    service.Message = result.ErrorMessage;
                    return service;
                }
                service.Status = result == null ? 404 : 200;
                service.Message = result == null ? "Account not found." : "Account retrieved successfully.";
                service.Data = result;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetUserById error: {ex.Message}";
            }
            return service;
        }

        public async Task<ServiceResponse<bool>> UpdateUser(int studentId, SaoUserUpdateRequest request)
        {
            var service = new ServiceResponse<bool>();
            using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                var param = new DynamicParameters();
                param.Add("@statementType", "UPDATEUSER");
                param.Add("@StudentID", studentId);
                param.Add("@FirstName", request.FirstName?.Trim());
                param.Add("@LastName", request.LastName?.Trim());
                param.Add("@Email", request.Email?.Trim());
                param.Add("@ContactNumber", request.ContactNumber?.Trim());
                param.Add("@Gender", request.Gender?.Trim());
                param.Add("@Address", request.Address?.Trim());
                var procedureResult = await connection.QueryFirstOrDefaultAsync<StoredProcedureResult>("SP_SAO", param, commandType: CommandType.StoredProcedure);
                if (!string.IsNullOrWhiteSpace(procedureResult?.ErrorMessage))
                {
                    service.Status = 500;
                    service.Message = procedureResult.ErrorMessage;
                    service.Data = false;
                    return service;
                }
                service.Status = 200;
                service.Message = "Account updated successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"UpdateUser error: {ex.Message}";
                service.Data = false;
            }
            return service;
        }

        public async Task<ServiceResponse<bool>> DeleteUser(int studentId)
        {
            var service = new ServiceResponse<bool>();
            using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                var param = new DynamicParameters();
                param.Add("@statementType", "DELETEUSER");
                param.Add("@StudentID", studentId);
                var procedureResult = await connection.QueryFirstOrDefaultAsync<StoredProcedureResult>("SP_SAO", param, commandType: CommandType.StoredProcedure);
                if (!string.IsNullOrWhiteSpace(procedureResult?.ErrorMessage))
                {
                    service.Status = 500;
                    service.Message = procedureResult.ErrorMessage;
                    service.Data = false;
                    return service;
                }
                service.Status = 200;
                service.Message = "Account deleted successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"DeleteUser error: {ex.Message}";
                service.Data = false;
            }
            return service;
        }

        public async Task<ServiceResponse<List<PendingDismissalModel>>> GetPendingDismissals()
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