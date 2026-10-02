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
