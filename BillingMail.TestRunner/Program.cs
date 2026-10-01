using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Dapper;
using Npgsql;
using Microsoft.Data.SqlClient;
using Serilog;
using Serilog.Events;
using BillingMail.Plugin;
using BillingMail.Plugin.Options;
using BillingMail.Plugin.Services;

namespace BillingMail.TestRunner;

class Program
{
    static async Task Main(string[] args)
    {
        var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        Directory.CreateDirectory(logsDirectory);
        var logFilePath = Path.Combine(logsDirectory, "billing-mail-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.File(
                path: logFilePath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("   Billing Mail Plugin (.NET 10) - Local Test   ");
            Console.WriteLine("=================================================");
            Console.WriteLine();
            Console.WriteLine("Select Test Mode:");
            Console.WriteLine("  [1] Immediate Single Batch Execution (Instant Test)");
            Console.WriteLine("  [2] Continuous BackgroundWorker Host Loop (Scheduler Test)");
            Console.WriteLine("  [3] Reset Database Test Record (Set SentDate = NULL)");
            Console.Write("\nEnter choice (1, 2 or 3, default = 1): ");

            var choice = args.Length > 0 ? args[0] : Console.ReadLine()?.Trim();

            var host = Host.CreateDefaultBuilder(args)
                .UseBillingMailFileLogging("logs")
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                    config.AddJsonFile(settingsPath, optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddBillingMailPlugin(context.Configuration);
                })
                .Build();

            if (choice == "3")
            {
                Console.WriteLine("\n[+] Resetting scheduled mail records in database...\n");
                using (var scope = host.Services.CreateScope())
                {
                    var opts = scope.ServiceProvider.GetRequiredService<IOptions<BillingMailOptions>>().Value;
                    var isPostgres = string.Equals(opts.DatabaseProvider, "PostgreSQL", StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(opts.DatabaseProvider, "Postgres", StringComparison.OrdinalIgnoreCase);

                    using var conn = isPostgres 
                        ? (System.Data.IDbConnection)new NpgsqlConnection(opts.ConnectionString)
                        : new SqlConnection(opts.ConnectionString);

                    var resetSql = isPostgres 
                        ? @"UPDATE dbo.""tblScheduledMail"" SET ""SentDate"" = NULL, ""Status"" = ''" 
                        : @"UPDATE [tblScheduledMail] SET [SentDate] = NULL, [Status] = ''";

                    var updatedRows = await conn.ExecuteAsync(resetSql);
                    Log.Information("Reset complete. Updated {Rows} record(s) in tblScheduledMail.", updatedRows);
                    Console.WriteLine($"[+] Reset complete! Updated {updatedRows} record(s) back to pending state.");
                }
            }
            else if (choice == "2")
            {
                Console.WriteLine("\n[+] Starting Continuous Host Worker Loop. Press Ctrl+C to stop...\n");
                await host.RunAsync();
            }
            else
            {
                Console.WriteLine("\n[+] Running Immediate Single Batch Execution...\n");
                using (var scope = host.Services.CreateScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<IBillingMailProcessor>();
                    await processor.ProcessPendingMailsAsync(CancellationToken.None);
                }
                Console.WriteLine("\n[+] Single batch test execution completed!");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
