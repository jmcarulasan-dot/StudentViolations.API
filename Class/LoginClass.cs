using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Data;

namespace StudentViolations.API.Class
{
    // This class handles login and user registration operations.
    public class LoginClass : ILoginRepository
    {
        private readonly string _connectionString;
        public LoginClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        // Handles user login by verifying credentials against the database.
        public async Task<ServiceResponse<object>> GetLogin(string username, string password)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
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
                        service.Status = 1;
                        service.Message = "Login successful.";
                        service.Data = new
                        {
                            id = result.StudentID.ToString(),
                            username = result.Username,
                            name = $"{result.FirstName} {result.LastName}",
                            role = result.Role,
                            email = result.Email,
                            contactNumber = result.ContactNumber,
                            studentNo = result.StudentNo ?? ""
                        };
                    }
                    else
                    {
                        service.Status = 0;
                        service.Message = "Invalid username or password.";
                    }
                }
                else
                {
                    service.Status = 0;
                    service.Message = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                service.Status = 0;
                service.Message = $"Login error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        // Checks if a username or email already exists in the system.
        public async Task<bool> UserExists(string username, string email)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("username", username);
                param.Add("email", email);

                int result = await connection.QueryFirstOrDefaultAsync<int>(
                    "SP_STUDENT_GETUSERLOGIN",
                    param,
                    commandType: CommandType.StoredProcedure);

                return result > 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        // Registers a new user in the system.
        public async Task<ServiceResponse<object>> RegisterUser(User user)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                string sql = @"INSERT INTO Users 
                    (Username, PasswordHash, Salt, Email, Gender, ContactNumber, 
                     RegistrationDate, Role, FirstName, LastName)
                    VALUES 
                    (@Username, @PasswordHash, @Salt, @Email, @Gender, @ContactNumber, 
                     @RegistrationDate, @Role, @FirstName, @LastName)";
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

                service.Status = 1;
                service.Message = "Registration successful.";
                service.Data = new { username = user.Username, role = user.Role };
            }
            catch (Exception ex)
            {
                service.Status = 0;
                service.Message = $"Registration error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        // Helper method to hash a password using PBKDF2.
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