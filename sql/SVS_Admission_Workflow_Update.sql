USE [StudentViolations]
GO
/* ============================================================
   ADMISSION WORKFLOW
   Enrollment -> Student App Registration -> Admission Verification
   -> Account/QR Creation -> Email -> Clearance
   ============================================================ */

USE [StudentViolations]
GO

IF OBJECT_ID('dbo.StudentEnrollments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StudentEnrollments
    (
        EnrollmentId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        StudentNo VARCHAR(50) NOT NULL UNIQUE,
        FirstName VARCHAR(50) NOT NULL,
        LastName VARCHAR(50) NOT NULL,
        DateOfBirth DATE NULL,
        Gender VARCHAR(10) NULL,
        Address VARCHAR(100) NULL,
        ContactNumber VARCHAR(20) NULL,
        Email VARCHAR(50) NULL,
        Course VARCHAR(50) NULL,
        Year VARCHAR(20) NULL,
        EnrollmentStatus VARCHAR(20) NOT NULL DEFAULT 'Active',
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    )
END
GO

IF COL_LENGTH('dbo.Students', 'AppRegistered') IS NULL
BEGIN
    ALTER TABLE dbo.Students
    ADD AppRegistered BIT NOT NULL CONSTRAINT DF_Students_AppRegistered DEFAULT 0
END
GO

IF OBJECT_ID('dbo.StudentRegistrationRequests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StudentRegistrationRequests
    (
        RequestId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EnrollmentId INT NOT NULL,
        StudentNo VARCHAR(50) NOT NULL,
        Email VARCHAR(50) NOT NULL,
        Username VARCHAR(50) NOT NULL,
        PasswordHash VARCHAR(255) NOT NULL,
        Salt VARCHAR(255) NOT NULL,
        RequestStatus VARCHAR(20) NOT NULL DEFAULT 'Pending',
        SubmittedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ReviewedAt DATETIME NULL,
        CONSTRAINT FK_RegistrationRequests_Enrollment
            FOREIGN KEY (EnrollmentId) REFERENCES dbo.StudentEnrollments(EnrollmentId)
    )
END
GO

IF OBJECT_ID('dbo.StudentClearances', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StudentClearances
    (
        ClearanceId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        StudentNo VARCHAR(50) NOT NULL UNIQUE,
        SignedByAdmission BIT NOT NULL DEFAULT 0,
        SignedAt DATETIME NULL
    )
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_ADMISSION
    @statementType VARCHAR(50),
    @EnrollmentId INT = NULL,
    @RequestId INT = NULL,
    @StudentNo VARCHAR(50) = NULL,
    @FirstName VARCHAR(50) = NULL,
    @LastName VARCHAR(50) = NULL,
    @DateOfBirth DATE = NULL,
    @Gender VARCHAR(10) = NULL,
    @Address VARCHAR(100) = NULL,
    @ContactNumber VARCHAR(20) = NULL,
    @Email VARCHAR(50) = NULL,
    @Course VARCHAR(50) = NULL,
    @Year VARCHAR(20) = NULL,
    @Username VARCHAR(50) = NULL,
    @PasswordHash VARCHAR(255) = NULL,
    @Salt VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF (@statementType = 'CREATEENROLLMENT')
        BEGIN
            IF EXISTS (SELECT 1 FROM dbo.StudentEnrollments WHERE StudentNo = @StudentNo)
            BEGIN
                SELECT NULL AS EnrollmentId, @StudentNo AS StudentNo, 'Enrollment already exists.' AS ErrorMessage;
                RETURN;
            END

            INSERT INTO dbo.StudentEnrollments
            (StudentNo, FirstName, LastName, DateOfBirth, Gender, Address, ContactNumber, Email, Course, Year)
            VALUES
            (@StudentNo, @FirstName, @LastName, @DateOfBirth, @Gender, @Address, @ContactNumber, @Email, @Course, @Year);

            SELECT * FROM dbo.StudentEnrollments WHERE EnrollmentId = SCOPE_IDENTITY();
        END

        ELSE IF (@statementType = 'GETENROLLMENTS')
        BEGIN
            SELECT * FROM dbo.StudentEnrollments
            ORDER BY LastName, FirstName;
        END

        ELSE IF (@statementType = 'SUBMITREGISTRATION')
        BEGIN
            DECLARE @MatchedEnrollmentId INT;

            SELECT @MatchedEnrollmentId = EnrollmentId
            FROM dbo.StudentEnrollments
            WHERE StudentNo = @StudentNo
              AND DateOfBirth = @DateOfBirth
              AND LOWER(ISNULL(Email, '')) = LOWER(@Email)
              AND EnrollmentStatus = 'Active';

            IF @MatchedEnrollmentId IS NULL
            BEGIN
                SELECT 0 AS Success, 'The submitted information does not match an active enrollment record.' AS Message;
                RETURN;
            END

            IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username OR Email = @Email)
            BEGIN
                SELECT 0 AS Success, 'Username or email is already registered.' AS Message;
                RETURN;
            END

            IF EXISTS (SELECT 1 FROM dbo.StudentRegistrationRequests
                       WHERE EnrollmentId = @MatchedEnrollmentId AND RequestStatus = 'Pending')
            BEGIN
                SELECT 0 AS Success, 'A registration request is already pending for this student.' AS Message;
                RETURN;
            END

            INSERT INTO dbo.StudentRegistrationRequests
            (EnrollmentId, StudentNo, Email, Username, PasswordHash, Salt)
            VALUES
            (@MatchedEnrollmentId, @StudentNo, @Email, @Username, @PasswordHash, @Salt);

            SELECT 1 AS Success, 'Registration request submitted for Admission verification.' AS Message;
        END

        ELSE IF (@statementType = 'GETREGISTRATIONREQUESTS')
        BEGIN
            SELECT
                r.RequestId,
                r.StudentNo,
                e.FirstName,
                e.LastName,
                r.Email,
                r.Username,
                r.RequestStatus,
                r.SubmittedAt
            FROM dbo.StudentRegistrationRequests r
            INNER JOIN dbo.StudentEnrollments e ON e.EnrollmentId = r.EnrollmentId
            WHERE r.RequestStatus = 'Pending'
            ORDER BY r.SubmittedAt ASC;
        END

        ELSE IF (@statementType = 'APPROVEREGISTRATION')
        BEGIN
            BEGIN TRANSACTION;

            DECLARE @EnrollmentId2 INT, @StudentNo2 VARCHAR(50), @Email2 VARCHAR(50),
                    @Username2 VARCHAR(50), @PasswordHash2 VARCHAR(255), @Salt2 VARCHAR(255);

            SELECT
                @EnrollmentId2 = r.EnrollmentId,
                @StudentNo2 = r.StudentNo,
                @Email2 = r.Email,
                @Username2 = r.Username,
                @PasswordHash2 = r.PasswordHash,
                @Salt2 = r.Salt
            FROM dbo.StudentRegistrationRequests r
            WHERE r.RequestId = @RequestId
              AND r.RequestStatus = 'Pending';

            IF @EnrollmentId2 IS NULL
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT NULL AS StudentID;
                RETURN;
            END

            IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username2 OR Email = @Email2 OR StudentNo = @StudentNo2)
            BEGIN
                ROLLBACK TRANSACTION;
                SELECT NULL AS StudentID;
                RETURN;
            END

            INSERT INTO dbo.Users
            (FirstName, LastName, DateOfBirth, Gender, Address, ContactNumber, Email,
             RegistrationDate, Username, PasswordHash, Salt, Role, Course, Year, StudentNo)
            SELECT FirstName, LastName, DateOfBirth, Gender, Address, ContactNumber, @Email2,
                   GETDATE(), @Username2, @PasswordHash2, @Salt2, 'Student', Course, Year, StudentNo
            FROM dbo.StudentEnrollments
            WHERE EnrollmentId = @EnrollmentId2;

            INSERT INTO dbo.Students
            (FirstName, LastName, DateOfBirth, Gender, Address, ContactNumber, Email,
             RegistrationDate, Course, Year, StudentNo, QRCode, Status, AppRegistered)
            SELECT FirstName, LastName, DateOfBirth, Gender, Address, ContactNumber, @Email2,
                   CAST(GETDATE() AS DATE), Course, Year, StudentNo, StudentNo, 'Active', 1
            FROM dbo.StudentEnrollments
            WHERE EnrollmentId = @EnrollmentId2;

            UPDATE dbo.StudentRegistrationRequests
            SET RequestStatus = 'Approved', ReviewedAt = GETDATE()
            WHERE RequestId = @RequestId;

            IF NOT EXISTS (SELECT 1 FROM dbo.StudentClearances WHERE StudentNo = @StudentNo2)
                INSERT INTO dbo.StudentClearances (StudentNo, SignedByAdmission)
                VALUES (@StudentNo2, 0);

            COMMIT TRANSACTION;

            SELECT TOP 1 *
            FROM dbo.Users
            WHERE StudentNo = @StudentNo2 AND Role = 'Student';
        END

        ELSE IF (@statementType = 'REJECTREGISTRATION')
        BEGIN
            UPDATE dbo.StudentRegistrationRequests
            SET RequestStatus = 'Rejected', ReviewedAt = GETDATE()
            WHERE RequestId = @RequestId
              AND RequestStatus = 'Pending';

            SELECT
                CASE WHEN @@ROWCOUNT = 1 THEN 1 ELSE 0 END AS Success,
                CASE WHEN @@ROWCOUNT = 1 THEN 'Registration request rejected.'
                     ELSE 'Registration request not found or already reviewed.' END AS Message;
        END

        ELSE IF (@statementType = 'CANCLEARANCE')
        BEGIN
            SELECT
                CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.Students s
                    WHERE s.StudentNo = @StudentNo
                      AND s.Status = 'Active'
                      AND s.AppRegistered = 1
                ) THEN 1 ELSE 0 END AS CanSign,
                CASE WHEN EXISTS (
                    SELECT 1
                    FROM dbo.Students s
                    WHERE s.StudentNo = @StudentNo
                      AND s.Status = 'Active'
                      AND s.AppRegistered = 1
                ) THEN 'Student has completed SVS registration and may have clearance signed.'
                ELSE 'Clearance cannot be signed until the student completes SVS registration.' END AS Message;
        END

        ELSE IF (@statementType = 'SIGNCLEARANCE')
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM dbo.Students
                WHERE StudentNo = @StudentNo AND Status = 'Active' AND AppRegistered = 1
            )
            BEGIN
                SELECT 0 AS Success, 'Clearance cannot be signed until the student completes SVS registration.' AS Message;
                RETURN;
            END

            IF EXISTS (SELECT 1 FROM dbo.StudentClearances WHERE StudentNo = @StudentNo)
                UPDATE dbo.StudentClearances
                SET SignedByAdmission = 1, SignedAt = GETDATE()
                WHERE StudentNo = @StudentNo;
            ELSE
                INSERT INTO dbo.StudentClearances (StudentNo, SignedByAdmission, SignedAt)
                VALUES (@StudentNo, 1, GETDATE());

            SELECT 1 AS Success, 'Clearance signed successfully.' AS Message;
        END
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT NULL AS StudentID, 0 AS Success, ERROR_MESSAGE() AS Message;
    END CATCH
END
GO
