using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Display header
            WriteColoredLine("=================================================", ConsoleColor.Cyan);
            WriteColoredLine("         SQL Script Executor v1.0", ConsoleColor.Cyan);
            WriteColoredLine("=================================================", ConsoleColor.Cyan);
            Console.WriteLine();

            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var connectionString = configuration.GetConnectionString("SqlServer");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                WriteColoredLine("ERROR: Connection string 'SqlServer' not found in appsettings.json", ConsoleColor.Red);
                return 1;
            }

            // Display connection info (masked)
            Console.WriteLine($"Connection String: {MaskConnectionString(connectionString)}");
            Console.WriteLine();

            // Test connection
            WriteColoredLine("Testing database connection...", ConsoleColor.Yellow);
            if (!await TestConnection(connectionString))
            {
                WriteColoredLine("ERROR: Failed to connect to database. Please check your connection string.", ConsoleColor.Red);
                return 1;
            }
            WriteColoredLine("✓ Database connection successful!", ConsoleColor.Green);
            Console.WriteLine();

            // Get script directory
            var scriptsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
            
            if (!Directory.Exists(scriptsDirectory))
            {
                WriteColoredLine($"WARNING: Scripts directory not found at: {scriptsDirectory}", ConsoleColor.Yellow);
                WriteColoredLine("Creating Scripts directory...", ConsoleColor.Yellow);
                Directory.CreateDirectory(scriptsDirectory);
                WriteColoredLine("Please add .sql files to the Scripts directory and run again.", ConsoleColor.Yellow);
                return 0;
            }

            // Get all SQL files
            var sqlFiles = Directory.GetFiles(scriptsDirectory, "*.sql", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f)
                .ToArray();

            if (sqlFiles.Length == 0)
            {
                WriteColoredLine("WARNING: No .sql files found in Scripts directory.", ConsoleColor.Yellow);
                WriteColoredLine("Please add SQL scripts to the Scripts directory and run again.", ConsoleColor.Yellow);
                return 0;
            }

            Console.WriteLine($"Found {sqlFiles.Length} SQL script(s) to execute:");
            foreach (var file in sqlFiles)
            {
                Console.WriteLine($"  - {Path.GetFileName(file)}");
            }
            Console.WriteLine();

            // Execute scripts
            WriteColoredLine("Starting script execution...", ConsoleColor.Cyan);
            Console.WriteLine();

            var result = await ExecuteScripts(connectionString, sqlFiles);

            // Display summary
            Console.WriteLine();
            WriteColoredLine("=================================================", ConsoleColor.Cyan);
            WriteColoredLine("              Execution Summary", ConsoleColor.Cyan);
            WriteColoredLine("=================================================", ConsoleColor.Cyan);
            Console.WriteLine($"Total Scripts:     {result.TotalScripts}");
            WriteColoredLine($"Successful:        {result.SuccessfulScripts}", ConsoleColor.Green);
            if (result.FailedScripts > 0)
            {
                WriteColoredLine($"Failed:            {result.FailedScripts}", ConsoleColor.Red);
            }
            else
            {
                WriteColoredLine($"Failed:            {result.FailedScripts}", ConsoleColor.Gray);
            }
            Console.WriteLine($"Total Time:        {result.TotalTime:F2} seconds");
            
            if (result.SuccessfulScripts == result.TotalScripts)
            {
                Console.WriteLine();
                WriteColoredLine("✓ All scripts executed successfully!", ConsoleColor.Green);
                return 0;
            }
            else
            {
                Console.WriteLine();
                WriteColoredLine("✗ Some scripts failed. All changes have been rolled back.", ConsoleColor.Red);
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            WriteColoredLine($"FATAL ERROR: {ex.Message}", ConsoleColor.Red);
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    static async Task<bool> TestConnection(string connectionString)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return true;
        }
        catch (Exception ex)
        {
            WriteColoredLine($"Connection Error: {ex.Message}", ConsoleColor.Red);
            return false;
        }
    }

    static async Task<ExecutionResult> ExecuteScripts(string connectionString, string[] scriptFiles)
    {
        var result = new ExecutionResult
        {
            TotalScripts = scriptFiles.Length
        };

        var totalStopwatch = Stopwatch.StartNew();

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var scriptFile in scriptFiles)
            {
                var scriptName = Path.GetFileName(scriptFile);
                Console.Write($"Executing: {scriptName}... ");

                var scriptStopwatch = Stopwatch.StartNew();

                try
                {
                    var scriptContent = await File.ReadAllTextAsync(scriptFile);
                    var batches = SplitSqlBatches(scriptContent);

                    foreach (var batch in batches)
                    {
                        if (string.IsNullOrWhiteSpace(batch))
                            continue;

                        using var command = new SqlCommand(batch, connection, transaction);
                        command.CommandTimeout = 300; // 5 minutes timeout
                        await command.ExecuteNonQueryAsync();
                    }

                    scriptStopwatch.Stop();
                    WriteColoredLine($"✓ SUCCESS ({scriptStopwatch.ElapsedMilliseconds}ms)", ConsoleColor.Green);
                    result.SuccessfulScripts++;
                }
                catch (Exception ex)
                {
                    scriptStopwatch.Stop();
                    WriteColoredLine($"✗ FAILED ({scriptStopwatch.ElapsedMilliseconds}ms)", ConsoleColor.Red);
                    WriteColoredLine($"  Error: {ex.Message}", ConsoleColor.Red);
                    result.FailedScripts++;
                    throw; // Re-throw to trigger rollback
                }
            }

            // If we get here, all scripts succeeded, commit the transaction
            await transaction.CommitAsync();
            WriteColoredLine("\n✓ Transaction committed successfully.", ConsoleColor.Green);
        }
        catch (Exception)
        {
            // Rollback on any error
            try
            {
                await transaction.RollbackAsync();
                WriteColoredLine("\n✗ Transaction rolled back due to errors.", ConsoleColor.Red);
            }
            catch (Exception rollbackEx)
            {
                WriteColoredLine($"\n✗ Error during rollback: {rollbackEx.Message}", ConsoleColor.Red);
            }
        }

        totalStopwatch.Stop();
        result.TotalTime = totalStopwatch.Elapsed.TotalSeconds;

        return result;
    }

    static List<string> SplitSqlBatches(string script)
    {
        // Split by GO statement (case-insensitive, must be on its own line)
        var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .ToList();

        return batches;
    }

    static string MaskConnectionString(string connectionString)
    {
        // Mask password in connection string for display
        var masked = Regex.Replace(connectionString, 
            @"(Password|Pwd)\s*=\s*[^;]*", 
            "$1=****", 
            RegexOptions.IgnoreCase);
        return masked;
    }

    static void WriteColoredLine(string text, ConsoleColor color)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = originalColor;
    }

    class ExecutionResult
    {
        public int TotalScripts { get; set; }
        public int SuccessfulScripts { get; set; }
        public int FailedScripts { get; set; }
        public double TotalTime { get; set; }
    }
}
