using Dapper;
using Microsoft.Data.SqlClient;
using QRCoder;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using System.Data;

namespace StudentViolations.API.Class
{
    public class RegisterClass : IRegisterRepository
    {
        private readonly string _connectionString;
        public RegisterClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        public async Task<ServiceResponse<UserModel>> RegisterUser(UserModel user)
        {
            var service = new ServiceResponse<UserModel>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                // Check if StudentNo already exists for student role
                if (user.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(user.StudentNo))
                {
                    DynamicParameters checkParam = new DynamicParameters();
                    checkParam.Add("@statementType", "STUDENTNOEXISTS");
                    checkParam.Add("@StudentNo", user.StudentNo);
                    checkParam.Add("@FirstName", user.FirstName);
                    checkParam.Add("@LastName", user.LastName);
                    checkParam.Add("@DateOfBirth", user.DateOfBirth);
                    checkParam.Add("@Gender", user.Gender);
                    checkParam.Add("@Address", user.Address);
                    checkParam.Add("@ContactNumber", user.ContactNumber);
                    checkParam.Add("@Email", user.Email);
                    checkParam.Add("@RegistrationDate", DateTime.Now);
                    checkParam.Add("@Username", user.Username);
                    checkParam.Add("@PasswordHash", user.PasswordHash);
                    checkParam.Add("@Salt", user.Salt);
                    checkParam.Add("@Role", user.Role);
                    checkParam.Add("@Course", user.Course);
                    checkParam.Add("@Year", user.Year);

                    var checkResult = await connection.QueryFirstOrDefaultAsync(
                        "SP_STUDENT_REGISTRATION", checkParam,
                        commandType: CommandType.StoredProcedure);

                    if (checkResult != null && checkResult.StudentNoExists == 1)
                    {
                        service.Status = 400;
                        service.Message = $"Student number {user.StudentNo} is already registered.";
                        return service;
                    }
                }

                // Generate QR code if student
                string? qrCodeBase64 = null;
                if (user.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(user.StudentNo))
                {
                    qrCodeBase64 = GenerateQrCode(user.StudentNo);
                }

                // Register user — SP handles both Users and Students inserts in one transaction
                DynamicParameters param = new DynamicParameters();
                param.Add("@FirstName", user.FirstName);
                param.Add("@LastName", user.LastName);
                param.Add("@DateOfBirth", user.DateOfBirth);
                param.Add("@Gender", user.Gender);
                param.Add("@Address", user.Address);
                param.Add("@ContactNumber", user.ContactNumber);
                param.Add("@Email", user.Email);
                param.Add("@RegistrationDate", DateTime.Now);
                param.Add("@Username", user.Username);
                param.Add("@PasswordHash", user.PasswordHash);
                param.Add("@Salt", user.Salt);
                param.Add("@Role", user.Role);
                param.Add("@Course", user.Course);
                param.Add("@Year", user.Year);
                param.Add("@statementType", "REGISTER");
                param.Add("@StudentNo", user.StudentNo);
                param.Add("@QRCode", qrCodeBase64);

                await connection.ExecuteAsync(
                    "SP_STUDENT_REGISTRATION", param,
                    commandType: CommandType.StoredProcedure);

                service.Status = 200;
                service.Message = "User registered successfully.";
                service.Data = user;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"Registration error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        private string GenerateQrCode(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrData);
            byte[] qrBytes = qrCode.GetGraphic(10);
            return Convert.ToBase64String(qrBytes);
        }
    }
}