/*
   Recovery for an account whose saved authenticator secret cannot be decrypted
   because the Data Protection key that encrypted it is missing.

   Replace the username below with the exact account username, review the
   preview result, then execute. The account must sign in with its password
   and enroll a new authenticator after this reset.
*/
DECLARE @Username NVARCHAR(100) = N'REPLACE_WITH_EXACT_USERNAME';
DECLARE @UserID INT;

SELECT @UserID = StudentID
FROM dbo.Users
WHERE Username = @Username;

IF @UserID IS NULL
    THROW 50001, 'No user matched the supplied username. Nothing was changed.', 1;

BEGIN TRANSACTION;

UPDATE dbo.Users
SET AuthenticatorEnabled = 0,
    AuthenticatorSecretProtected = NULL
WHERE StudentID = @UserID;

IF @@ROWCOUNT <> 1
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50002, 'Expected exactly one account to reset. Changes were rolled back.', 1;
END;

DELETE FROM dbo.AuthenticatorRecoveryCodes WHERE UserID = @UserID;
DELETE FROM dbo.AuthenticatorLoginChallenges WHERE UserID = @UserID;

COMMIT TRANSACTION;

SELECT Username, AuthenticatorEnabled
FROM dbo.Users
WHERE StudentID = @UserID;
