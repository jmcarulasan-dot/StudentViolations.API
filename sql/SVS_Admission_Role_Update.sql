USE [StudentViolations];
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_ADMISSION]
    @statementType VARCHAR(50),
    @StudentID INT = NULL,
    @StudentNo VARCHAR(50) = NULL,
    @FirstName VARCHAR(100) = NULL,
    @LastName VARCHAR(100) = NULL,
    @Email VARCHAR(100) = NULL,
    @ContactNumber VARCHAR(20) = NULL,
    @Gender VARCHAR(20) = NULL,
    @Address VARCHAR(255) = NULL,
    @Role VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF (@statementType = 'GETALLUSERS')
        BEGIN
            SELECT
                u.*,
                s.ProfilePhoto
            FROM dbo.Users u
            LEFT JOIN dbo.Students s ON u.StudentNo = s.StudentNo
            ORDER BY u.Role, u.LastName;
        END
        ELSE IF (@statementType = 'GETUSERBYID')
        BEGIN
            SELECT
                u.*,
                s.ProfilePhoto
            FROM dbo.Users u
            LEFT JOIN dbo.Students s ON u.StudentNo = s.StudentNo
            WHERE u.StudentID = @StudentID;
        END
        ELSE IF (@statementType = 'UPDATEUSER')
        BEGIN
            UPDATE dbo.Users
            SET FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email,
                ContactNumber = @ContactNumber,
                Gender = @Gender,
                Address = @Address,
                Role = @Role
            WHERE StudentID = @StudentID;
        END
        ELSE IF (@statementType = 'DELETEUSER')
        BEGIN
            DELETE FROM dbo.Students
            WHERE StudentNo = (
                SELECT StudentNo
                FROM dbo.Users
                WHERE StudentID = @StudentID
            );

            DELETE FROM dbo.Users
            WHERE StudentID = @StudentID;
        END
        ELSE IF (@statementType = 'GETSTUDENTQR')
        BEGIN
            SELECT QRCode
            FROM dbo.Students
            WHERE StudentNo = @StudentNo;
        END
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END;
GO
