using Dapper;
using Microsoft.Extensions.Configuration;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using System;
using System.Data;
using System.Data.SqlClient;


namespace StudentViolations.API.Class
{
    public class LoginClass : ILoginRepository
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection con;
        public LoginClass(IConfiguration configuration)
        {
            _configuration = configuration;
            con = new SqlConnection(configuration["ConnectionStrings:StudentViolationsdb"]);
        }


        public async Task<ServiceResponse<object>> GetLogin(string username, string password)
        {
            var service  = new ServiceResponse<object>();

            try
            {
                var param = new DynamicParameters();
                param.Add("username", username);
                param.Add("password", password);
                param.Add("firstName", "");
                param.Add("statementType", "Update");
               

                var result = con.Query("SP_STUDENT_GETUSERLOGIN", param, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                service.Message = ex.Message;

                return service;
            }

        }
    }
}