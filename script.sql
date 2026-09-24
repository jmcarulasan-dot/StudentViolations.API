USE [master]
GO
/****** Object:  Database [StudentViolations]    Script Date: 9/23/2026 7:45:13 PM ******/
CREATE DATABASE [StudentViolations]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'student_violations', FILENAME = N'C:\Users\jessa\student_violations.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'student_violations_log', FILENAME = N'C:\Users\jessa\student_violations_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [StudentViolations] SET COMPATIBILITY_LEVEL = 170
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [StudentViolations].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [StudentViolations] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [StudentViolations] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [StudentViolations] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [StudentViolations] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [StudentViolations] SET ARITHABORT OFF 
GO
ALTER DATABASE [StudentViolations] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [StudentViolations] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [StudentViolations] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [StudentViolations] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [StudentViolations] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [StudentViolations] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [StudentViolations] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [StudentViolations] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [StudentViolations] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [StudentViolations] SET  DISABLE_BROKER 
GO
ALTER DATABASE [StudentViolations] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [StudentViolations] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [StudentViolations] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [StudentViolations] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [StudentViolations] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [StudentViolations] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [StudentViolations] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [StudentViolations] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [StudentViolations] SET  MULTI_USER 
GO
ALTER DATABASE [StudentViolations] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [StudentViolations] SET DB_CHAINING OFF 
GO
ALTER DATABASE [StudentViolations] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [StudentViolations] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [StudentViolations] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [StudentViolations] SET OPTIMIZED_LOCKING = OFF 
GO
ALTER DATABASE [StudentViolations] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [StudentViolations] SET QUERY_STORE = ON
GO
ALTER DATABASE [StudentViolations] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [StudentViolations]
GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TargetUsername] [nvarchar](100) NULL,
	[TargetRole] [nvarchar](50) NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[IsRead] [bit] NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PasswordResetTokens]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PasswordResetTokens](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Token] [varchar](200) NOT NULL,
	[ExpiresAt] [datetime] NOT NULL,
	[IsUsed] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Students]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Students](
	[StudentID] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [varchar](50) NOT NULL,
	[LastName] [varchar](50) NOT NULL,
	[DateOfBirth] [date] NULL,
	[Gender] [varchar](10) NULL,
	[Address] [varchar](100) NULL,
	[ContactNumber] [varchar](20) NULL,
	[Email] [varchar](50) NULL,
	[RegistrationDate] [date] NULL,
	[Course] [varchar](50) NULL,
	[Year] [varchar](20) NULL,
	[StudentNo] [varchar](50) NULL,
	[QRCode] [varchar](max) NULL,
	[Status] [varchar](20) NOT NULL,
	[ProfilePhoto] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[StudentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserFCMTokens]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserFCMTokens](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[FCMToken] [nvarchar](max) NOT NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_UserFCMTokens_Username] UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[StudentID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](50) NOT NULL,
	[PasswordHash] [varchar](256) NOT NULL,
	[Salt] [varchar](100) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[FirstName] [varchar](50) NULL,
	[LastName] [varchar](50) NULL,
	[DateOfBirth] [date] NULL,
	[Gender] [varchar](10) NULL,
	[Address] [varchar](100) NULL,
	[ContactNumber] [varchar](20) NULL,
	[RegistrationDate] [date] NULL,
	[Role] [varchar](50) NOT NULL,
	[Course] [varchar](50) NULL,
	[Year] [varchar](20) NULL,
	[StudentNo] [varchar](50) NULL,
 CONSTRAINT [PK_Student_Login] PRIMARY KEY CLUSTERED 
(
	[StudentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Violations]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Violations](
	[ViolationID] [int] IDENTITY(1,1) NOT NULL,
	[StudentId] [int] NULL,
	[ViolationName] [varchar](255) NOT NULL,
	[Description] [varchar](max) NULL,
	[Severity] [varchar](50) NULL,
	[GuardId] [varchar](255) NULL,
	[ViolationDate] [datetime] NULL,
	[Status] [varchar](20) NOT NULL,
	[AppealText] [varchar](max) NULL,
	[AppealStatus] [varchar](20) NOT NULL,
	[AppealRemarks] [varchar](max) NULL,
	[IsArchived] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ViolationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_PasswordResetTokens_Token]    Script Date: 9/23/2026 7:45:13 PM ******/
