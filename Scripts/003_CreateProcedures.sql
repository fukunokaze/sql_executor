-- Create stored procedures (idempotent using CREATE OR ALTER)
CREATE OR ALTER PROCEDURE GetUserByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SELECT UserId, Username, Email, CreatedDate
    FROM SampleUsers
    WHERE Email = @Email;
END;
GO

-- Create another stored procedure (idempotent)
CREATE OR ALTER PROCEDURE GetAllUsers
AS
BEGIN
    SELECT UserId, Username, Email, CreatedDate
    FROM SampleUsers
    ORDER BY CreatedDate DESC;
END;
GO
