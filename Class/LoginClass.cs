using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using StudentViolations.API.IRepository;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentViolations.API.Class
{
    public class LoginClass : ILoginRepository
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public LoginClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
            _configuration = configuration;
        }

        public async Task<ServiceResponse<UserModel>> Authenticate(string username, string password)
        {
            var service = new ServiceResponse<UserModel>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETLOGIN");
                param.Add("@username", username);
                param.Add("@email", "");

                var result = (await connection.QueryAsync<UserModel>(
                    "SP_STUDENT_GETUSERLOGIN", param,
                    commandType: CommandType.StoredProcedure)).FirstOrDefault();

                if (result == null)
                {
                    service.Status = 400;
                    service.Message = "Invalid username or password.";
                    return service;
                }

                if (result.Status == "Dismissed")
                {
                    service.Status = 400;
                    service.Message = "Your account has been dismissed. Please contact the SAO office.";
                    return service;
                }

                string hashedPassword = HashPassword(password, result.Salt);
                if (hashedPassword != result.PasswordHash)
                {
                    service.Status = 400;
                    service.Message = "Invalid username or password.";
                    return service;
                }

                service.Status = 200;
                service.Message = "Login successful.";
                service.Data = result;
                service.Token = GenerateToken(result);
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"Login error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        public async Task<ServiceResponse<bool>> UserExists(string username, string email)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@username", username);
                param.Add("@email", email);
                param.Add("@statementType", "USEREXISTS");

                int result = await connection.QueryFirstOrDefaultAsync<int>(
                    "SP_STUDENT_GETUSERLOGIN", param,
                    commandType: CommandType.StoredProcedure);

                service.Status = 200;
                service.Data = result > 0;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"UserExists error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        // Generates JWT token from the logged in user
        private string GenerateToken(UserModel user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.StudentID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("name", $"{user.FirstName} {user.LastName}"),
                new Claim("studentNo", user.StudentNo ?? ""),
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpiryInHours"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // PBKDF2 password hashing
        private string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}