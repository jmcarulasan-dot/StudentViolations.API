using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using System.Data;

namespace StudentViolations.API.Class
{
    public class AdmissionClass : IAdmissionRepository
    {
        private readonly string _connectionString;

        public AdmissionClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        public async Task<ServiceResponse<List<UserModel>>> GetAllUsers()
        {
            var service = new ServiceResponse<List<UserModel>>();

            using var connection = new SqlConnection(_connectionString);

            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "GETALLUSERS");

                var result = await connection.QueryAsync<UserModel>(
                    "SP_ADMISSION",
                    param,
                    commandType: CommandType.StoredProcedure);

                service.Status = 200;
                service.Message = "Users retrieved successfully.";
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetAllUsers error: {ex.Message}";
            }

            return service;
        }

        public async Task<ServiceResponse<UserModel>> GetUserById(int id)
        {
            var service = new ServiceResponse<UserModel>();

            using var connection = new SqlConnection(_connectionString);

            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "GETUSERBYID");
                param.Add("@StudentID", id);

                var result = await connection.QueryFirstOrDefaultAsync<UserModel>(
                    "SP_ADMISSION",
                    param,
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                {
                    service.Status = 404;
                    service.Message = "User not found.";
                    return service;
                }

                service.Status = 200;
                service.Message = "User retrieved successfully.";
                service.Data = result;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetUserById error: {ex.Message}";
            }

            return service;
        }

        public async Task<ServiceResponse<bool>> UpdateUser(UserModel user)
        {
            var service = new ServiceResponse<bool>();

            using var connection = new SqlConnection(_connectionString);

            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "UPDATEUSER");
                param.Add("@StudentID", user.StudentID);
                param.Add("@FirstName", user.FirstName);
                param.Add("@LastName", user.LastName);
                param.Add("@Email", user.Email);
                param.Add("@ContactNumber", user.ContactNumber);
                param.Add("@Gender", user.Gender);
                param.Add("@Address", user.Address);
                param.Add("@Role", user.Role);

                await connection.ExecuteAsync(
                    "SP_ADMISSION",
                    param,
                    commandType: CommandType.StoredProcedure);

                service.Status = 200;
                service.Message = "User updated successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"UpdateUser error: {ex.Message}";
            }

            return service;
        }

        public async Task<ServiceResponse<bool>> DeleteUser(int id)
        {
            var service = new ServiceResponse<bool>();

            using var connection = new SqlConnection(_connectionString);

            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "DELETEUSER");
                param.Add("@StudentID", id);

                await connection.ExecuteAsync(
                    "SP_ADMISSION",
                    param,
                    commandType: CommandType.StoredProcedure);

                service.Status = 200;
                service.Message = "User deleted successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"DeleteUser error: {ex.Message}";
            }

            return service;
        }

        public async Task<ServiceResponse<string>> GetStudentQrCode(string studentNo)
        {
            var service = new ServiceResponse<string>();

            using var connection = new SqlConnection(_connectionString);

            try
            {
                var param = new DynamicParameters();
                param.Add("@statementType", "GETSTUDENTQR");
                param.Add("@StudentNo", studentNo);

                var qrCode = await connection.QueryFirstOrDefaultAsync<string>(
                    "SP_ADMISSION",
                    param,
                    commandType: CommandType.StoredProcedure);

                if (string.IsNullOrWhiteSpace(qrCode))
                {
                    service.Status = 404;
                    service.Message = "QR code not found for this student.";
                    return service;
                }

                service.Status = 200;
                service.Message = "QR code retrieved successfully.";
                service.Data = qrCode;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetStudentQrCode error: {ex.Message}";
            }

            return service;
        }
    }
}
