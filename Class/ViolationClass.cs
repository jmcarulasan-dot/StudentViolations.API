using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using System.Data;

namespace StudentViolations.API.Class
{
    // Handles all violation-related database operations
    public class ViolationClass : IViolationRepository
    {
        private readonly string _connectionString;

        public ViolationClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
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

                // Execute SP_VIOLATION to insert — no return value needed
                await connection.ExecuteAsync(
                    "SP_VIOLATION", param,
                    commandType: CommandType.StoredProcedure);
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

                // Call SP_VIOLATION and return all violations for this student
                var result = await connection.QueryAsync(
                    "SP_VIOLATION", param,
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

        // Gets every violation in the database
        public async Task<List<dynamic>> GetAllViolations()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETALL");

                // Call SP_VIOLATION and return the full violations list
                var result = await connection.QueryAsync(
                    "SP_VIOLATION", param,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"GetAllViolations error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Gets one violation by its ID — returns null if not found
        public async Task<dynamic?> GetViolationById(int id)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETBYID");
                param.Add("ViolationID", id);

                // Returns one violation record or null if the ID does not exist
                var result = await connection.QueryFirstOrDefaultAsync(
                    "SP_VIOLATION", param,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetViolationById error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Updates the status of a violation — Pending, Approved, or Rejected
        public async Task UpdateViolationStatus(int id, string status)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "UPDATESTATUS");
                param.Add("ViolationID", id);
                param.Add("Status", status);

                // Execute SP_VIOLATION to update the status — no return value needed
                await connection.ExecuteAsync(
                    "SP_VIOLATION", param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception($"UpdateViolationStatus error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Permanently deletes a violation from the database by its ID
        public async Task DeleteViolation(int id)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "DELETE");
                param.Add("ViolationID", id);

                // Execute SP_VIOLATION to delete — no return value needed
                await connection.ExecuteAsync(
                    "SP_VIOLATION", param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception($"DeleteViolation error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }
    }
}