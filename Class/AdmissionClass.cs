using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using System.Data;
using System.Security.Cryptography;

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
            var service = new ServiceResponse<EnrollmentModel>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "CREATEENROLLMENT");
                param.Add("@StudentNo", enrollment.StudentNo);
                param.Add("@FirstName", enrollment.FirstName);
                param.Add("@LastName", enrollment.LastName);
                param.Add("@Email", enrollment.Email);
                param.Add("@Course", enrollment.Course);
                param.Add("@Year", enrollment.Year);

                var result = await connection.QueryFirstOrDefaultAsync<EnrollmentModel>(
                    "SP_ADMISSION", param, commandType: CommandType.StoredProcedure);
                if (result == null || string.IsNullOrWhiteSpace(result.StudentNo))
                {
                    service.Status = 400;
                    service.Message = "Enrollment could not be created.";
                    service.Data = null;
                }
                else
                {
                    service.Status = 200;
                    service.Message = "Enrollment record saved.";
                    service.Data = result;
                }
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"CreateEnrollment error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        public async Task<ServiceResponse<List<EnrollmentModel>>> GetEnrollments()
        {
            var service = new ServiceResponse<List<EnrollmentModel>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETENROLLMENTS");
                var result = await connection.QueryAsync<EnrollmentModel>(
                    "SP_ADMISSION", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Enrollments retrieved.";
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetEnrollments error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        public async Task<ServiceResponse<List<RegistrationRequestModel>>> GetRegistrationRequests()
        {
            var service = new ServiceResponse<List<RegistrationRequestModel>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETREGISTRATIONREQUESTS");
                var result = await connection.QueryAsync<RegistrationRequestModel>(
                    "SP_ADMISSION", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Registration requests retrieved.";
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetRegistrationRequests error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        public async Task<ServiceResponse<bool>> SubmitRegistrationRequest(StudentRegistrationRequestModel request)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "SUBMITREGISTRATION");
                param.Add("@StudentNo", request.StudentNo);
                param.Add("@Email", request.Email);
                param.Add("@Username", request.Username);
                var saltBytes = RandomNumberGenerator.GetBytes(16);
                var salt = Convert.ToBase64String(saltBytes);
                var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    request.Password, saltBytes, KeyDerivationPrf.HMACSHA256, 10000, 32));
                param.Add("@PasswordHash", hash);
                param.Add("@Salt", salt);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SP_ADMISSION", param, commandType: CommandType.StoredProcedure);
                if (result == null || (int)result.Success != 1)
                {
                    service.Status = 400;
                    service.Message = (string?)result?.Message ?? "Registration request was rejected.";
                    service.Data = false;
                }
                else
                {
                    service.Status = 200;
                    service.Message = "Registration request submitted for Admission verification.";
                    service.Data = true;
                }
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"SubmitRegistrationRequest error: {ex.Message}";
                service.Data = false;
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        public async Task<ServiceResponse<UserModel>> ApproveRegistration(int requestId)
        {
            var service = new ServiceResponse<UserModel>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "APPROVEREGISTRATION");
                param.Add("@RequestId", requestId);
                var user = await connection.QueryFirstOrDefaultAsync<UserModel>(
                    "SP_ADMISSION", param, commandType: CommandType.StoredProcedure);
                connection.Close();

                if (user == null)
                {
                    service.Status = 400;
                    service.Message = "Registration request could not be approved.";
                    return service;
                }

                var emailSent = await _emailRepository.SendEmailAsync(
                    user.Email,
                    "SVS Account Approved",
                    $"<p>Hello {user.FirstName},</p><p>Your Student Violations System account has been verified by Admission and successfully created.</p><p>You may now log in to the SVS mobile application.</p><p>Username: <b>{user.Username}</b></p>");

                service.Status = 200;
                service.Message = emailSent
                    ? "Student account approved, created, and email sent."
                    : "Student account approved and created, but the confirmation email could not be sent. Configure EmailSettings.";
                service.Data = user;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"ApproveRegistration error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        public async Task<ServiceResponse<bool>> RejectRegistration(int requestId)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "REJECTREGISTRATION");
                param.Add("@RequestId", requestId);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SP_ADMISSION", param, commandType: CommandType.StoredProcedure);
                var succeeded = result != null && (int)result.Success == 1;
                service.Status = succeeded ? 200 : 400;
                service.Message = (string?)result?.Message ?? "Registration request could not be rejected.";
                service.Data = succeeded;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"RejectRegistration error: {ex.Message}";
                service.Data = false;
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        public async Task<ServiceResponse<bool>> CanSignClearance(string studentNo)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "CANCLEARANCE");
                param.Add("@StudentNo", studentNo);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SP_ADMISSION", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = (string?)result?.Message ?? "Clearance status checked.";
                service.Data = result != null && (int)result.CanSign == 1;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"CanSignClearance error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }

        public async Task<ServiceResponse<bool>> SignClearance(string studentNo)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "SIGNCLEARANCE");
                param.Add("@StudentNo", studentNo);
                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    "SP_ADMISSION", param, commandType: CommandType.StoredProcedure);
                var succeeded = result != null && (int)result.Success == 1;
                service.Status = succeeded ? 200 : 400;
                service.Message = (string?)result?.Message ?? "Clearance could not be signed.";
                service.Data = succeeded;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"SignClearance error: {ex.Message}";
                service.Data = false;
            }
            finally
            {
                connection.Close();
            }
            return service;
        }
    }
}
