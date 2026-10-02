using Dapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using StudentViolations.API.Model;
using StudentViolations.API.Model.Response;
using StudentViolations.API.IRepository;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace StudentViolations.API.Class
{
    public class LoginClass : ILoginRepository
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly AuthenticatorService _authenticator;

        public LoginClass(IConfiguration configuration, AuthenticatorService authenticator)
        {
            _connectionString = configuration.GetConnectionString("StudentViolationsdb")
                ?? throw new InvalidOperationException("StudentViolationsdb connection string is missing.");
            _configuration = configuration;
            _authenticator = authenticator;
        }

        public async Task<ServiceResponse<AuthenticationFlow>> Authenticate(string username, string password)
        {
            await using var connection = new SqlConnection(_connectionString);
            try
            {
                await connection.OpenAsync();
                var account = await connection.QueryFirstOrDefaultAsync<LoginAccount>(@"
                    SELECT u.StudentID, u.Username, u.PasswordHash, u.Salt, u.FirstName, u.LastName,
                           u.Role, u.StudentNo, ISNULL(s.Status, 'Active') AS Status,
                           ISNULL(u.AuthenticatorEnabled, 0) AS AuthenticatorEnabled,
                           u.AuthenticatorSecretProtected
                    FROM dbo.Users u
                    LEFT JOIN dbo.Students s ON u.StudentNo = s.StudentNo
                    WHERE u.Username = @Username;", new { Username = username });

                if (account == null || !PasswordMatches(password, account.Salt, account.PasswordHash))
                    return Failure(401, "Invalid username or password.");
                if (string.Equals(account.Status, "Dismissed", StringComparison.OrdinalIgnoreCase))
                    return Failure(403, "Your account has been dismissed. Please contact the SAO office.");

                var setup = !account.AuthenticatorEnabled;
                string? provisioningUri = null;
                string? manualKey = null;
                if (setup)
                {
                    // Replace any unconfirmed enrollment with a fresh secret on a new password login.
                    manualKey = _authenticator.GenerateSecret();
                    account.AuthenticatorSecretProtected = _authenticator.Protect(manualKey);
                    await connection.ExecuteAsync(@"
                        UPDATE dbo.Users SET AuthenticatorSecretProtected = @Secret
                        WHERE StudentID = @UserId AND AuthenticatorEnabled = 0;",
                        new { Secret = account.AuthenticatorSecretProtected, UserId = account.StudentID });
                    var issuer = _configuration["Authenticator:Issuer"] ?? "ACLC College of Mandaue SVS";
                    provisioningUri = _authenticator.CreateProvisioningUri(account.Username, manualKey, issuer);
                }

                var challengeId = await CreateChallenge(connection, account.StudentID, setup ? "Setup" : "Login");
                return new ServiceResponse<AuthenticationFlow>
                {
                    Status = 200,
                    Message = setup ? "Scan the QR code and verify a code to finish authenticator setup." : "Enter your authenticator code to finish signing in.",
                    Data = new AuthenticationFlow
                    {
                        RequiresAuthenticatorSetup = setup,
                        RequiresAuthenticatorCode = !setup,
                        ChallengeId = challengeId,
                        AuthenticatorUri = provisioningUri,
                        QrCodeDataUri = provisioningUri == null ? null : "data:image/png;base64," + Convert.ToBase64String(_authenticator.CreateQrPng(provisioningUri)),
                        ManualEntryKey = manualKey
                    }
                };
            }
            catch (Exception ex)
            {
                return Failure(500, $"Login error: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<AuthenticationFlow>> VerifyAuthenticatorCode(string challengeId, string code)
        {
            if (string.IsNullOrWhiteSpace(challengeId) || string.IsNullOrWhiteSpace(code))
                return Failure(400, "Challenge ID and authenticator code are required.");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            try
            {
                var challenge = await connection.QueryFirstOrDefaultAsync<ChallengeAccount>(@"
                    SELECT c.UserID, c.Purpose, c.ExpiresAtUtc, c.FailedAttempts, c.IsUsed,
                           u.StudentID, u.Username, u.FirstName, u.LastName, u.Role, u.StudentNo,
                           u.AuthenticatorEnabled, u.AuthenticatorSecretProtected
                    FROM dbo.AuthenticatorLoginChallenges c WITH (UPDLOCK, ROWLOCK)
                    INNER JOIN dbo.Users u ON u.StudentID = c.UserID
                    WHERE c.ChallengeHash = @Hash;",
                    new { Hash = HashChallenge(challengeId) }, transaction);

                if (challenge == null || challenge.IsUsed || challenge.ExpiresAtUtc <= DateTime.UtcNow || challenge.FailedAttempts >= 5)
                {
                    await transaction.RollbackAsync();
                    return Failure(401, "This sign-in challenge is invalid or expired. Sign in again.");
                }

                var isSetup = challenge.Purpose == "Setup";
                if (isSetup && challenge.AuthenticatorEnabled || !isSetup && !challenge.AuthenticatorEnabled ||
                    string.IsNullOrWhiteSpace(challenge.AuthenticatorSecretProtected))
                {
                    await transaction.RollbackAsync();
                    return Failure(401, "This sign-in challenge is no longer valid. Sign in again.");
                }

                var secret = _authenticator.Unprotect(challenge.AuthenticatorSecretProtected);
                var valid = _authenticator.VerifyCode(secret, code);
                var recoveryCode = false;
                if (!isSetup && !valid)
                {
                    var normalizedCode = AuthenticatorService.NormalizeRecoveryCode(code);
                    if (normalizedCode.Length >= 8)
                    {
                        var deleted = await connection.ExecuteAsync(@"
                            DELETE FROM dbo.AuthenticatorRecoveryCodes
                            WHERE UserID = @UserId AND CodeHash = @CodeHash;",
                            new { UserId = challenge.StudentID, CodeHash = AuthenticatorService.HashRecoveryCode(normalizedCode) }, transaction);
                        recoveryCode = deleted == 1;
                        valid = recoveryCode;
                    }
                }

                if (!valid)
                {
                    var attempts = challenge.FailedAttempts + 1;
                    await connection.ExecuteAsync(@"
                        UPDATE dbo.AuthenticatorLoginChallenges
                        SET FailedAttempts = @Attempts, IsUsed = CASE WHEN @Attempts >= 5 THEN 1 ELSE IsUsed END
                        WHERE ChallengeHash = @Hash;",
                        new { Attempts = attempts, Hash = HashChallenge(challengeId) }, transaction);
                    await transaction.CommitAsync();
                    return Failure(401, attempts >= 5 ? "Too many incorrect codes. Sign in again." : "Incorrect authenticator code.");
                }

                IReadOnlyList<string>? recoveryCodes = null;
                if (isSetup)
                {
                    recoveryCodes = AuthenticatorService.GenerateRecoveryCodes();
                    await connection.ExecuteAsync(@"
                        UPDATE dbo.Users SET AuthenticatorEnabled = 1
                        WHERE StudentID = @UserId AND AuthenticatorEnabled = 0;",
                        new { UserId = challenge.StudentID }, transaction);
                    foreach (var recovery in recoveryCodes)
                    {
                        await connection.ExecuteAsync(@"
                            INSERT INTO dbo.AuthenticatorRecoveryCodes (UserID, CodeHash)
                            VALUES (@UserId, @CodeHash);",
                            new { UserId = challenge.StudentID, CodeHash = AuthenticatorService.HashRecoveryCode(recovery) }, transaction);
                    }
                }

                await connection.ExecuteAsync(@"
                    UPDATE dbo.AuthenticatorLoginChallenges SET IsUsed = 1
                    WHERE ChallengeHash = @Hash;",
                    new { Hash = HashChallenge(challengeId) }, transaction);

                if (string.Equals(challenge.Role, "Student", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(challenge.StudentNo))
                {
                    await connection.ExecuteAsync(
                        "UPDATE dbo.Students SET AppRegistered = 1 WHERE StudentNo = @StudentNo;",
                        new { challenge.StudentNo }, transaction);
                }

                await transaction.CommitAsync();
                return new ServiceResponse<AuthenticationFlow>
                {
                    Status = 200,
                    Message = recoveryCode ? "Login successful using a recovery code." : "Login successful.",
                    Data = new AuthenticationFlow
                    {
                        Role = challenge.Role,
                        Token = GenerateToken(new LoginAccount
                        {
                            StudentID = challenge.StudentID,
                            Username = challenge.Username,
                            FirstName = challenge.FirstName,
                            LastName = challenge.LastName,
                            Role = challenge.Role,
                            StudentNo = challenge.StudentNo
                        }),
                        RecoveryCodes = recoveryCodes
                    }
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Failure(500, $"Authenticator verification error: {ex.Message}");
            }
        }

        public async Task<ServiceResponse<bool>> UserExists(string username, string email)
        {
            await using var connection = new SqlConnection(_connectionString);
            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<int>(
                    "SP_STUDENT_GETUSERLOGIN",
                    new { username, email, statementType = "USEREXISTS" },
                    commandType: CommandType.StoredProcedure);
                return new ServiceResponse<bool> { Status = 200, Data = result > 0 };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool> { Status = 500, Message = $"UserExists error: {ex.Message}" };
            }
        }

        private async Task<string> CreateChallenge(SqlConnection connection, int userId, string purpose)
        {
            var challengeId = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
            await connection.ExecuteAsync(@"
                UPDATE dbo.AuthenticatorLoginChallenges
                SET IsUsed = 1
                WHERE UserID = @UserId AND Purpose = @Purpose AND IsUsed = 0;
                INSERT INTO dbo.AuthenticatorLoginChallenges
                    (ChallengeHash, UserID, Purpose, ExpiresAtUtc, FailedAttempts, IsUsed, CreatedAtUtc)
                VALUES (@Hash, @UserId, @Purpose, DATEADD(MINUTE, 5, SYSUTCDATETIME()), 0, 0, SYSUTCDATETIME());",
                new { Hash = HashChallenge(challengeId), UserId = userId, Purpose = purpose });
            return challengeId;
        }

        private static string HashChallenge(string challengeId) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(challengeId)));

        private static bool PasswordMatches(string password, string salt, string expectedHash)
        {
            try
            {
                var saltBytes = Convert.FromBase64String(salt);
                var derived = KeyDerivation.Pbkdf2(password, saltBytes, KeyDerivationPrf.HMACSHA256, 10000, 32);
                var expected = Convert.FromBase64String(expectedHash);
                return CryptographicOperations.FixedTimeEquals(derived, expected);
            }
            catch (FormatException) { return false; }
        }

        private string GenerateToken(LoginAccount user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.StudentID.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.Trim().ToUpperInvariant()),
                new Claim("name", $"{user.FirstName} {user.LastName}".Trim()),
                new Claim("studentNo", user.StudentNo ?? string.Empty)
            };
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"], audience: jwtSettings["Audience"], claims: claims,
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(jwtSettings["ExpiryInHours"])),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static ServiceResponse<AuthenticationFlow> Failure(int status, string message) =>
            new() { Status = status, Message = message };

        private sealed class ChallengeAccount
        {
            public int UserID { get; set; }
            public string Purpose { get; set; } = string.Empty;
            public DateTime ExpiresAtUtc { get; set; }
            public int FailedAttempts { get; set; }
            public bool IsUsed { get; set; }
            public int StudentID { get; set; }
            public string Username { get; set; } = string.Empty;
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string Role { get; set; } = string.Empty;
            public string? StudentNo { get; set; }
            public bool AuthenticatorEnabled { get; set; }
            public string? AuthenticatorSecretProtected { get; set; }
        }
    }
}
