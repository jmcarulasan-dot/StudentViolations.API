using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using System.Data;

namespace StudentViolations.API.Class
{
    public class GuardClass : IGuardRepository
    {
        private readonly string _connectionString;
        public GuardClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }
        public async Task<ServiceResponse<List<ViolationModel>>> GetViolationsInDateRange(DateTime startDate, DateTime endDate)
        {
            var service = new ServiceResponse<List<ViolationModel>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETBYDATE");
                param.Add("@StartDate", startDate);
                param.Add("@EndDate", endDate);

                var result = await connection.QueryAsync<ViolationModel>("SP_GUARD", param, commandType: CommandType.StoredProcedure);
                var list = result.ToList();
                if (list.Count == 0)
                {
                    service.Status = 404;
                    service.Message = "No violations found in this date range.";
                    return service;
                }
                service.Status = 200;
                service.Data = list;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetViolationsInDateRange error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }

        public async Task<ServiceResponse<List<ViolationModel>>> GetViolationsByStudentId(string studentNo)
        {
            var service = new ServiceResponse<List<ViolationModel>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETBYSTUDENT");
                param.Add("@StudentNo", studentNo);
                var result = await connection.QueryAsync<ViolationModel>("SP_GUARD", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetViolationsByStudentId error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }

        public async Task<ServiceResponse<StudentModel>> GetStudentByQrCode(string qrCode)
        {
            var service = new ServiceResponse<StudentModel>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETSTUDENTBYQR");
                param.Add("@StudentNo", qrCode);
                var result = await connection.QueryFirstOrDefaultAsync<StudentModel>("SP_GUARD", param, commandType: CommandType.StoredProcedure);
                if (result == null)
                {
                    service.Status = 404;
                    service.Message = "Student not found.";
                }
                else
                {
                    service.Status = 200;
                    service.Data = result;
                }
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetStudentByQrCode error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }

        public async Task<ServiceResponse<bool>> RecordViolation(ViolationModel violation)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "RECORDVIOLATION");
                param.Add("@StudentId", violation.StudentId);
                param.Add("@ViolationName", violation.ViolationName);
                param.Add("@Description", violation.Description);
                param.Add("@Severity", violation.Severity);
                param.Add("@GuardId", violation.GuardId);
                var result = await connection.QueryAsync("SP_GUARD", param, commandType: CommandType.StoredProcedure);
                var errorRow = result.FirstOrDefault();
                if (errorRow != null && errorRow.ErrorMessage != null)
                {
                    service.Status = 400;
                    service.Message = (string)errorRow.ErrorMessage;
                    return service;
                }
                service.Status = 200;
                service.Message = "Violation recorded successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"RecordViolation error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }

        public async Task<ServiceResponse<List<StudentModel>>> GetAllStudents()
        {
            var service = new ServiceResponse<List<StudentModel>>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETALLSTUDENTS");
                var result = await connection.QueryAsync<StudentModel>("SP_GUARD", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Data = result.ToList();
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetAllStudents error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }

        public async Task<ServiceResponse<StudentModel>> GetStudentByStudentNo(string studentNo)
        {
            var service = new ServiceResponse<StudentModel>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETSTUDENTBYNO");
                param.Add("@StudentNo", studentNo);
                var result = await connection.QueryFirstOrDefaultAsync<StudentModel>(
                    "SP_GUARD", param, commandType: CommandType.StoredProcedure);
                if (result == null)
                {
                    service.Status = 404;
                    service.Message = "Student not found.";
                    return service;
                }
                service.Status = 200;
                service.Data = result;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetStudentByStudentNo error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }
            return service;
        }
    }
}