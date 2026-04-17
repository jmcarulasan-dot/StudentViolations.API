using Dapper;
using Microsoft.Data.SqlClient;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System.Data;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace StudentViolations.API.Class
{
    public class NotificationClass : INotificationRepository
    {
        private readonly string _connectionString;

        private static bool _firebaseInitialized = false;

        public NotificationClass(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb");
        }

        private void EnsureFirebaseInitialized()
        {
            if (!_firebaseInitialized)
            {
                try
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "service-account.json");
                    var credential = GoogleCredential.FromFile(path);

                    FirebaseApp.Create(new AppOptions() { Credential = credential });
                    _firebaseInitialized = true;
                    Console.WriteLine("Firebase App Initialized successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Firebase Initialization Error: {ex.Message}");
                }
            }
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

        public async Task SaveFCMToken(string username, string fcmToken)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                DynamicParameters param = new DynamicParameters();
                param.Add("@statementType", "SAVEFCMTOKEN");
                param.Add("@TargetUsername", username);
                param.Add("@FCMToken", fcmToken);

                await connection.ExecuteAsync(
                    "SP_NOTIFICATIONS",
                    param,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving FCM token: {ex.Message}");
            }
            finally { connection.Close(); }
        }

        public async Task SendPushNotification(string targetUsername, string title, string message)
        {
            EnsureFirebaseInitialized();
            Console.WriteLine($"--- Push Notification Request Started for {targetUsername} ---");

            SqlConnection connection = new SqlConnection(_connectionString);
            string token = null;

            try
            {
                await connection.OpenAsync();
                var param = new DynamicParameters();
                param.Add("@statementType", "GETFCMTOKEN");
                param.Add("@TargetUsername", targetUsername);

                token = await connection.QueryFirstOrDefaultAsync<string>(
                    "SP_NOTIFICATIONS", param, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Error getting token: {ex.Message}");
                return;
            }
            finally { connection.Close(); }

            string status = string.IsNullOrEmpty(token) ? "Yes" : "No";
            Console.WriteLine($"Token Retrieved: {status}");
            if (string.IsNullOrEmpty(token)) return; 

            try
            {
                Console.WriteLine("Sending to Firebase...");

                var messagePayload = new Message()
                {
                    Token = token,
                    Notification = new Notification()
                    {
                        Title = title,
                        Body = message
                    },
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            Sound = "default"
                        }
                    }
                };

                await FirebaseMessaging.DefaultInstance.SendAsync(messagePayload);
                Console.WriteLine("✅ Push notification sent!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Firebase Error: {ex.Message}");
            }
        }
    }
}