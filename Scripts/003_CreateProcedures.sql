-- Create a stored procedure with GO statement
CREATE PROCEDURE GetUserByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SELECT UserId, Username, Email, CreatedDate
    FROM SampleUsers
    WHERE Email = @Email;
END;
GO

-- Create another stored procedure
CREATE PROCEDURE GetAllUsers
AS
BEGIN
    SELECT UserId, Username, Email, CreatedDate
    FROM SampleUsers
    ORDER BY CreatedDate DESC;
END;
GO
