using Dapper;
using Microsoft.Data.SqlClient; // Use Microsoft.Data.SqlClient
using Microsoft.Extensions.Configuration;
using StudentViolations.API.Controllers;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation; // Import for password hashing

namespace StudentViolations.API.Class
{
    public class LoginClass : ILoginRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString; // Store connection string

        public LoginClass(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("StudentViolationsdb"); // Get from config
        }

        public async Task<ServiceResponse<object>> GetLogin(string username, string password)
        {
            var service = new ServiceResponse<object>();
            SqlConnection connection = null; // Declare connection

            try
            {
                connection = new SqlConnection(_connectionString); // Initialize connection
                await connection.OpenAsync(); // Open asynchronously

                var param = new DynamicParameters();
                param.Add("username", username);
                param.Add("statementType", "GETLOGIN"); // Correct statement type

                // Execute the stored procedure and retrieve the user data
                var result = (await connection.QueryAsync("SP_STUDENT_GETUSERLOGIN", param, commandType: CommandType.StoredProcedure)).FirstOrDefault(); // Use FirstOrDefault

                if (result != null)
                {
                    // User found, now verify the password
                    string storedPasswordHash = result.PasswordHash; // Assuming PasswordHash is a property in the result
                    string salt = result.Salt; // Assuming Salt is a property in the result

                    // Hash the provided password using the stored salt
                    string hashedPassword = HashPassword(password, salt);

                    // Compare the hashed password with the stored password hash
                    if (hashedPassword == storedPasswordHash)
                    {
                   
                        service.Data = result; // Return the user data
                        service.Message = "Login successful.";
                    }
                    else
                    {
                        service.Message = "Incorrect password.";
                    }
                }
                else
                {
                  
                    service.Message = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error during login: {ex.Message}");
             
                service.Message = $"An error occurred during login: {ex.Message}";
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close(); // Explicitly close the connection
                    connection.Dispose(); // Explicitly dispose the connection
                }
            }

            return service;
        }

        public async Task<bool> UserExists(string username, string email)
        {
            SqlConnection connection = null; // Declare connection
            try
            {
                connection = new SqlConnection(_connectionString); // Initialize connection
                await connection.OpenAsync();

                var param = new DynamicParameters();
                param.Add("username", username);
                param.Add("email", email);
                param.Add("statementType", "USEREXISTS"); // New statement type

                var result = await connection.QueryFirstOrDefaultAsync<int>("SP_STUDENT_GETUSERLOGIN", param, commandType: CommandType.StoredProcedure); // Execute the query

                return result > 0; // Return true if a user exists
            }
            catch (Exception)
            {
                return false; // Or log the exception
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close(); // Explicitly close the connection
                    connection.Dispose(); // Explicitly dispose the connection
                }
            }
        }

        public async Task<ServiceResponse<object>> RegisterUser(User user)
        {
            var service = new ServiceResponse<object>();
            SqlConnection connection = null; // Declare connection

            try
            {
                connection = new SqlConnection(_connectionString); // Initialize connection
                await connection.OpenAsync();

                var param = new DynamicParameters();
                param.Add("FirstName", user); // Map properties
                param.Add("LastName", user);   // Map properties
                param.Add("DateOfBirth", user); // Map properties
                param.Add("Gender", user.Gender);       // Map properties
                param.Add("Address", user);     // Map properties
                param.Add("ContactNumber", user); // Map properties
                param.Add("Email", user.Email);         // Map properties
                param.Add("RegistrationDate", DateTime.Now); // Map properties
                param.Add("Username", user.Username);
                param.Add("PasswordHash", user.PasswordHash);
                param.Add("Salt", user.Salt);
                param.Add("statementType", "REGISTER"); // Correct statement type

                await connection.ExecuteAsync("SP_STUDENT_REGISTRATION", param, commandType: CommandType.StoredProcedure); // Execute the query

               
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

        // Helper method for password hashing (same as in the controller)
        private string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
    }
}