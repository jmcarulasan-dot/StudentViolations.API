namespace StudentViolations.API.Model
{
    public class AuthenticatorCodeRequest
    {
        public string ChallengeId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class AuthenticationFlow
    {
        public bool RequiresAuthenticatorSetup { get; set; }
        public bool RequiresAuthenticatorCode { get; set; }
        public string? ChallengeId { get; set; }
        public string? AuthenticatorUri { get; set; }
        public string? QrCodeDataUri { get; set; }
        public string? ManualEntryKey { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; }
        public IReadOnlyList<string>? RecoveryCodes { get; set; }
    }

    internal class LoginAccount
    {
        public int StudentID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? StudentNo { get; set; }
        public string? Status { get; set; }
        public bool AuthenticatorEnabled { get; set; }
        public string? AuthenticatorSecretProtected { get; set; }
    }
}
