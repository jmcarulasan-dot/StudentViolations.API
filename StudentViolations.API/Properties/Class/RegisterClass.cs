using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolationsAPI.Model.Entities;
using System;
using System.Data;
using System.Threading.Tasks;

namespace StudentViolations.API.Class
{
    public class RegisterClass : IRegisterRepository
    {
        private readonly string _connectionString;

        public RegisterClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        public async Task<ServiceResponse<object>> RegisterUser(User user)
        {
            var service = new ServiceResponse<object>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var param = new DynamicParameters();
                param.Add("FirstName", user.FirstName);
                param.Add("LastName", user.LastName);
                param.Add("DateOfBirth", user.DateOfBirth);
                param.Add("Gender", user.Gender);
                param.Add("Address", user.Address);
                param.Add("ContactNumber", user.ContactNumber);
                param.Add("Email", user.Email);
                param.Add("RegistrationDate", DateTime.Now);
                param.Add("Username", user.Username);
                param.Add("PasswordHash", user.PasswordHash);
                param.Add("Salt", user.Salt);
                param.Add("statementType", "REGISTER");

                await connection.ExecuteAsync(
                    "SP_STUDENT_REGISTRATION",
                    param,
                    commandType: CommandType.StoredProcedure);

                service.Message = "User registered successfully.";
            }
            catch (Exception ex)
            {
                service.Message = $"Registration error: {ex.Message}";
            }

            return service;
        }
    }
}