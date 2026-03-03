using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;

using StudentViolationsAPI.Model.Entities;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace StudentViolations.API.Class
{
    public class LoginClass : ILoginRepository
    {
        private readonly string _connectionString;

        public LoginClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        public async Task<ServiceResponse<object>> GetLogin(string username, string password)
        {
            var service = new ServiceResponse<object>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var param = new DynamicParameters();
                param.Add("username", username);
                param.Add("password", password);
                param.Add("statementType", "GETLOGIN");

                var result = (await connection.QueryAsync("SP_STUDENT_GETUSERLOGIN",param,commandType: CommandType.StoredProcedure)).FirstOrDefault();

                if (result != null)
                {
                    service.Data = result;
                    service.Message = "Login successful.";
                }
                else
                {
                    service.Message = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                service.Message = $"Login error: {ex.Message}";
            }

            return service;
        }

        public Task<ServiceResponse<object>> RegisterUser(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UserExists(string username, string email)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var param = new DynamicParameters();
                param.Add("username", username);
                param.Add("email", email);
                param.Add("statementType", "USEREXISTS");

                var result = await connection.QueryFirstOrDefaultAsync<int>("SP_STUDENT_GETUSERLOGIN",param,commandType: CommandType.StoredProcedure);

                return result > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}