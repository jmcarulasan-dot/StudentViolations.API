/* Apply once to the StudentViolations database before deploying the API build. */
IF OBJECT_ID(N'dbo.AuditTrail', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditTrail
    (
        AuditID BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditTrail PRIMARY KEY,
        Action NVARCHAR(80) NOT NULL,
        EntityType NVARCHAR(40) NOT NULL,
        EntityID NVARCHAR(80) NOT NULL,
        StudentNo NVARCHAR(50) NULL,
        PreviousValue NVARCHAR(200) NULL,
        NewValue NVARCHAR(200) NULL,
        Remarks NVARCHAR(2000) NULL,
        ActorUsername NVARCHAR(100) NOT NULL,
        ActorRole NVARCHAR(30) NOT NULL,
        CreatedAtUtc DATETIME2(3) NOT NULL CONSTRAINT DF_AuditTrail_CreatedAtUtc DEFAULT SYSUTCDATETIME()
    );

    CREATE INDEX IX_AuditTrail_StudentNo_AuditID
        ON dbo.AuditTrail (StudentNo, AuditID DESC);
END;
GO

CREATE OR ALTER PROCEDURE dbo.SP_AUDIT_TRAIL
    @statementType VARCHAR(20),
    @Action NVARCHAR(80) = NULL,
    @EntityType NVARCHAR(40) = NULL,
    @EntityID NVARCHAR(80) = NULL,
    @StudentNo NVARCHAR(50) = NULL,
    @PreviousValue NVARCHAR(200) = NULL,
    @NewValue NVARCHAR(200) = NULL,
    @Remarks NVARCHAR(2000) = NULL,
    @ActorUsername NVARCHAR(100) = NULL,
    @ActorRole NVARCHAR(30) = NULL,
    @Take INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @statementType = 'INSERT'
    BEGIN
        INSERT INTO dbo.AuditTrail
            (Action, EntityType, EntityID, StudentNo, PreviousValue, NewValue, Remarks, ActorUsername, ActorRole)
        VALUES
            (@Action, @EntityType, @EntityID, @StudentNo, @PreviousValue, @NewValue, @Remarks, @ActorUsername, @ActorRole);
        RETURN;
    END;

    IF @statementType = 'GETRECENT'
    BEGIN
        SELECT TOP (@Take)
            AuditID, Action, EntityType, EntityID, StudentNo, PreviousValue, NewValue,
            Remarks, ActorUsername, ActorRole, CreatedAtUtc
        FROM dbo.AuditTrail
        WHERE (@StudentNo IS NULL OR StudentNo = @StudentNo)
        ORDER BY AuditID DESC;
        RETURN;
    END;

    THROW 50001, 'Unsupported SP_AUDIT_TRAIL statement type.', 1;
END;
GO
