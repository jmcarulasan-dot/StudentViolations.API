using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using System.Data;

namespace StudentViolations.API.Class
{
    public class StudentClass : IStudentRepository
    {
        private readonly string _connectionString;
        public StudentClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }
        public async Task<ServiceResponse<StudentModel>> GetStudentByStudentId(string studentNo)
        {
            var service = new ServiceResponse<StudentModel>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETSTUDENT");
                param.Add("@StudentNo", studentNo);
                var result = await connection.QueryFirstOrDefaultAsync<StudentModel>("SP_STUDENT_DATA", param, commandType: CommandType.StoredProcedure);
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
                service.Message = $"GetStudentByStudentId error: {ex.Message}";
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
                var result = await connection.QueryAsync<StudentModel>("SP_STUDENT_DATA", param, commandType: CommandType.StoredProcedure);
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
        public async Task<ServiceResponse<bool>> UpdateStudent(StudentModel student)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "UPDATESTUDENT");
                param.Add("@StudentID", student.StudentID);
                param.Add("@FirstName", student.FirstName);
                param.Add("@LastName", student.LastName);
                param.Add("@Email", student.Email);
                param.Add("@ContactNumber", student.ContactNumber);
                param.Add("@Gender", student.Gender);
                param.Add("@Address", student.Address);
                param.Add("@Course", student.Course);
                param.Add("@Year", student.Year);
                await connection.ExecuteAsync("SP_STUDENT_DATA", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Student updated successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"UpdateStudent error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }

        public async Task<ServiceResponse<bool>> UpdateStudentStatus(int studentId, string status)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "UPDATESTATUS");
                param.Add("@StudentID", studentId);
                param.Add("@Status", status);
                await connection.ExecuteAsync("SP_STUDENT_DATA", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = $"Student status updated to {status} successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"UpdateStudentStatus error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
        public async Task<ServiceResponse<string>> GetUsernameByStudentNo(string studentNo)
        {
            var service = new ServiceResponse<string>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETUSERNAMEBYNO");
                param.Add("@StudentNo", studentNo);
                var result = await connection.QueryFirstOrDefaultAsync<string>(
                    "SP_STUDENT_DATA", param, commandType: CommandType.StoredProcedure);
                if (result == null)
                {
                    service.Status = 404;
                    service.Message = "Username not found.";
                    return service;
                }
                service.Status = 200;
                service.Data = result;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"GetUsernameByStudentNo error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
        public async Task<ServiceResponse<bool>> UpdateProfilePhoto(string studentNo, string base64Photo)
        {
            var service = new ServiceResponse<bool>();
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "UPDATEPHOTO");
                param.Add("@StudentNo", studentNo);
                param.Add("@ProfilePhoto", base64Photo);
                await connection.ExecuteAsync("SP_STUDENT_DATA", param, commandType: CommandType.StoredProcedure);
                service.Status = 200;
                service.Message = "Profile photo updated successfully.";
                service.Data = true;
            }
            catch (Exception ex)
            {
                service.Status = 500;
                service.Message = $"UpdateProfilePhoto error: {ex.Message}";
            }
            finally { connection.Close(); }
            return service;
        }
    }
}