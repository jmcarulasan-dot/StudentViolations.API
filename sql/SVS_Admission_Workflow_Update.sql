USE [StudentViolations]
GO
/* ============================================================
   ADMISSION WORKFLOW
   Enrollment -> Student App Registration ->a Admission Verification
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
        EnrollmentId INT NULL,
        StudentNo VARCHAR(50) NOT NULL,
        FirstName VARCHAR(50) NULL,
        LastName VARCHAR(50) NULL,
        DateOfBirth DATE NULL,
        Gender VARCHAR(10) NULL,
        Address VARCHAR(100) NULL,
        ContactNumber VARCHAR(20) NULL,
        Email VARCHAR(50) NOT NULL,
        Course VARCHAR(50) NULL,
        Year VARCHAR(20) NULL,
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

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.StudentRegistrationRequests')
      AND name = 'EnrollmentId' AND is_nullable = 0
)
    ALTER TABLE dbo.StudentRegistrationRequests ALTER COLUMN EnrollmentId INT NULL;
GO

IF COL_LENGTH('dbo.StudentRegistrationRequests', 'FirstName') IS NULL
    ALTER TABLE dbo.StudentRegistrationRequests ADD FirstName VARCHAR(50) NULL;
IF COL_LENGTH('dbo.StudentRegistrationRequests', 'LastName') IS NULL
    ALTER TABLE dbo.StudentRegistrationRequests ADD LastName VARCHAR(50) NULL;
IF COL_LENGTH('dbo.StudentRegistrationRequests', 'DateOfBirth') IS NULL
    ALTER TABLE dbo.StudentRegistrationRequests ADD DateOfBirth DATE NULL;
IF COL_LENGTH('dbo.StudentRegistrationRequests', 'Gender') IS NULL
    ALTER TABLE dbo.StudentRegistrationRequests ADD Gender VARCHAR(10) NULL;
IF COL_LENGTH('dbo.StudentRegistrationRequests', 'Address') IS NULL
    ALTER TABLE dbo.StudentRegistrationRequests ADD Address VARCHAR(100) NULL;
IF COL_LENGTH('dbo.StudentRegistrationRequests', 'ContactNumber') IS NULL
    ALTER TABLE dbo.StudentRegistrationRequests ADD ContactNumber VARCHAR(20) NULL;
IF COL_LENGTH('dbo.StudentRegistrationRequests', 'Course') IS NULL
    ALTER TABLE dbo.StudentRegistrationRequests ADD Course VARCHAR(50) NULL;
IF COL_LENGTH('dbo.StudentRegistrationRequests', 'Year') IS NULL
    ALTER TABLE dbo.StudentRegistrationRequests ADD Year VARCHAR(20) NULL;
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
    @Email VARCHAR(50) = NULL,
    @Course VARCHAR(50) = NULL,
    @Year VARCHAR(20) = NULL,
    @Username VARCHAR(50) = NULL,
    @PasswordHash VARCHAR(255) = NULL,
    @Salt VARCHAR(255) = NULL,
    @DateOfBirth DATE = NULL,
    @Gender VARCHAR(10) = NULL,
    @Address VARCHAR(100) = NULL,
    @ContactNumber VARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF (@statementType = 'CREATEENROLLMENT')
        BEGIN
            IF EXISTS (SELECT 1 FROM dbo.StudentEnrollments WHERE UPPER(LTRIM(RTRIM(StudentNo))) = UPPER(LTRIM(RTRIM(@StudentNo))))
               OR EXISTS (SELECT 1 FROM dbo.Students WHERE UPPER(LTRIM(RTRIM(StudentNo))) = UPPER(LTRIM(RTRIM(@StudentNo))))
               OR EXISTS (SELECT 1 FROM dbo.Users WHERE UPPER(LTRIM(RTRIM(StudentNo))) = UPPER(LTRIM(RTRIM(@StudentNo))))
            BEGIN
                SELECT NULL AS StudentNo, 'Student ID is already taken.' AS ErrorMessage;
                RETURN;
            END

            INSERT INTO dbo.StudentEnrollments
            (StudentNo, FirstName, LastName, Email, Course, Year)
            VALUES
            (@StudentNo, @FirstName, @LastName, @Email, @Course, @Year);

            SELECT StudentNo, FirstName, LastName, Email, Course, Year
            FROM dbo.StudentEnrollments
            WHERE EnrollmentId = SCOPE_IDENTITY();
        END

        ELSE IF (@statementType = 'GETENROLLMENTS')
        BEGIN
            SELECT StudentNo, FirstName, LastName, Email, Course, Year
            FROM dbo.StudentEnrollments
            UNION
            SELECT s.StudentNo, s.FirstName, s.LastName, s.Email, s.Course, CAST(s.Year AS VARCHAR(20)) AS Year
            FROM dbo.Students s
            WHERE NOT EXISTS (
                SELECT 1 FROM dbo.StudentEnrollments e
                WHERE UPPER(LTRIM(RTRIM(e.StudentNo))) = UPPER(LTRIM(RTRIM(s.StudentNo)))
            )
            ORDER BY LastName, FirstName;
        END

        ELSE IF (@statementType = 'SUBMITREGISTRATION')
        BEGIN
            IF EXISTS (
                SELECT 1 FROM dbo.Users
                WHERE UPPER(LTRIM(RTRIM(ISNULL(StudentNo, '')))) = UPPER(LTRIM(RTRIM(@StudentNo))))
            )
            BEGIN
                SELECT 0 AS Success, 'An account already exists for this student ID.' AS Message;
                RETURN;
            END

            IF EXISTS (
                SELECT 1 FROM dbo.Users
                WHERE LOWER(LTRIM(RTRIM(Username))) = LOWER(LTRIM(RTRIM(@Username)))
                   OR LOWER(LTRIM(RTRIM(Email))) = LOWER(LTRIM(RTRIM(@Email)))
            )
            BEGIN
                SELECT 0 AS Success, 'Username or email is already registered.' AS Message;
                RETURN;
            END

            IF EXISTS (
                SELECT 1 FROM dbo.StudentRegistrationRequests
                WHERE RequestStatus = 'Pending'
                  AND (
                      UPPER(LTRIM(RTRIM(StudentNo))) = UPPER(LTRIM(RTRIM(@StudentNo)))
                      OR LOWER(LTRIM(RTRIM(Username))) = LOWER(LTRIM(RTRIM(@Username)))
                      OR LOWER(LTRIM(RTRIM(Email))) = LOWER(LTRIM(RTRIM(@Email)))
                  )
            )
            BEGIN
                SELECT 0 AS Success, 'A registration request for this student ID, username, or email is already pending.' AS Message;
                RETURN;
            END

            INSERT INTO dbo.StudentRegistrationRequests
            (EnrollmentId, StudentNo, FirstName, LastName, DateOfBirth, Gender, Address, ContactNumber,
             Email, Course, Year, Username, PasswordHash, Salt)
            VALUES
            (NULL, @StudentNo, @FirstName, @LastName, @DateOfBirth, @Gender, @Address, @ContactNumber,
             @Email, @Course, @Year, @Username, @PasswordHash, @Salt);

            SELECT 1 AS Success, 'Registration request submitted for Admission approval.' AS Message;
        END

        ELSE IF (@statementType = 'GETREGISTRATIONREQUESTS')
        BEGIN
            SELECT
                r.RequestId,
                r.StudentNo,
                COALESCE(NULLIF(r.FirstName, ''), e.FirstName, s.FirstName, '') AS FirstName,
                COALESCE(NULLIF(r.LastName, ''), e.LastName, s.LastName, '') AS LastName,
                r.Email,
                r.Username,
                COALESCE(r.Course, e.Course, s.Course, '') AS Course,
                COALESCE(r.Year, e.Year, CAST(s.Year AS VARCHAR(20)), '') AS Year,
                r.RequestStatus
            FROM dbo.StudentRegistrationRequests r
            LEFT JOIN dbo.StudentEnrollments e ON e.EnrollmentId = r.EnrollmentId
            LEFT JOIN dbo.Students s ON UPPER(LTRIM(RTRIM(s.StudentNo))) = UPPER(LTRIM(RTRIM(r.StudentNo)))
            WHERE r.RequestStatus = 'Pending'
            ORDER BY r.SubmittedAt ASC;
        END

        ELSE IF (@statementType = 'APPROVEREGISTRATION')
        BEGIN
            BEGIN TRANSACTION;

            DECLARE @EnrollmentId2 INT, @StudentNo2 VARCHAR(50), @Email2 VARCHAR(50),
                    @Username2 VARCHAR(50), @PasswordHash2 VARCHAR(255), @Salt2 VARCHAR(255),
                    @FirstName2 VARCHAR(50), @LastName2 VARCHAR(50), @DateOfBirth2 DATE,
                    @Gender2 VARCHAR(10), @Address2 VARCHAR(100), @ContactNumber2 VARCHAR(20),
                    @Course2 VARCHAR(50), @Year2 VARCHAR(20);

            SELECT
                @EnrollmentId2 = EnrollmentId, @StudentNo2 = StudentNo,
                @Email2 = Email, @Username2 = Username,
                @PasswordHash2 = PasswordHash, @Salt2 = Salt,
                @FirstName2 = FirstName, @LastName2 = LastName,
                @DateOfBirth2 = DateOfBirth, @Gender2 = Gender,
                @Address2 = Address, @ContactNumber2 = ContactNumber,
                @Course2 = Course, @Year2 = Year
            FROM dbo.StudentRegistrationRequests
            WHERE RequestId = @RequestId AND RequestStatus = 'Pending';

            IF @StudentNo2 IS NULL
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

            SELECT
                @FirstName2 = COALESCE(NULLIF(s.FirstName, ''), NULLIF(e.FirstName, ''), @FirstName2),
                @LastName2 = COALESCE(NULLIF(s.LastName, ''), NULLIF(e.LastName, ''), @LastName2),
                @DateOfBirth2 = COALESCE(s.DateOfBirth, e.DateOfBirth, @DateOfBirth2),
                @Gender2 = COALESCE(s.Gender, e.Gender, @Gender2),
                @Address2 = COALESCE(s.Address, e.Address, @Address2),
                @ContactNumber2 = COALESCE(s.ContactNumber, e.ContactNumber, @ContactNumber2),
                @Course2 = COALESCE(s.Course, e.Course, @Course2),
                @Year2 = COALESCE(CAST(s.Year AS VARCHAR(20)), e.Year, @Year2)
            FROM (SELECT 1 AS SourceRow) x
            LEFT JOIN dbo.Students s
                ON UPPER(LTRIM(RTRIM(s.StudentNo))) = UPPER(LTRIM(RTRIM(@StudentNo2)))
            LEFT JOIN dbo.StudentEnrollments e ON e.EnrollmentId = @EnrollmentId2;

            INSERT INTO dbo.Users
            (FirstName, LastName, DateOfBirth, Gender, Address, ContactNumber, Email,
             RegistrationDate, Username, PasswordHash, Salt, Role, Course, Year, StudentNo)
            VALUES
            (@FirstName2, @LastName2, @DateOfBirth2, @Gender2, @Address2, @ContactNumber2, @Email2,
             GETDATE(), @Username2, @PasswordHash2, @Salt2, 'Student', @Course2, @Year2, @StudentNo2);

            IF EXISTS (
                SELECT 1 FROM dbo.Students
                WHERE UPPER(LTRIM(RTRIM(StudentNo))) = UPPER(LTRIM(RTRIM(@StudentNo2)))
            )
            BEGIN
                UPDATE dbo.Students
                SET AppRegistered = 1, Email = @Email2
                WHERE UPPER(LTRIM(RTRIM(StudentNo))) = UPPER(LTRIM(RTRIM(@StudentNo2)));
            END
            ELSE
            BEGIN
                INSERT INTO dbo.Students
                (FirstName, LastName, DateOfBirth, Gender, Address, ContactNumber, Email,
                 RegistrationDate, Course, Year, StudentNo, QRCode, Status, AppRegistered)
                VALUES
                (@FirstName2, @LastName2, @DateOfBirth2, @Gender2, @Address2, @ContactNumber2, @Email2,
                 CAST(GETDATE() AS DATE), @Course2, @Year2, @StudentNo2, @StudentNo2, 'Active', 1);
            END

            UPDATE dbo.StudentRegistrationRequests
            SET RequestStatus = 'Approved', ReviewedAt = GETDATE()
            WHERE RequestId = @RequestId;

            IF NOT EXISTS (SELECT 1 FROM dbo.StudentClearances WHERE StudentNo = @StudentNo2)
                INSERT INTO dbo.StudentClearances (StudentNo, SignedByAdmission)
                VALUES (@StudentNo2, 0);

            COMMIT TRANSACTION;

            SELECT TOP 1 StudentID, FirstName, LastName, Email, RegistrationDate,
                   Username, Role, Course, Year, StudentNo
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
