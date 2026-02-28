using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentViolations.API.Controllers;
using StudentViolations.API.IRepository;
using StudentViolationsAPI.Model.Entities;
using StudentViolations.API.Model;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace StudentViolations.API.Class
{
    public class LoginClass : ILoginRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public LoginClass(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("StudentViolationsdb");
        }

        public async Task<ServiceResponse<object>> GetLogin(string username, string password)
        {
            var service = new ServiceResponse<object>();
            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var param = new DynamicParameters();
                param.Add("username", username);
                param.Add("password", password);
                param.Add("statementType", "GETLOGIN");

                var result = (await connection.QueryAsync("SP_STUDENT_GETUSERLOGIN", param, commandType: CommandType.StoredProcedure)).FirstOrDefault();

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
                Console.WriteLine($"Error during login: {ex.Message}");
                service.Message = $"An error occurred during login: {ex.Message}";
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            return service;
        }

        public async Task<bool> UserExists(string username, string email)
        {
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var param = new DynamicParameters();
                param.Add("username", username);
                param.Add("email", email);
                param.Add("statementType", "USEREXISTS");

                var result = await connection.QueryFirstOrDefaultAsync<int>("SP_STUDENT_GETUSERLOGIN", param, commandType: CommandType.StoredProcedure);

                return result > 0;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }

        public async Task<ServiceResponse<object>> RegisterUser(User user)
        {
            var service = new ServiceResponse<object>();
            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var param = new DynamicParameters();
                param.Add("FirstName", user);
                param.Add("LastName", user);
                param.Add("DateOfBirth", user);
                param.Add("Gender", user.Gender);
                param.Add("Address", user);
                param.Add("ContactNumber", user);
                param.Add("Email", user.Email);
                param.Add("RegistrationDate", DateTime.Now);
                param.Add("Username", user.Username);
                param.Add("PasswordHash", user.PasswordHash);
                param.Add("Salt", user.Salt);
                param.Add("statementType", "REGISTER");

                await connection.ExecuteAsync("SP_STUDENT_REGISTRATION", param, commandType: CommandType.StoredProcedure);

                service.Message = "User registered successfully.";
            }
            catch (Exception ex)
            {
                service.Message = ex.Message;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            return service;
        }
    }
}