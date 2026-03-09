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
                param.Add("Role", user.Role);
                param.Add("Course", user.Course);
                param.Add("Year", user.Year);
                param.Add("statementType", "REGISTER");

                await connection.ExecuteAsync(
                    "SP_STUDENT_REGISTRATION",
                    param,
                    commandType: CommandType.StoredProcedure);

                if (user.Role.Equals("student", StringComparison.OrdinalIgnoreCase))
                {
                    var studentSql = @"INSERT INTO Students 
                        (FirstName, LastName, Gender, ContactNumber, Email, 
                         RegistrationDate, DateOfBirth, Address, Course, Year)
                        VALUES 
                        (@FirstName, @LastName, @Gender, @ContactNumber, @Email,
                         @RegistrationDate, @DateOfBirth, @Address, @Course, @Year)";

                    await connection.ExecuteAsync(studentSql, new
                    {
                        user.FirstName,
                        user.LastName,
                        user.Gender,
                        user.ContactNumber,
                        user.Email,
                        RegistrationDate = DateTime.Now,
                        DateOfBirth = user.DateOfBirth != null ? DateTime.Parse(user.DateOfBirth) : (DateTime?)null,
                        user.Address,
                        user.Course,
                        user.Year
                    });
                }

                service.Status = 1;
                service.Message = "User registered successfully.";
            }
            catch (Exception ex)
            {
                service.Status = 0;
                service.Message = $"Registration error: {ex.Message}";
            }
            return service;
        }
    }
}