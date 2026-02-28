using Microsoft.EntityFrameworkCore;
using StudentViolations.API.Controllers;
using StudentViolations.API.IRepository;
using StudentViolations.API.Model;
using StudentViolationsAPI.Data; // Make sure to include this
using System;
using System.Threading.Tasks;
using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.Repository
{
    public class LoginRepository : ILoginRepository
    {
        private readonly AppDbContext _context; // Add this

        public LoginRepository(AppDbContext context) // Modify constructor
        {
            _context = context; // Assign the context
        }

        public async Task<ServiceResponse<object>> GetLogin(string username, string password)
        {
            // Your implementation here (e.g., query the database)
            // Replace with your actual data access code
            var loginModel = new LoginModel { Username = username, Password = password }; // Replace with actual data

            // Wrap the result in a ServiceResponse
            var serviceResponse = new ServiceResponse<object>
            {
                Status = 0, // Set the status to 0
                Message = "Login successful",
                Data = loginModel
            };
            return serviceResponse;
        }

        public async Task<bool> UserExists(string username, string email)
        {
            // Check if a user with the given username or email exists in the database
            return await _context.Users.AnyAsync(u => u.Username == username || u.Email == email);
        }

        public async Task<ServiceResponse<object>> RegisterUser(User user)
        {
            var serviceResponse = new ServiceResponse<object>();
            try
            {
                // Add the user to the database
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                //serviceResponse.Success = true; // Removed this line
                serviceResponse.Message = "Registration successful";
                serviceResponse.Data = user; // Return the created user

            }
            catch (Exception ex)
            {
                //serviceResponse.Success = false; // Removed this line
                serviceResponse.Message = $"Registration failed: {ex.Message}";
                serviceResponse.Data = null; // Or any error data
            }
            return serviceResponse;
        }
    }
}