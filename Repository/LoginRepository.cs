using Microsoft.EntityFrameworkCore;
using StudentViolations.API.Controllers;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolationsAPI.Data;
using System;
using System.Threading.Tasks;
using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly AppDbContext _context; 

        public LoginRepository(AppDbContext context) 
        {
            _context = context; 
        }

        public async Task<ServiceResponse<object>> GetLogin(string username, string password)
        {
       
            var loginModel = new LoginModel { Username = username, Password = password };

            var serviceResponse = new ServiceResponse<object>
            {
                Status = 0, 
                Message = "Login successful",
                Data = loginModel
            };
            return serviceResponse;
        }

        public async Task<bool> UserExists(string username, string email)
        {
            return await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
        }

        public async Task<ServiceResponse<object>> RegisterUser(User user)
        {
            var serviceResponse = new ServiceResponse<object>();
            try
            {
              
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                serviceResponse.Message = "Registration successful";
                serviceResponse.Data = user; 
            }
            catch (Exception ex)
            {
                serviceResponse.Message = $"Registration failed: {ex.Message}";
                serviceResponse.Data = null; 
            }
            return serviceResponse;
        }
    }
}