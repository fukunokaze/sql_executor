-- Insert sample data (idempotent - only insert if table is empty)
IF NOT EXISTS (SELECT 1 FROM SampleUsers)
BEGIN
    INSERT INTO SampleUsers (Username, Email) VALUES ('john_doe', 'john@example.com');
    INSERT INTO SampleUsers (Username, Email) VALUES ('jane_smith', 'jane@example.com');
    INSERT INTO SampleUsers (Username, Email) VALUES ('bob_wilson', 'bob@example.com');
END
