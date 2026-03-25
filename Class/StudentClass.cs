using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using System.Data;

namespace StudentViolations.API.Class
{
    // Handles all student data database operations
    public class StudentClass : IStudentRepository
    {
        private readonly string _connectionString;

        public StudentClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        // Gets one student's data using their StudentNo
        public async Task<dynamic?> GetStudentByStudentId(string studentNo)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETSTUDENT");
                param.Add("StudentNo", studentNo);

                // Returns one student record or null if not found
                var result = await connection.QueryFirstOrDefaultAsync(
                    "SP_STUDENT_DATA", param,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetStudentByStudentId error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Gets all students from the database
        public async Task<List<dynamic>> GetAllStudents()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETALLSTUDENTS");

                // Call SP_STUDENT_DATA and return the full list of students
                var result = await connection.QueryAsync(
                    "SP_STUDENT_DATA", param,
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

        // Updates an existing student's information
        public async Task UpdateStudent(dynamic student)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "UPDATESTUDENT");
                param.Add("StudentID", student.Id);
                param.Add("FirstName", student.FirstName);
                param.Add("LastName", student.LastName);
                param.Add("Email", student.Email);
                param.Add("ContactNumber", student.ContactNumber);
                param.Add("Gender", student.Gender);
                param.Add("Address", student.Address);
                param.Add("Course", student.Course);
                param.Add("Year", student.Year);

                // Execute SP_STUDENT_DATA to update — no return value needed
                await connection.ExecuteAsync(
                    "SP_STUDENT_DATA", param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception($"UpdateStudent error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }
    }
}