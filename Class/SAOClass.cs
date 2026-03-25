using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using System.Data;

namespace StudentViolations.API.Class
{
    // Handles all SAO (Student Affairs Office) database operations
    public class SAOClass : ISAORepository
    {
        private readonly string _connectionString;

        public SAOClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        // Gets every user in the system regardless of role
        public async Task<List<dynamic>> GetAllUsers()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETALLUSERS");

                // Call SP_SAO and return the full list of users
                var result = await connection.QueryAsync(
                    "SP_SAO", param,
                    commandType: CommandType.StoredProcedure);

                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"GetAllUsers error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Gets one user by their ID — returns null if not found
        public async Task<dynamic?> GetUserById(int id)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "GETUSERBYID");
                param.Add("StudentID", id);

                // Returns one user record or null if the ID does not exist
                var result = await connection.QueryFirstOrDefaultAsync(
                    "SP_SAO", param,
                    commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"GetUserById error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Updates an existing user's information
        public async Task UpdateUser(dynamic user)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "UPDATEUSER");
                param.Add("StudentID", user.Id);
                param.Add("FirstName", user.FirstName);
                param.Add("LastName", user.LastName);
                param.Add("Email", user.Email);
                param.Add("ContactNumber", user.ContactNumber);
                param.Add("Gender", user.Gender);
                param.Add("Address", user.Address);
                param.Add("Course", user.Course);
                param.Add("Year", user.Year);
                param.Add("Role", user.Role);

                // Execute SP_SAO to update — no return value needed
                await connection.ExecuteAsync(
                    "SP_SAO", param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception($"UpdateUser error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        // Permanently deletes a user from the database by their ID
        public async Task DeleteUser(int id)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();

                DynamicParameters param = new DynamicParameters();
                param.Add("statementType", "DELETEUSER");
                param.Add("StudentID", id);

                // Execute SP_SAO to delete — no return value needed
                await connection.ExecuteAsync(
                    "SP_SAO", param,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                throw new Exception($"DeleteUser error: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }
    }
}