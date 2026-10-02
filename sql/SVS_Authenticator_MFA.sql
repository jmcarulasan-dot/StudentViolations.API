/* Apply once to the existing StudentViolations database before deploying the MFA-enabled API. */
IF COL_LENGTH('dbo.Users', 'AuthenticatorSecretProtected') IS NULL
    ALTER TABLE dbo.Users ADD AuthenticatorSecretProtected NVARCHAR(1000) NULL;
GO

IF COL_LENGTH('dbo.Users', 'AuthenticatorEnabled') IS NULL
    ALTER TABLE dbo.Users ADD AuthenticatorEnabled BIT NOT NULL
        CONSTRAINT DF_Users_AuthenticatorEnabled DEFAULT (0) WITH VALUES;
GO

IF OBJECT_ID('dbo.AuthenticatorLoginChallenges', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuthenticatorLoginChallenges
    (
        ChallengeHash CHAR(64) NOT NULL CONSTRAINT PK_AuthenticatorLoginChallenges PRIMARY KEY,
        UserID INT NOT NULL,
        Purpose VARCHAR(10) NOT NULL,
        ExpiresAtUtc DATETIME2(3) NOT NULL,
        FailedAttempts TINYINT NOT NULL CONSTRAINT DF_AuthenticatorLoginChallenges_FailedAttempts DEFAULT (0),
        IsUsed BIT NOT NULL CONSTRAINT DF_AuthenticatorLoginChallenges_IsUsed DEFAULT (0),
        CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_AuthenticatorLoginChallenges_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT CK_AuthenticatorLoginChallenges_Purpose CHECK (Purpose IN ('Setup', 'Login')),
        CONSTRAINT CK_AuthenticatorLoginChallenges_FailedAttempts CHECK (FailedAttempts <= 5),
        CONSTRAINT FK_AuthenticatorLoginChallenges_Users FOREIGN KEY (UserID)
            REFERENCES dbo.Users(StudentID) ON DELETE CASCADE
    );
    CREATE INDEX IX_AuthenticatorLoginChallenges_UserID
        ON dbo.AuthenticatorLoginChallenges(UserID, Purpose, IsUsed, ExpiresAtUtc);
END;
GO

IF OBJECT_ID('dbo.AuthenticatorRecoveryCodes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuthenticatorRecoveryCodes
    (
        RecoveryCodeID BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuthenticatorRecoveryCodes PRIMARY KEY,
        UserID INT NOT NULL,
        CodeHash CHAR(64) NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_AuthenticatorRecoveryCodes_CreatedAtUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_AuthenticatorRecoveryCodes_User_Code UNIQUE (UserID, CodeHash),
        CONSTRAINT FK_AuthenticatorRecoveryCodes_Users FOREIGN KEY (UserID)
            REFERENCES dbo.Users(StudentID) ON DELETE CASCADE
    );
    CREATE INDEX IX_AuthenticatorRecoveryCodes_UserID
        ON dbo.AuthenticatorRecoveryCodes(UserID);
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_AUTHENTICATOR_MFA
    @statementType VARCHAR(40),
    @Username NVARCHAR(256) = NULL,
    @UserID INT = NULL,
    @Secret NVARCHAR(1000) = NULL,
    @ChallengeHash CHAR(64) = NULL,
    @Purpose VARCHAR(10) = NULL,
    @CodeHash CHAR(64) = NULL,
    @FailedAttempts TINYINT = NULL,
    @StudentNo NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @statementType = 'GETACCOUNT'
    BEGIN
        SELECT u.StudentID, u.Username, u.PasswordHash, u.Salt, u.FirstName, u.LastName,
               u.Role, u.StudentNo, ISNULL(s.Status, 'Active') AS Status,
               ISNULL(u.AuthenticatorEnabled, 0) AS AuthenticatorEnabled,
               u.AuthenticatorSecretProtected
        FROM dbo.Users u
        LEFT JOIN dbo.Students s ON u.StudentNo = s.StudentNo
        WHERE u.Username = @Username;
        RETURN;
    END;

    IF @statementType = 'SETUPSECRET'
    BEGIN
        UPDATE dbo.Users
        SET AuthenticatorSecretProtected = @Secret
        WHERE StudentID = @UserID AND AuthenticatorEnabled = 0;
        RETURN;
    END;

    IF @statementType = 'CREATE_CHALLENGE'
    BEGIN
        UPDATE dbo.AuthenticatorLoginChallenges
        SET IsUsed = 1
        WHERE UserID = @UserID AND Purpose = @Purpose AND IsUsed = 0;

        INSERT INTO dbo.AuthenticatorLoginChallenges
            (ChallengeHash, UserID, Purpose, ExpiresAtUtc, FailedAttempts, IsUsed, CreatedAtUtc)
        VALUES
            (@ChallengeHash, @UserID, @Purpose, DATEADD(MINUTE, 5, SYSUTCDATETIME()), 0, 0, SYSUTCDATETIME());
        RETURN;
    END;

    IF @statementType = 'GETCHALLENGE'
    BEGIN
        SELECT c.UserID, c.Purpose, c.ExpiresAtUtc, c.FailedAttempts, c.IsUsed,
               u.StudentID, u.Username, u.FirstName, u.LastName, u.Role, u.StudentNo,
               u.AuthenticatorEnabled, u.AuthenticatorSecretProtected
        FROM dbo.AuthenticatorLoginChallenges c WITH (UPDLOCK, ROWLOCK)
        INNER JOIN dbo.Users u ON u.StudentID = c.UserID
        WHERE c.ChallengeHash = @ChallengeHash;
        RETURN;
    END;

    IF @statementType = 'USE_RECOVERY_CODE'
    BEGIN
        DELETE FROM dbo.AuthenticatorRecoveryCodes
        WHERE UserID = @UserID AND CodeHash = @CodeHash;
        RETURN;
    END;

    IF @statementType = 'RECORD_FAILURE'
    BEGIN
        UPDATE dbo.AuthenticatorLoginChallenges
        SET FailedAttempts = @FailedAttempts,
            IsUsed = CASE WHEN @FailedAttempts >= 5 THEN 1 ELSE IsUsed END
        WHERE ChallengeHash = @ChallengeHash;
        RETURN;
    END;

    IF @statementType = 'ENABLE_AUTHENTICATOR'
    BEGIN
        UPDATE dbo.Users
        SET AuthenticatorEnabled = 1
        WHERE StudentID = @UserID AND AuthenticatorEnabled = 0;
        RETURN;
    END;

    IF @statementType = 'SAVE_RECOVERY_CODE'
    BEGIN
        INSERT INTO dbo.AuthenticatorRecoveryCodes (UserID, CodeHash)
        VALUES (@UserID, @CodeHash);
        RETURN;
    END;

    IF @statementType = 'COMPLETE_CHALLENGE'
    BEGIN
        UPDATE dbo.AuthenticatorLoginChallenges
        SET IsUsed = 1
        WHERE ChallengeHash = @ChallengeHash;
        RETURN;
    END;

    IF @statementType = 'MARK_APP_REGISTERED'
    BEGIN
        UPDATE dbo.Students
        SET AppRegistered = 1
        WHERE StudentNo = @StudentNo;
        RETURN;
    END;

    THROW 50002, 'Unsupported SP_AUTHENTICATOR_MFA statement type.', 1;
END;
GO
