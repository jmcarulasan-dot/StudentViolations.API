using StudentViolationsAPI.Model.Entities;

namespace StudentViolationsAPI.IRepository
{
    public interface ISAORepository
    {
        Task<List<User>> GetAllUsers();
        Task<User?> GetUserById(int id);
        Task UpdateUser(User user);
        Task DeleteUser(int id);
    }
}