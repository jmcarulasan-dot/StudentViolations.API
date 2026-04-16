using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Data;


namespace StudentViolations.API.Class
{
    public class NotificationClass : INotificationRepository
    {
        private readonly string _connectionString;

        public NotificationClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        public async Task<List<NotificationModel>> GetByUserAndRole(string username, string role)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETBYUSER");
                param.Add("@TargetUsername", username);
                param.Add("@TargetRole", role);

                var result = await connection.QueryAsync<NotificationModel>(
                    "SP_NOTIFICATIONS", param, commandType: CommandType.StoredProcedure);
                return result.ToList();
            }
            catch
            {
                return new List<NotificationModel>();
            }
            finally { connection.Close(); }
        }

        public async Task<int> GetUnreadCount(string username, string role)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "GETUNREADCOUNT");
                param.Add("@TargetUsername", username);
                param.Add("@TargetRole", role);

                return await connection.QueryFirstOrDefaultAsync<int>(
                    "SP_NOTIFICATIONS", param, commandType: CommandType.StoredProcedure);
            }
            catch
            {
                return 0;
            }
            finally { connection.Close(); }
        }

        public async Task SendToUser(string targetUsername, string title, string message)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "SENDTOUSER");
                param.Add("@TargetUsername", targetUsername);
                param.Add("@Title", title);
                param.Add("@Message", message);

                await connection.ExecuteAsync(
                    "SP_NOTIFICATIONS", param, commandType: CommandType.StoredProcedure);
            }
            catch { }
            finally { connection.Close(); }
        }

        public async Task SendToRole(string targetRole, string title, string message)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "SENDTOROLE");
                param.Add("@TargetRole", targetRole);
                param.Add("@Title", title);
                param.Add("@Message", message);

                await connection.ExecuteAsync(
                    "SP_NOTIFICATIONS", param, commandType: CommandType.StoredProcedure);
            }
            catch { }
            finally { connection.Close(); }
        }

        public async Task MarkAsRead(int id)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "MARKASREAD");
                param.Add("@Id", id);

                await connection.ExecuteAsync(
                    "SP_NOTIFICATIONS", param, commandType: CommandType.StoredProcedure);
            }
            catch { }
            finally { connection.Close(); }
        }

        public async Task MarkAllAsRead(string username, string role)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "MARKALLREAD");
                param.Add("@TargetUsername", username);
                param.Add("@TargetRole", role);

                await connection.ExecuteAsync(
                    "SP_NOTIFICATIONS", param, commandType: CommandType.StoredProcedure);
            }
            catch { }
            finally { connection.Close(); }
        }
    }
}