namespace StudentViolations.API.IRepository
{
    public interface IEmailRepository
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
