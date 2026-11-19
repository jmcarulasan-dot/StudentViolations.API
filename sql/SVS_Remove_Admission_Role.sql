/*
   Consolidate the former Admission account role into SAO.
   Existing admission accounts become SAO accounts; student/guard accounts
   and registration, enrollment, and clearance records remain unchanged.
   Run once against the SVS database before testing SAO student services.
*/
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.CK_Users_SVS_Role', N'C') IS NOT NULL
        ALTER TABLE dbo.Users DROP CONSTRAINT CK_Users_SVS_Role;

    DECLARE @AccountsConverted INT;
    UPDATE dbo.Users
    SET Role = N'SAO'
    WHERE UPPER(LTRIM(RTRIM(Role))) = N'ADMISSION';
    SET @AccountsConverted = @@ROWCOUNT;

    ALTER TABLE dbo.Users WITH CHECK
        ADD CONSTRAINT CK_Users_SVS_Role
        CHECK (Role IN (N'Student', N'Guard', N'SAO'));
    ALTER TABLE dbo.Users CHECK CONSTRAINT CK_Users_SVS_Role;

    COMMIT TRANSACTION;

    SELECT @AccountsConverted AS AccountsConvertedToSAO,
           N'Admission role removed; student services are now available to SAO accounts.' AS Result;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
