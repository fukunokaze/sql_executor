-- Create a sample table for testing (idempotent)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SampleUsers' AND type = 'U')
BEGIN
    CREATE TABLE SampleUsers (
        UserId INT PRIMARY KEY IDENTITY(1,1),
        Username NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        CreatedDate DATETIME DEFAULT GETDATE()
    );
END
