using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using System.Data;

namespace StudentViolations.API.Class
{
    // Handles all guard-related database operations
    public class GuardClass : IGuardRepository
    {
        private readonly string _connectionString;

        public GuardClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        // Gets all violations between two dates
        public async Task<List<dynamic>> GetViolationsInDateRange(DateTime startDate, DateTime endDate)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETBYDATE");
                param.Add("StartDate", startDate);
                param.Add("EndDate", endDate);

                var result = await connection.QueryAsync(
                    "SP_GUARD", param,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"GetViolationsInDateRange error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Gets all violations for a specific student using their StudentNo
        public async Task<List<dynamic>> GetViolationsByStudentId(string studentNo)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETBYSTUDENT");
                param.Add("StudentNo", studentNo);

                var result = await connection.QueryAsync(
                    "SP_GUARD", param,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"GetViolationsByStudentId error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Finds a student by scanning their QR code (StudentNo)
        public async Task<dynamic?> GetStudentByQrCode(string qrCode)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETSTUDENTBYQR");
                param.Add("StudentNo", qrCode);

                var result = await connection.QueryFirstOrDefaultAsync(
                    "SP_GUARD", param,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetStudentByQrCode error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Saves a new violation record to the database
        public async Task RecordViolation(dynamic violation)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "RECORDVIOLATION");
                param.Add("StudentId", violation.StudentId);
                param.Add("ViolationName", violation.Type);
                param.Add("Description", violation.Details);
                param.Add("Severity", violation.Severity);
                param.Add("GuardId", violation.GuardId);

                var result = await connection.QueryAsync(
                    "SP_GUARD", param,
                    commandType: CommandType.StoredProcedure);

                var errorRow = result.FirstOrDefault();
                if (errorRow != null && errorRow.ErrorMessage != null)
                {
                    throw new Exception((string)errorRow.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"RecordViolation error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Gets all registered students
        public async Task<List<dynamic>> GetAllStudents()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETALLSTUDENTS");

                var result = await connection.QueryAsync(
                    "SP_GUARD", param,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"GetAllStudents error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Gets a specific student by their StudentNo
        public async Task<dynamic?> GetStudentByStudentNo(string studentNo)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETSTUDENTBYNO");
                param.Add("StudentNo", studentNo);

                var result = await connection.QueryFirstOrDefaultAsync(
                    "SP_GUARD", param,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetStudentByStudentNo error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

    }

}
