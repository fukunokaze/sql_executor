# SQL Executor

A .NET console application that executes multiple SQL scripts against a SQL Server database with transaction support, error handling, and detailed logging.

## Features

- ✅ Execute multiple SQL scripts in alphabetical order
- ✅ Transaction support - all-or-nothing execution (automatic rollback on failure)
- ✅ GO statement parsing for batch execution
- ✅ Color-coded console output (green for success, red for errors)
- ✅ Execution time tracking for each script
- ✅ Connection testing before script execution
- ✅ Comprehensive error handling with detailed error messages
- ✅ Progress display during execution
- ✅ Execution summary at the end
- ✅ Graceful handling of missing or empty Scripts directory

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- SQL Server (2016 or later) or Azure SQL Database
- Network access to SQL Server instance
- Database user with appropriate permissions (CREATE, ALTER, INSERT, UPDATE, DELETE, etc.)

## Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/fukunokaze/sql_executor.git
cd sql_executor
```

### 2. Configure Connection String

Edit the `appsettings.json` file and update the connection string with your SQL Server details:

```json
{
  "ConnectionStrings": {
    "SqlServer": "Server=your-server;Database=your-database;User Id=your-user;Password=your-password;TrustServerCertificate=True;"
  }
}
```

#### Connection String Examples

**SQL Server Authentication:**
```
Server=localhost;Database=MyDatabase;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;
```

**Windows/Integrated Authentication:**
```
Server=localhost;Database=MyDatabase;Integrated Security=True;TrustServerCertificate=True;
```

**Azure SQL Database:**
```
Server=your-server.database.windows.net;Database=your-database;User Id=your-user;Password=your-password;Encrypt=True;
```

**Named Instance:**
```
Server=localhost\SQLEXPRESS;Database=MyDatabase;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;
```

### 3. Add SQL Scripts

Create SQL script files in the `Scripts` directory. Scripts are executed in **alphabetical order**, so use numeric prefixes:

```
Scripts/
├── 001_CreateDatabase.sql
├── 002_CreateTables.sql
├── 003_InsertData.sql
└── 004_CreateIndexes.sql
```

See [Scripts/README.md](Scripts/README.md) for detailed guidelines on creating SQL scripts.

### 4. Build the Application

```bash
dotnet build
```

### 5. Run the Application

```bash
dotnet run
```

Or run the compiled executable:

```bash
dotnet run --project SqlExecutor.csproj
```

## Usage

### Basic Usage

Simply run the application - it will automatically discover and execute all `.sql` files in the Scripts directory:

```bash
dotnet run
```

### Example Output

```
=================================================
         SQL Script Executor v1.0
=================================================

Connection String: Server=localhost;Database=TestDB;User Id=sa;Password=****;TrustServerCertificate=True;

Testing database connection...
✓ Database connection successful!

Found 3 SQL script(s) to execute:
  - 001_CreateTables.sql
  - 002_InsertData.sql
  - 003_CreateIndexes.sql

Starting script execution...

Executing: 001_CreateTables.sql... ✓ SUCCESS (45ms)
Executing: 002_InsertData.sql... ✓ SUCCESS (23ms)
Executing: 003_CreateIndexes.sql... ✓ SUCCESS (67ms)

✓ Transaction committed successfully.

=================================================
              Execution Summary
=================================================
Total Scripts:     3
Successful:        3
Failed:            0
Total Time:        0.15 seconds

✓ All scripts executed successfully!
```

### Error Handling Example

If a script fails, all changes are automatically rolled back:

```
Executing: 001_CreateTables.sql... ✓ SUCCESS (45ms)
Executing: 002_BadScript.sql... ✗ FAILED (12ms)
  Error: Invalid column name 'NonExistentColumn'.

✗ Transaction rolled back due to errors.

=================================================
              Execution Summary
=================================================
Total Scripts:     2
Successful:        1
Failed:            1
Total Time:        0.06 seconds

