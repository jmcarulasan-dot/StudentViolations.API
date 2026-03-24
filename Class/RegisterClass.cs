using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Data;
using QRCoder;

namespace StudentViolations.API.Class
{
    // Handles all user registration operations
    public class RegisterClass : IRegisterRepository
    {
        private readonly string _connectionString;

        public RegisterClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        public async Task<ServiceResponse<object>> RegisterUser(User user)
        {
            ServiceResponse<object> service = new ServiceResponse<object>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                // Check if StudentNo already exists before registering a student
                if (user.Role.Equals("Student", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrEmpty(user.StudentNo))
                {
                    DynamicParameters checkParam = new DynamicParameters();
                    checkParam.Add("statementType", "STUDENTNOEXISTS");
                    checkParam.Add("StudentNo", user.StudentNo);
                    checkParam.Add("FirstName", user.FirstName);
                    checkParam.Add("LastName", user.LastName);
                    checkParam.Add("DateOfBirth", user.DateOfBirth);
                    checkParam.Add("Gender", user.Gender);
                    checkParam.Add("Address", user.Address);
                    checkParam.Add("ContactNumber", user.ContactNumber);
                    checkParam.Add("Email", user.Email);
                    checkParam.Add("RegistrationDate", DateTime.Now);
                    checkParam.Add("Username", user.Username);
                    checkParam.Add("PasswordHash", user.PasswordHash);
                    checkParam.Add("Salt", user.Salt);
                    checkParam.Add("Role", user.Role);
                    checkParam.Add("Course", user.Course);
                    checkParam.Add("Year", user.Year);

                    var checkResult = await connection.QueryFirstOrDefaultAsync(
                        "SP_STUDENT_REGISTRATION",
                        checkParam,
                        commandType: CommandType.StoredProcedure);

                    if (checkResult != null && checkResult.StudentNoExists == 1)
                    {
                        service.Status = 0;
                        service.Message = $"Student number {user.StudentNo} is already registered.";
                        return service;
                    }
                }

                // Register the user into the Users table
                DynamicParameters param = new DynamicParameters();
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
                param.Add("StudentNo", user.StudentNo);

                await connection.ExecuteAsync(
                    "SP_STUDENT_REGISTRATION",
                    param,
                    commandType: CommandType.StoredProcedure);

                string studentNo = null;
                string qrCodeBase64 = null;

                // If student role — also insert into Students table and generate QR code
                if (user.Role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    string studentSql = @"INSERT INTO Students
                        (FirstName, LastName, Gender, ContactNumber, Email,
                         RegistrationDate, DateOfBirth, Address, Course, Year)
                        VALUES
                        (@FirstName, @LastName, @Gender, @ContactNumber, @Email,
                         @RegistrationDate, @DateOfBirth, @Address, @Course, @Year);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int newStudentId = await connection.ExecuteScalarAsync<int>(studentSql, new
                    {
                        user.FirstName,
                        user.LastName,
                        user.Gender,
                        user.ContactNumber,
                        user.Email,
                        RegistrationDate = DateTime.Now,
                        DateOfBirth = user.DateOfBirth,
                        user.Address,
                        user.Course,
                        user.Year
                    });

                    studentNo = user.StudentNo;

                    // Generate QR code from StudentNo
                    if (!string.IsNullOrEmpty(studentNo))
                        qrCodeBase64 = GenerateQrCode(studentNo);

                    string updateSql = @"UPDATE Students
                                         SET StudentNo = @StudentNo, QRCode = @QRCode
                                         WHERE StudentID = @Id";

                    await connection.ExecuteAsync(updateSql, new
                    {
                        StudentNo = studentNo,
                        QRCode = qrCodeBase64,
                        Id = newStudentId
                    });
                }

                service.Status = 1;
                service.Message = "User registered successfully.";
                service.Data = new
                {
                    username = user.Username,
                    role = user.Role,
                    studentNo = studentNo,
                    qrCode = qrCodeBase64
                };
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

        // Generates a QR code from the StudentNo and returns it as a Base64 string
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