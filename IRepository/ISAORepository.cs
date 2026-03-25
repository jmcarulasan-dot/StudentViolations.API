namespace StudentViolations.API.IRepository
{
    // Defines all SAO (Student Affairs Office) operations the SAOClass must implement
    public interface ISAORepository
    {
        // Gets every user in the system regardless of role
        Task<List<dynamic>> GetAllUsers();

        // Gets one user by their ID — returns null if not found
        Task<dynamic?> GetUserById(int id);

        // Updates an existing user's information
        Task UpdateUser(dynamic user);

        // Permanently deletes a user from the database
        Task DeleteUser(int id);
    }
}