namespace StudentViolations.API.IRepository
{
    // Defines all SAO (Student Affairs Office) operations the SAOClass must implement
    public interface ISAORepository
    {
        Task<List<dynamic>> GetAllUsers();
        Task<dynamic?> GetUserById(int id);
        Task UpdateUser(dynamic user);
        Task DeleteUser(int id);
    }
}