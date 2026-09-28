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
