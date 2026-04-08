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
        }
        public async Task<ServiceResponse<List<UserModel>>> GetAllUsers()
        {
            var service = new ServiceResponse<List<UserModel>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETALLUSERS");
                var result = await connection.QueryAsync<UserModel>("SP_SAO", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetAllUsers error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
        public async Task<ServiceResponse<UserModel>> GetUserById(int id)
        {
            var service = new ServiceResponse<UserModel>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETUSERBYID");
                param.Add("@StudentID", id);
                var result = await connection.QueryFirstOrDefaultAsync<UserModel>("SP_SAO", param, commandType: CommandType.StoredProcedure);
                if (result == null)
                {
                    service.Status = 404;
                    service.Message = "User not found.";
                    return service;
                }
                service.Status = 200;
                service.Data = result;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetUserById error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
        public async Task<ServiceResponse<bool>> UpdateUser(UserModel user)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "UPDATEUSER");
                param.Add("@StudentID", user.StudentID);
                param.Add("@FirstName", user.FirstName);
                param.Add("@LastName", user.LastName);
                param.Add("@Email", user.Email);
                param.Add("@ContactNumber", user.ContactNumber);
                param.Add("@Gender", user.Gender);
                param.Add("@Address", user.Address);
                param.Add("@Course", user.Course);
                param.Add("@Year", user.Year);
                param.Add("@Role", user.Role);
                await connection.ExecuteAsync("SP_SAO", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "User updated successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"UpdateUser error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
        public async Task<ServiceResponse<bool>> DeleteUser(int id)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "DELETEUSER");
                param.Add("@StudentID", id);
                await connection.ExecuteAsync("SP_SAO", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "User deleted successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"DeleteUser error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
    }
}