# SQL Scripts Directory

This directory is where you place SQL scripts that will be executed by the SQL Executor application.

## Naming Convention

Scripts are executed in **alphabetical order** by filename. It is recommended to use numeric prefixes to control the execution order:

### Examples:
```
001_CreateDatabase.sql
002_CreateTables.sql
003_CreateIndexes.sql
004_InsertSeedData.sql
005_CreateStoredProcedures.sql
```

## File Format

- **File Extension**: All SQL scripts must have a `.sql` extension
- **Encoding**: UTF-8 encoding is recommended
- **GO Statements**: The application supports the `GO` statement (case-insensitive) to separate batches of SQL commands
  - Each batch separated by `GO` is executed as a separate command
  - This is necessary for certain SQL statements that must be the first in a batch (e.g., `CREATE PROCEDURE`)

## Example Script Content

### Simple Script (001_CreateTable.sql)
```sql
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
);
```

### Script with GO Statements (002_CreateProcedure.sql)
```sql
-- Create the stored procedure
CREATE PROCEDURE GetUserByEmail
    @Email NVARCHAR(100)
AS
BEGIN
    SELECT UserId, Username, Email, CreatedDate
    FROM Users
    WHERE Email = @Email;
END;
GO

-- Grant execute permission
GRANT EXECUTE ON GetUserByEmail TO PUBLIC;
GO
```

## Best Practices

1. **Use Descriptive Names**: Make script names clear and descriptive
2. **Keep Scripts Focused**: Each script should have a single, clear purpose
3. **Include Comments**: Add comments to explain complex logic
4. **Test Scripts**: Test scripts individually before adding them to this directory
5. **Use Transactions**: The application wraps all script execution in a transaction, but you can also use explicit transactions within scripts if needed
6. **Handle Idempotency**: Consider making scripts idempotent (safe to run multiple times) using checks like `IF NOT EXISTS`

## Execution Behavior

- All scripts are executed within a **single transaction**
- If any script fails, **all changes are rolled back**
- Scripts are executed in **alphabetical order**
- Execution time is tracked and displayed for each script
- Success/failure status is shown in the console with color coding

## Troubleshooting

- **Syntax Errors**: Check SQL syntax using SQL Server Management Studio or Azure Data Studio
- **Permission Issues**: Ensure the database user has appropriate permissions
- **GO Statement Issues**: Make sure GO statements are on their own line
- **Object Already Exists**: Use `IF NOT EXISTS` checks or `DROP IF EXISTS` statements
