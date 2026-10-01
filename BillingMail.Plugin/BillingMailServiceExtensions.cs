using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using BillingMail.Plugin.BackgroundServices;
using BillingMail.Plugin.Options;
using BillingMail.Plugin.Services;

namespace BillingMail.Plugin;

/// <summary>
/// Service Collection Extensions for registering Billing Mail Plugin services in .NET 10 Web Applications.
/// </summary>
public static class BillingMailServiceExtensions
{
    /// <summary>
    /// Registers the Billing Mail Plugin services, options, and background worker.
    /// Usage in Program.cs:
    ///   builder.Services.AddBillingMailPlugin(builder.Configuration);
    /// </summary>
    public static IServiceCollection AddBillingMailPlugin(this IServiceCollection services, IConfiguration configuration)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        // Bind Configuration Section [BillingMail]
        services.Configure<BillingMailOptions>(configuration.GetSection(BillingMailOptions.SectionName));

        // Connections:Primary is the single source of truth for the primary database so the
        // web application and this worker can never point at different databases. Applied
        // after the section binding, so it wins when supplied.
        services.Configure<BillingMailOptions>(options =>
        {
            var provider = configuration["Connections:Primary:Provider"];
            if (!string.IsNullOrWhiteSpace(provider))
            {
                options.DatabaseProvider = provider;
            }

            var connectionString = configuration["Connections:Primary:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.ConnectionString = connectionString;
            }
        });

        // Register Plugin Core Services
        services.AddScoped<IBillingRepository, BillingRepository>();
        services.AddScoped<IStatementEmailBuilder, StatementEmailBuilder>();
        services.AddScoped<IEmailDispatcher, MailKitDispatcher>();
        services.AddScoped<IBillingMailProcessor, BillingMailProcessor>();

        // Register Hosted Background Worker
        services.AddHostedService<BillingMailWorker>();

        return services;
    }

    /// <summary>
    /// Configures Serilog daily rolling file logging for the host application in production or local environments.
    /// Logs are saved under the specified directory (default: "logs/billing-mail-YYYYMMDD.log").
    /// Usage in Program.cs:
    ///   builder.Host.UseBillingMailFileLogging();
    /// </summary>
    public static IHostBuilder UseBillingMailFileLogging(this IHostBuilder hostBuilder, string logDirectory = "logs")
    {
        if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));

        Directory.CreateDirectory(logDirectory);
        var logFilePath = Path.Combine(logDirectory, "billing-mail-.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.File(
                path: logFilePath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        return hostBuilder.UseSerilog();
    }
}