CREATE NONCLUSTERED INDEX [IX_PasswordResetTokens_Token] ON [dbo].[PasswordResetTokens]
(
	[Token] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Student_Login_Email]    Script Date: 9/23/2026 7:45:13 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Student_Login_Email] ON [dbo].[Users]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Student_Login_Username]    Script Date: 9/23/2026 7:45:13 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Student_Login_Username] ON [dbo].[Users]
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[PasswordResetTokens] ADD  DEFAULT ((0)) FOR [IsUsed]
GO
ALTER TABLE [dbo].[PasswordResetTokens] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Students] ADD  DEFAULT ('Active') FOR [Status]
GO
ALTER TABLE [dbo].[UserFCMTokens] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ('Guard') FOR [Role]
GO
ALTER TABLE [dbo].[Users] WITH CHECK ADD CONSTRAINT [CK_Users_SVS_Role] CHECK ([Role] IN ('Admission', 'Student', 'Guard', 'SAO'))
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [CK_Users_SVS_Role]
GO
ALTER TABLE [dbo].[Violations] ADD  DEFAULT (getdate()) FOR [ViolationDate]
GO
ALTER TABLE [dbo].[Violations] ADD  DEFAULT ('Pending') FOR [Status]
GO
ALTER TABLE [dbo].[Violations] ADD  DEFAULT ('None') FOR [AppealStatus]
GO
ALTER TABLE [dbo].[Violations] ADD  DEFAULT ((0)) FOR [IsArchived]
GO
ALTER TABLE [dbo].[PasswordResetTokens]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([StudentID])
GO
ALTER TABLE [dbo].[Violations]  WITH CHECK ADD FOREIGN KEY([StudentId])
REFERENCES [dbo].[Students] ([StudentID])
GO
/****** Object:  StoredProcedure [dbo].[SP_GUARD]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Jeff Carulasan
-- Create date:   03/01/2026
-- Description:   Handles all guard-related database operations.
-- =============================================
CREATE PROCEDURE [dbo].[SP_GUARD]
    @statementType VARCHAR(50),
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @StudentNo VARCHAR(50) = NULL,
    @StudentId INT = NULL,
    @ViolationName VARCHAR(255) = NULL,
    @Description VARCHAR(MAX) = NULL,
    @Severity VARCHAR(50) = NULL,
    @GuardId VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Gets all violations between two dates
        IF (@statementType = 'GETBYDATE')
        BEGIN
            SELECT 
                v.ViolationID,
                v.StudentId,
                v.ViolationName,
                v.Description,
                v.Severity,
                v.ViolationDate,
                v.Status,
                v.GuardId,
                CONCAT(u.FirstName, ' ', u.LastName) AS GuardName,
                s.StudentNo
            FROM dbo.Violations v
            LEFT JOIN dbo.Students s ON v.StudentId = s.StudentID
            LEFT JOIN dbo.Users u ON v.GuardId = CAST(u.StudentID AS VARCHAR)
            WHERE v.ViolationDate >= @StartDate
            AND v.ViolationDate < DATEADD(DAY, 1, @EndDate)
            ORDER BY v.ViolationDate DESC
        END
        -- Gets all violations for a specific student using their StudentNo
        ELSE IF (@statementType = 'GETBYSTUDENT')
        BEGIN
            SELECT 
                v.ViolationID,
                v.ViolationName,
                v.Description,
                v.Severity,
                v.ViolationDate,
                v.Status,
                v.StudentId,
                v.GuardId,
                CONCAT(u.FirstName, ' ', u.LastName) AS GuardName
            FROM dbo.Violations v
            INNER JOIN dbo.Students s ON v.StudentId = s.StudentID
            LEFT JOIN dbo.Users u ON v.GuardId = CAST(u.StudentID AS VARCHAR)
            WHERE s.StudentNo = @StudentNo
            ORDER BY v.ViolationDate DESC
        END
        -- Finds a student by their StudentNo (QR code scan)
        ELSE IF (@statementType = 'GETSTUDENTBYQR')
        BEGIN
            SELECT * FROM dbo.Students
            WHERE StudentNo = @StudentNo
        END
        -- Records a new violation
        ELSE IF (@statementType = 'RECORDVIOLATION')
        BEGIN
            INSERT INTO dbo.Violations
                (StudentId, ViolationName, Description, Severity, GuardId, ViolationDate, Status, AppealStatus)
            VALUES
                (@StudentId, @ViolationName, @Description, @Severity, @GuardId, GETDATE(), 'Pending', 'None')
        END
        -- Gets all registered students
        ELSE IF (@statementType = 'GETALLSTUDENTS')
        BEGIN
            SELECT StudentID, StudentNo, FirstName, LastName, Course, Year, ProfilePhoto
            FROM dbo.Students
            ORDER BY LastName, FirstName
        END
        -- Gets a specific student by their StudentNo
        ELSE IF (@statementType = 'GETSTUDENTBYNO')
        BEGIN
            SELECT StudentID, StudentNo, FirstName, LastName, Course, Year
            FROM dbo.Students
            WHERE StudentNo = @StudentNo
        END
        ELSE IF (@statementType = 'GETUSERNAMEBYNO')
        BEGIN
            SELECT Username FROM dbo.Users
            WHERE StudentNo = @StudentNo AND Role = 'student'
        END
        -- Gets all violations recorded by a specific guard
        ELSE IF (@statementType = 'GETBYGUARD')
        BEGIN
            SELECT 
                v.ViolationID,
                v.ViolationName,
                v.Description,
                v.Severity,
                v.ViolationDate,
                v.Status,
                v.StudentId,
                v.GuardId,
                s.StudentNo,
                CONCAT(s.FirstName, ' ', s.LastName) AS StudentName
            FROM dbo.Violations v
            INNER JOIN dbo.Students s ON v.StudentId = s.StudentID
            WHERE v.GuardId = @GuardId
            ORDER BY v.ViolationDate DESC
        END
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ErrorMessage;
        RETURN;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[SP_NOTIFICATIONS]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SP_NOTIFICATIONS]
    @statementType  VARCHAR(50),
    @Id             INT           = NULL,
    @TargetUsername NVARCHAR(100) = NULL,
    @TargetRole     NVARCHAR(50)  = NULL,
    @Title          NVARCHAR(255) = NULL,
    @Message        NVARCHAR(MAX) = NULL,
    @FCMToken       NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF (@statementType = 'SAVEFCMTOKEN')
        BEGIN
            IF EXISTS (SELECT 1 FROM dbo.UserFCMTokens WHERE Username = @TargetUsername)
                UPDATE dbo.UserFCMTokens 
                SET FCMToken = @FCMToken, UpdatedAt = GETDATE()
                WHERE Username = @TargetUsername;
            ELSE
                INSERT INTO dbo.UserFCMTokens (Username, FCMToken, UpdatedAt)
                VALUES (@TargetUsername, @FCMToken, GETDATE());
        END
        ELSE IF (@statementType = 'GETFCMTOKEN')
        BEGIN
            SELECT FCMToken 
            FROM dbo.UserFCMTokens 
            WHERE Username = @TargetUsername;
        END
        ELSE IF (@statementType = 'GETBYUSER')
        BEGIN
            SELECT Id, TargetUsername, TargetRole, Title, Message, IsRead, CreatedAt
            FROM dbo.Notifications
            WHERE (TargetUsername = @TargetUsername)
            OR (TargetRole = @TargetRole AND TargetUsername IS NULL)
            ORDER BY CreatedAt DESC
        END
        ELSE IF (@statementType = 'SENDTOUSER')
        BEGIN
            INSERT INTO dbo.Notifications (TargetUsername, TargetRole, Title, Message, IsRead, CreatedAt)
            VALUES (@TargetUsername, NULL, @Title, @Message, 0, GETDATE())
        END
        ELSE IF (@statementType = 'SENDTOROLE')
        BEGIN
            INSERT INTO dbo.Notifications (TargetUsername, TargetRole, Title, Message, IsRead, CreatedAt)
            VALUES (NULL, @TargetRole, @Title, @Message, 0, GETDATE())
        END
        ELSE IF (@statementType = 'MARKASREAD')
        BEGIN
            UPDATE dbo.Notifications SET IsRead = 1 WHERE Id = @Id;
        END
        ELSE IF (@statementType = 'MARKALLREAD')
        BEGIN
            UPDATE dbo.Notifications SET IsRead = 1
            WHERE (TargetUsername = @TargetUsername)
            OR (TargetRole = @TargetRole AND TargetUsername IS NULL)
        END
        ELSE IF (@statementType = 'GETUNREADCOUNT')
        BEGIN
            SELECT COUNT(*) AS UnreadCount
            FROM dbo.Notifications
            WHERE ((TargetUsername = @TargetUsername)
            OR (TargetRole = @TargetRole AND TargetUsername IS NULL))
            AND IsRead = 0;
        END
    END TRY
    BEGIN CATCH
        SELECT 'Error: ' + ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[SP_PASSWORD_RESET]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Jeff Carulasan
-- Description:   Handles forgot-password / reset-password flow.
-- =============================================
CREATE PROCEDURE [dbo].[SP_PASSWORD_RESET]
    @statementType VARCHAR(50),
    @Email VARCHAR(100) = NULL,
    @Token VARCHAR(200) = NULL,
    @NewPasswordHash VARCHAR(256) = NULL,
    @NewSalt VARCHAR(100) = NULL,
    @ExpiresAt DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF (@statementType = 'GETUSERBYEMAIL')
        BEGIN
            SELECT StudentID, Username, Email, FirstName, LastName
            FROM [dbo].[Users]
            WHERE Email = @Email
        END
        ELSE IF (@statementType = 'CREATETOKEN')
        BEGIN
            DECLARE @UserId INT = (SELECT StudentID FROM [dbo].[Users] WHERE Email = @Email)

            IF @UserId IS NOT NULL
            BEGIN
                -- invalidate any previous unused tokens for this user
                UPDATE [dbo].[PasswordResetTokens]
                SET IsUsed = 1
                WHERE UserId = @UserId AND IsUsed = 0

                INSERT INTO [dbo].[PasswordResetTokens] (UserId, Token, ExpiresAt, IsUsed, CreatedAt)
                VALUES (@UserId, @Token, @ExpiresAt, 0, GETDATE())
            END
        END
        ELSE IF (@statementType = 'VALIDATETOKEN')
        BEGIN
            SELECT TOP 1 t.Id, t.UserId, t.ExpiresAt, t.IsUsed, u.Username, u.Email
            FROM [dbo].[PasswordResetTokens] t
            INNER JOIN [dbo].[Users] u ON u.StudentID = t.UserId
            WHERE t.Token = @Token
            ORDER BY t.CreatedAt DESC
        END
        ELSE IF (@statementType = 'RESETPASSWORD')
        BEGIN
            DECLARE @UserId2 INT = (
                SELECT UserId FROM [dbo].[PasswordResetTokens]
                WHERE Token = @Token AND IsUsed = 0 AND ExpiresAt > GETDATE()
            )

            IF @UserId2 IS NOT NULL
            BEGIN
                UPDATE [dbo].[Users]
                SET PasswordHash = @NewPasswordHash, Salt = @NewSalt
                WHERE StudentID = @UserId2

                UPDATE [dbo].[PasswordResetTokens]
                SET IsUsed = 1
                WHERE Token = @Token

                SELECT 1 AS Success
            END
            ELSE
            BEGIN
                SELECT 0 AS Success
            END
        END
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ErrorMessage
        RETURN
    END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[SP_SAO]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Jeff Carulasan
-- Create date:   03/01/2026
-- Description:   Handles all SAO operations
-- =============================================
CREATE PROCEDURE [dbo].[SP_SAO]
    @statementType VARCHAR(50),
    @StudentID INT = NULL,
    @FirstName VARCHAR(100) = NULL,
    @LastName VARCHAR(100) = NULL,
    @Email VARCHAR(100) = NULL,
    @ContactNumber VARCHAR(20) = NULL,
    @Gender VARCHAR(20) = NULL,
    @Address VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF (@statementType = 'GETALLUSERS')
        BEGIN
            SELECT u.*, s.ProfilePhoto
            FROM dbo.Users u
            LEFT JOIN dbo.Students s ON u.StudentNo = s.StudentNo
            ORDER BY u.Role, u.LastName
        END
        ELSE IF (@statementType = 'GETUSERBYID')
        BEGIN
            SELECT * FROM dbo.Users
            WHERE StudentID = @StudentID
        END
        ELSE IF (@statementType = 'UPDATEUSER')
        BEGIN
            UPDATE dbo.Users
            SET FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email,
                ContactNumber = @ContactNumber,
                Gender = @Gender,
                Address = @Address
            WHERE StudentID = @StudentID
        END
        ELSE IF (@statementType = 'GETPENDINGDISMISSAL')
        BEGIN
            SELECT 
                u.StudentID, u.FirstName, u.LastName, u.StudentNo,
                s.Course, s.Year, s.Status, s.ProfilePhoto
            FROM dbo.Users u
            INNER JOIN dbo.Students s ON u.StudentNo = s.StudentNo
            WHERE s.Status = 'PendingDismissal'
            AND u.Role = 'Student'
            ORDER BY u.LastName
        END
        ELSE IF (@statementType = 'GETDISMISSED')
        BEGIN
            SELECT u.StudentID, u.FirstName, u.LastName, u.StudentNo,
                s.Course, s.Year, s.Status, s.ProfilePhoto
            FROM dbo.Users u
            INNER JOIN dbo.Students s ON u.StudentNo = s.StudentNo
            WHERE s.Status = 'Dismissed'
            AND u.Role = 'Student'
            ORDER BY u.LastName
        END
        ELSE IF (@statementType = 'DELETEUSER')
        BEGIN
            DELETE FROM dbo.Students
            WHERE StudentNo = (SELECT StudentNo FROM dbo.Users WHERE StudentID = @StudentID)
            DELETE FROM dbo.Users
            WHERE StudentID = @StudentID
        END
        ELSE IF (@statementType = 'ARCHIVEVIOLATIONS')
        BEGIN
            UPDATE dbo.Violations
            SET IsArchived = 1
            WHERE StudentId = @StudentID
        END
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ErrorMessage;
        RETURN;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[SP_STUDENT_DATA]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:        Jeff Carulasan
-- Create date:   3/01/2026
-- Description:   Handles all Student data operations
-- =============================================
CREATE PROCEDURE [dbo].[SP_STUDENT_DATA]
    @statementType VARCHAR(50),
    @StudentNo VARCHAR(50) = NULL,
    @StudentID INT = NULL,
    @FirstName VARCHAR(100) = NULL,
    @LastName VARCHAR(100) = NULL,
    @Email VARCHAR(100) = NULL,
    @ContactNumber VARCHAR(20) = NULL,
    @Gender VARCHAR(20) = NULL,
    @Address VARCHAR(255) = NULL,
    @Course VARCHAR(50) = NULL,
    @Year VARCHAR(50) = NULL,
    @Status VARCHAR(20) = NULL,
    @ProfilePhoto   NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF (@statementType = 'GETSTUDENT')
        BEGIN
            SELECT * FROM dbo.Students
            WHERE StudentNo = @StudentNo
        END
        ELSE IF (@statementType = 'GETALLSTUDENTS')
        BEGIN
            SELECT * FROM dbo.Students
            ORDER BY LastName
        END
        ELSE IF (@statementType = 'UPDATESTUDENT')
        BEGIN
            UPDATE dbo.Students
            SET FirstName = @FirstName,
                LastName = @LastName,
                Email = @Email,
                ContactNumber = @ContactNumber,
                Gender = @Gender,
                Address = @Address,
                Course = @Course,
                Year = @Year
            WHERE StudentID = @StudentID
        END
        ELSE IF (@statementType = 'GETUSERNAMEBYNO')
        BEGIN
            SELECT Username FROM dbo.Users
            WHERE StudentNo = @StudentNo AND Role = 'student'
        END
        ELSE IF (@statementType = 'UPDATESTATUS')
        BEGIN
            UPDATE dbo.Students
            SET Status = @Status
            WHERE StudentID = @StudentID
        END
        ELSE IF @statementType = 'UPDATEPHOTO'
        BEGIN 
            UPDATE Students
            SET ProfilePhoto = @ProfilePhoto
            WHERE StudentNO = @StudentNo
        END
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ErrorMessage;
        RETURN;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[SP_STUDENT_GETUSERLOGIN]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Jeff Carulasan
-- Create date:   2/24/2026
-- Description:   Retrieves user login information or checks if a user exists.
-- =============================================
CREATE PROCEDURE [dbo].[SP_STUDENT_GETUSERLOGIN]
    @username VARCHAR(50),
    @statementType VARCHAR(50),
    @email VARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF (@statementType = 'GETLOGIN')
BEGIN
    SELECT
        u.StudentID,
        u.Username,
        u.PasswordHash,
        u.Salt,
        u.FirstName,
        u.LastName,
        u.Role,
        u.Email,
        u.ContactNumber,
        u.StudentNo,
        ISNULL(s.Status, 'Active') AS Status
    FROM [dbo].[Users] u
    LEFT JOIN [dbo].[Students] s ON u.StudentNo = s.StudentNo
    WHERE u.Username = @username
END
        ELSE IF (@statementType = 'USEREXISTS')
        BEGIN
            SELECT
                CASE
                    WHEN EXISTS (
                        SELECT 1
                        FROM [dbo].[Users]
                        WHERE Username = @username OR Email = @email
                    )
                    THEN 1
                    ELSE 0
                END AS UserExists;
        END
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ErrorMessage;
        RETURN;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[SP_STUDENT_REGISTRATION]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Jeff Carulasan
-- Create date:   2/24/2026
-- Description:   Handles student registration or checks if a user exists.
-- =============================================
CREATE PROCEDURE [dbo].[SP_STUDENT_REGISTRATION]
    @FirstName VARCHAR(50),
    @LastName VARCHAR(50),
    @DateOfBirth DATE,
    @Gender VARCHAR(10),
    @Address VARCHAR(100),
    @ContactNumber VARCHAR(20),
    @Email VARCHAR(50),
    @RegistrationDate DATETIME,
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(255),
    @Salt VARCHAR(255),
    @Role VARCHAR(50),
    @Course VARCHAR(50),
    @Year VARCHAR(20),
    @statementType VARCHAR(50),
    @StudentNo VARCHAR(50) = NULL,
    @QRCode VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY

        -- Registers a new user into the Users table
        IF (@statementType = 'REGISTER')
        BEGIN
            BEGIN TRANSACTION;

            -- Insert into Users table
            INSERT INTO [dbo].[Users] (
                FirstName, LastName, DateOfBirth, Gender, Address,
                ContactNumber, Email, RegistrationDate, Username,
                PasswordHash, Salt, Role, Course, Year, StudentNo
            )
            VALUES (
                @FirstName, @LastName, @DateOfBirth, @Gender, @Address,
                @ContactNumber, @Email, @RegistrationDate, @Username,
                @PasswordHash, @Salt, @Role, @Course, @Year, @StudentNo
            );

            -- If student role, also insert into Students table
            IF (LOWER(@Role) = 'student')
            BEGIN
                DECLARE @NewStudentId INT;

                INSERT INTO [dbo].[Students] (
                    FirstName, LastName, Gender, ContactNumber, Email,
                    RegistrationDate, DateOfBirth, Address, Course, Year, StudentNo, QRCode
                )
                VALUES (
                    @FirstName, @LastName, @Gender, @ContactNumber, @Email,
                    @RegistrationDate, @DateOfBirth, @Address, @Course, @Year, @StudentNo, @QRCode
                );

                SET @NewStudentId = SCOPE_IDENTITY();
            END

            COMMIT TRANSACTION;
        END

        -- Checks if a username or email is already taken
        ELSE IF (@statementType = 'USEREXISTS')
        BEGIN
            SELECT CASE
                WHEN EXISTS (
                    SELECT 1 FROM [dbo].[Users]
                    WHERE Username = @Username OR Email = @Email
                )
                THEN 1 ELSE 0
            END AS UserExists;
        END

        -- Checks if a StudentNo is already registered in the Students table
        ELSE IF (@statementType = 'STUDENTNOEXISTS')
        BEGIN
            SELECT CASE
                WHEN EXISTS (
                    SELECT 1 FROM [dbo].[Students]
                    WHERE StudentNo = @StudentNo
                )
                THEN 1 ELSE 0
            END AS StudentNoExists;
        END

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT ERROR_MESSAGE() AS ErrorMessage;
        RETURN;
    END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[SP_VIOLATION]    Script Date: 9/23/2026 7:45:13 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:        Jeff Carulasan
-- Create date:   3/01/2026
-- Description:   Handles all Student Violation operations
-- =============================================
CREATE PROCEDURE [dbo].[SP_VIOLATION]
    @statementType VARCHAR(50),
    @ViolationID INT = NULL,
    @Status VARCHAR(50) = NULL,
    @StudentNo VARCHAR(50) = NULL,
    @StudentId INT = NULL,
    @ViolationName VARCHAR(100) = NULL,
    @Description VARCHAR(500) = NULL,
    @Severity VARCHAR(50) = NULL,
    @GuardId VARCHAR(50) = NULL,
    @AppealText VARCHAR(MAX) = NULL,
    @AppealStatus VARCHAR(20) = NULL,
    @AppealRemarks VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF (@statementType = 'GETALL')
        BEGIN
            SELECT 
                v.ViolationID,
                v.StudentId,
                s.StudentNo,
                v.ViolationName,
                v.Description,
                v.Severity,
                v.ViolationDate,
                v.Status,
                v.GuardId,
                v.AppealText,
                v.AppealStatus,
                v.AppealRemarks,
                CONCAT(u.FirstName, ' ', u.LastName) AS GuardName
            FROM dbo.Violations v
            LEFT JOIN dbo.Students s ON v.StudentId = s.StudentID
            LEFT JOIN dbo.Users u ON v.GuardId = CAST(u.StudentID AS VARCHAR)
            ORDER BY v.ViolationDate DESC
        END
        ELSE IF (@statementType = 'GETBYID')
        BEGIN
            SELECT 
                v.ViolationID,
                v.StudentId,
                s.StudentNo,
                v.ViolationName,
                v.Description,
                v.Severity,
                v.ViolationDate,
                v.Status,
                v.GuardId,
                v.AppealText,
                v.AppealStatus,
                v.AppealRemarks,
                CONCAT(u.FirstName, ' ', u.LastName) AS GuardName
            FROM dbo.Violations v
            LEFT JOIN dbo.Students s ON v.StudentId = s.StudentID
            LEFT JOIN dbo.Users u ON v.GuardId = CAST(u.StudentID AS VARCHAR)
            WHERE v.ViolationID = @ViolationID
        END
        ELSE IF (@statementType = 'GETBYSTUDENT')
BEGIN
    SELECT 
        v.ViolationID,
        v.StudentId,
        s.StudentNo,
        v.ViolationName,
        v.Description,
        v.Severity,
        v.ViolationDate,
        v.Status,
        v.GuardId,
        v.AppealText,
        v.AppealStatus,
        v.AppealRemarks,
        v.IsArchived,
        CONCAT(u.FirstName, ' ', u.LastName) AS GuardName
    FROM dbo.Violations v
    LEFT JOIN dbo.Students s ON v.StudentId = s.StudentID
    LEFT JOIN dbo.Users u ON v.GuardId = CAST(u.StudentID AS VARCHAR)
    WHERE s.StudentNo = @StudentNo
    ORDER BY v.ViolationDate DESC
END
        ELSE IF (@statementType = 'RECORDVIOLATION')
        BEGIN
            INSERT INTO dbo.Violations 
                (StudentId, ViolationName, Description, Severity, GuardId, ViolationDate, Status, AppealStatus)
            VALUES 
                (@StudentId, @ViolationName, @Description, @Severity, @GuardId, GETDATE(), 'Pending', 'None')
        END
        ELSE IF (@statementType = 'UPDATESTATUS')
        BEGIN
            UPDATE dbo.Violations
            SET Status = @Status
            WHERE ViolationID = @ViolationID
        END
        ELSE IF (@statementType = 'UPDATEAPPEALSTATUS')
        BEGIN
            UPDATE dbo.Violations
            SET AppealStatus = @AppealStatus,
                AppealRemarks = @AppealRemarks
            WHERE ViolationID = @ViolationID
        END
        ELSE IF (@statementType = 'SUBMITAPPEAL')
        BEGIN
            UPDATE dbo.Violations
            SET AppealText = @AppealText,
                AppealStatus = 'Pending'
            WHERE ViolationID = @ViolationID
        END
        ELSE IF (@statementType = 'DELETE')
        BEGIN
            DELETE FROM dbo.Violations
            WHERE ViolationID = @ViolationID
        END
        ELSE IF (@statementType = 'ARCHIVEVIOLATIONS')
BEGIN
    UPDATE dbo.Violations
    SET IsArchived = 1
    WHERE StudentId = @StudentId
END
    END TRY
    BEGIN CATCH
        SELECT ERROR_MESSAGE() AS ErrorMessage;
        RETURN;
    END CATCH
END
GO
USE [master]
GO
ALTER DATABASE [StudentViolations] SET  READ_WRITE 
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
