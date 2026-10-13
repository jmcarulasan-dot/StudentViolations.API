/*
    SVS sign-in lockout migration
    Apply this script once to the existing StudentViolations database before
    running the API version that enforces sign-in lockouts.
*/
IF COL_LENGTH('dbo.Users', 'FailedSignInAttempts') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD FailedSignInAttempts INT NOT NULL
            CONSTRAINT DF_Users_FailedSignInAttempts DEFAULT (0) WITH VALUES;
END;
GO

IF COL_LENGTH('dbo.Users', 'LockoutEndUtc') IS NULL
BEGIN
    ALTER TABLE dbo.Users
        ADD LockoutEndUtc DATETIME2(3) NULL;
END;
GO
