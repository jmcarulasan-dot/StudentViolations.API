using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using System.Data;

namespace StudentViolations.API.Class
{
    public class AdmissionClass : IAdmissionRepository
    {
        private readonly string _connectionString;
        private readonly IEmailRepository _emailRepository;

        public AdmissionClass(IConfiguration configuration, IEmailRepository emailRepository)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
            _emailRepository = emailRepository;
        }

        public async Task<ServiceResponse<EnrollmentModel>> CreateEnrollment(EnrollmentModel enrollment)
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@statementType", "CREATEENROLLMENT");
            p.Add("@StudentNo", enrollment.StudentNo);
            p.Add("@FirstName", enrollment.FirstName);
            p.Add("@LastName", enrollment.LastName);
            p.Add("@DateOfBirth", enrollment.DateOfBirth);
            p.Add("@Gender", enrollment.Gender);
            p.Add("@Address", enrollment.Address);
            p.Add("@ContactNumber", enrollment.ContactNumber);
            p.Add("@Email", enrollment.Email);
            p.Add("@Course", enrollment.Course);
            p.Add("@Year", enrollment.Year);
            var result = await connection.QueryFirstOrDefaultAsync<EnrollmentModel>("SP_ADMISSION", p, commandType: CommandType.StoredProcedure);
            return result == null
                ? new ServiceResponse<EnrollmentModel> { Status = 400, Message = "Enrollment could not be created." }
                : new ServiceResponse<EnrollmentModel> { Status = 200, Message = "Enrollment record saved.", Data = result };
        }

        public async Task<ServiceResponse<List<EnrollmentModel>>> GetEnrollments()
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@statementType", "GETENROLLMENTS");
            var data = (await connection.QueryAsync<EnrollmentModel>("SP_ADMISSION", p, commandType: CommandType.StoredProcedure)).ToList();
            return new ServiceResponse<List<EnrollmentModel>> { Status = 200, Message = "Enrollments retrieved.", Data = data };
        }

        public async Task<ServiceResponse<List<RegistrationRequestModel>>> GetRegistrationRequests()
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@statementType", "GETREGISTRATIONREQUESTS");
            var data = (await connection.QueryAsync<RegistrationRequestModel>("SP_ADMISSION", p, commandType: CommandType.StoredProcedure)).ToList();
            return new ServiceResponse<List<RegistrationRequestModel>> { Status = 200, Message = "Registration requests retrieved.", Data = data };
        }

        public async Task<ServiceResponse<bool>> SubmitRegistrationRequest(StudentRegistrationRequestModel request)
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@statementType", "SUBMITREGISTRATION");
            p.Add("@StudentNo", request.StudentNo);
            p.Add("@DateOfBirth", DateTime.Parse(request.DateOfBirth));
            p.Add("@Email", request.Email);
            p.Add("@Username", request.Username);
            var saltBytes = RandomNumberGenerator.GetBytes(16);
            var salt = Convert.ToBase64String(saltBytes);
            var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(request.Password, saltBytes, KeyDerivationPrf.HMACSHA256, 10000, 32));
            p.Add("@PasswordHash", hash);
            p.Add("@Salt", salt);
            
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_ADMISSION", p, commandType: CommandType.StoredProcedure);
            if (result == null || (int)result.Success != 1)
                return new ServiceResponse<bool> { Status = 400, Message = (string?)result?.Message ?? "Registration request was rejected.", Data = false };
            return new ServiceResponse<bool> { Status = 200, Message = "Registration request submitted for Admission verification.", Data = true };
        }

        public async Task<ServiceResponse<UserModel>> ApproveRegistration(int requestId)
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@statementType", "APPROVEREGISTRATION");
            p.Add("@RequestId", requestId);
            var user = await connection.QueryFirstOrDefaultAsync<UserModel>("SP_ADMISSION", p, commandType: CommandType.StoredProcedure);
            if (user == null)
                return new ServiceResponse<UserModel> { Status = 400, Message = "Registration request could not be approved." };

            var emailSent = await _emailRepository.SendEmailAsync(
                user.Email,
                "SVS Account Approved",
                $"<p>Hello {user.FirstName},</p><p>Your Student Violations System account has been verified by Admission and successfully created.</p><p>You may now log in to the SVS mobile application.</p><p>Username: <b>{user.Username}</b></p>");

            return new ServiceResponse<UserModel>
            {
                Status = 200,
                Message = emailSent
                    ? "Student account approved, created, and email sent."
                    : "Student account approved and created, but the confirmation email could not be sent. Configure EmailSettings.",
                Data = user
            };
        }

        public async Task<ServiceResponse<bool>> RejectRegistration(int requestId)
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@statementType", "REJECTREGISTRATION");
            p.Add("@RequestId", requestId);
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_ADMISSION", p, commandType: CommandType.StoredProcedure);
            return new ServiceResponse<bool>
            {
                Status = (result != null && (int)result.Success == 1) ? 200 : 400,
                Message = (string?)result?.Message ?? "Registration request could not be rejected.",
                Data = result != null && (int)result.Success == 1
            };
        }

        public async Task<ServiceResponse<bool>> CanSignClearance(string studentNo)
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@statementType", "CANCLEARANCE");
            p.Add("@StudentNo", studentNo);
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_ADMISSION", p, commandType: CommandType.StoredProcedure);
            return new ServiceResponse<bool>
            {
                Status = 200,
                Message = (string?)result?.Message ?? "Clearance status checked.",
                Data = result != null && (int)result.CanSign == 1
            };
        }

        public async Task<ServiceResponse<bool>> SignClearance(string studentNo)
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            var p = new DynamicParameters();
            p.Add("@statementType", "SIGNCLEARANCE");
            p.Add("@StudentNo", studentNo);
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>("SP_ADMISSION", p, commandType: CommandType.StoredProcedure);
            return new ServiceResponse<bool>
            {
                Status = (result != null && (int)result.Success == 1) ? 200 : 400,
                Message = (string?)result?.Message ?? "Clearance could not be signed.",
                Data = result != null && (int)result.Success == 1
            };
        }
    }
}