✗ Some scripts failed. All changes have been rolled back.
```

## Project Structure

```
sql_executor/
├── SqlExecutor.csproj          # Project file with NuGet package references
├── Program.cs                   # Main application logic
├── appsettings.json            # Configuration file (connection strings)
├── .gitignore                  # Git ignore file
├── Scripts/                    # Directory for SQL scripts
│   └── README.md               # Guidelines for SQL scripts
└── README.md                   # This file
```

## How It Works

1. **Configuration Loading**: Reads connection string from `appsettings.json`
2. **Connection Test**: Validates database connectivity before execution
3. **Script Discovery**: Finds all `.sql` files in the Scripts directory
4. **Alphabetical Ordering**: Sorts scripts by filename
5. **Transaction Start**: Begins a database transaction
6. **Script Execution**: 
   - Reads each script file
   - Splits by GO statements (case-insensitive)
   - Executes each batch
   - Tracks execution time
7. **Transaction Commit**: If all scripts succeed, commits the transaction
8. **Rollback on Error**: If any script fails, rolls back all changes
9. **Summary Display**: Shows execution statistics

## Troubleshooting

### Connection Issues

**Problem**: Cannot connect to SQL Server

**Solutions**:
- Verify SQL Server is running: `services.msc` (Windows) or `sudo systemctl status mssql-server` (Linux)
- Check firewall settings - ensure port 1433 is open
- Verify connection string parameters (server name, database name, credentials)
- For `TrustServerCertificate=True` - use this for local development with self-signed certificates
- For named instances, use format: `Server=localhost\SQLEXPRESS`

### Permission Issues

**Problem**: Permission denied errors when executing scripts

**Solutions**:
- Ensure database user has necessary permissions (CREATE TABLE, INSERT, UPDATE, etc.)
- Grant appropriate roles: `db_owner` for full access or specific permissions as needed
- Check if database exists and user has access to it

### Script Execution Issues

**Problem**: Script fails with syntax errors

**Solutions**:
- Test scripts individually in SQL Server Management Studio (SSMS) or Azure Data Studio
- Ensure GO statements are on their own line
- Check for special characters or encoding issues (use UTF-8)
- Verify SQL Server version compatibility

**Problem**: Transaction timeout

**Solutions**:
- Break large scripts into smaller ones
- Increase command timeout (currently set to 300 seconds / 5 minutes)
- Optimize slow queries

### File Not Found Issues

**Problem**: Scripts directory or appsettings.json not found

**Solutions**:
- Ensure you're running from the project directory
- Check that `appsettings.json` has "Copy to Output Directory" set to "PreserveNewest"
- Verify Scripts directory exists in the same location as the executable

## Advanced Configuration

### Custom Script Directory

Modify `Program.cs` to change the scripts directory location:

```csharp
var scriptsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "MyCustomScripts");
```

### Adjust Command Timeout

Modify the timeout in `Program.cs`:

```csharp
command.CommandTimeout = 600; // 10 minutes
```

### Multiple Environments

Create environment-specific configuration files:

```
appsettings.json                 # Default
appsettings.Development.json     # Development
appsettings.Production.json      # Production
```

## Security Best Practices

- ❌ **DO NOT** commit `appsettings.json` with real credentials to source control
- ✅ Use environment variables or Azure Key Vault for sensitive data in production
- ✅ Use Windows Authentication when possible
- ✅ Apply principle of least privilege - grant only necessary permissions
- ✅ Use strong passwords for SQL authentication
- ✅ Enable TLS/SSL for remote connections (remove `TrustServerCertificate=True` in production)

## Development

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run
```

### Clean

```bash
dotnet clean
```

### Publish (Self-Contained)

```bash
dotnet publish -c Release -r win-x64 --self-contained true
dotnet publish -c Release -r linux-x64 --self-contained true
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is open source and available under the MIT License.

## Support

For issues, questions, or contributions, please open an issue on GitHub.

## Changelog

### v1.0 (Initial Release)
- Script execution with transaction support
- GO statement parsing
- Colored console output
- Execution time tracking
- Connection testing
- Comprehensive error handling
- Configuration management