namespace StudentViolations.API.Model
{
    // Model for the POST /login endpoint
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}