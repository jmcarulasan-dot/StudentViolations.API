using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolationsAPI.Model.Entities;
using System.Data;

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
                param.Add("email", "");
                param.Add("statementType", "GETLOGIN");

                var result = (await connection.QueryAsync(
                    "SP_STUDENT_GETUSERLOGIN",
                    param,
                    commandType: CommandType.StoredProcedure)).FirstOrDefault();

                if (result != null)
                {
                    string salt = result.Salt;
                    string hashedPassword = HashPassword(password, salt);

                    if (hashedPassword == result.PasswordHash)
                    {
                        service.Data = new { username = result.Username };
                        service.Message = "Login successful.";
                    }
                    else
                    {
                        service.Message = "Invalid username or password.";
                    }
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

                var result = await connection.QueryFirstOrDefaultAsync<int>(
                    "SP_STUDENT_GETUSERLOGIN",
                    param,
                    commandType: CommandType.StoredProcedure);

                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ServiceResponse<object>> RegisterUser(User user)
        {
            var service = new ServiceResponse<object>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"INSERT INTO Student_Login 
                    (Username, PasswordHash, Salt, Email, Gender, ContactNumber, RegistrationDate, Role, FirstName, LastName)
                    VALUES 
                    (@Username, @PasswordHash, @Salt, @Email, @Gender, @ContactNumber, @RegistrationDate, @Role, @FirstName, @LastName)";

                await connection.ExecuteAsync(sql, new
                {
                    user.Username,
                    user.PasswordHash,
                    user.Salt,
                    user.Email,
                    user.Gender,
                    user.ContactNumber,
                    RegistrationDate = DateTime.Now,
                    user.Role,
                    user.FirstName,
                    user.LastName
                });

               
                if (user.Role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    var studentSql = @"INSERT INTO Students 
                     (FirstName, LastName, Gender, ContactNumber, Email, RegistrationDate, DateOfBirth, Address)
                     VALUES 
                     (@FirstName, @LastName, @Gender, @ContactNumber, @Email, @RegistrationDate, @DateOfBirth, @Address)";

                    await connection.ExecuteAsync(studentSql, new
                    {
                        user.FirstName,
                        user.LastName,
                        user.Gender,
                        user.ContactNumber,
                        user.Email,
                        RegistrationDate = DateTime.Now,
                        DateOfBirth = user.DateOfBirth != null ? DateTime.Parse(user.DateOfBirth) : (DateTime?)null,
                        user.Address
                    });
                }

                service.Message = "Registration successful.";
                service.Data = new { username = user.Username, role = user.Role };
            }
            catch (Exception ex)
            {
                service.Message = $"Registration error: {ex.Message}";
            }
            return service;
        }

        private string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            string hashed = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: saltBytes,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8));
            return hashed;
        }
    }
}