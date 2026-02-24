using StudentViolations.API.IRepository;

namespace StudentViolations.API.Class
{
    public class LoginClass : ILoginRepository
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection conn;

        public LoginClass(IConfiguration configuration)
        {
            _configuration = configuration;
            conn = new SqlConnection(configuration["ConnectionString:StudentViolationsdb"]);
        }

        public async Task<ServiceResponse<object>> GetLogin(string username, string password)
        {
            var service = new ServiceResponse<object>();
            try
            {
                var param = new DynamicParameters();
                param.Add("@Username", username);
                param.Add("@Password", password);

                var result = conn.Query("SP_STUDENT_GETUSERLOGIN", param, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex) { }


            return service;

            
        }
    }
}
