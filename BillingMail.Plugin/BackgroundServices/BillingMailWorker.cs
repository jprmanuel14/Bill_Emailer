using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BillingMail.Plugin.Options;
using BillingMail.Plugin.Services;

namespace BillingMail.Plugin.BackgroundServices;

/// <summary>
/// Cross-platform background worker service compatible with Linux & Windows .NET Host containers.
/// </summary>
public class BillingMailWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BillingMailWorker> _logger;
    private readonly BillingMailOptions _options;

    public BillingMailWorker(
        IServiceProvider serviceProvider,
        ILogger<BillingMailWorker> logger,
        IOptions<BillingMailOptions> options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Billing Mail Background Worker started (CallType: {CallType}, Interval: {Interval}s)", _options.CallType, _options.CallDurationSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<IBillingMailProcessor>();
                    await processor.ProcessPendingMailsAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Billing Mail background loop.");
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            var delaySeconds = CalculateNextDelaySeconds();
            _logger.LogInformation("Next execution in {Seconds} seconds.", delaySeconds);

            // The delay is awaited with the stopping token so shutdown is prompt, which means
            // it throws OperationCanceledException on every normal stop. That must not escape
            // ExecuteAsync: with the default BackgroundServiceExceptionBehavior.StopHost an
            // escaping exception stops the whole web application, not just this worker.
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Billing Mail Background Worker is stopping.");
    }

    private int CalculateNextDelaySeconds()
    {
        if (_options.CallType == 1 && DateTime.TryParse(_options.StartTime, out var startTime))
        {
            var now = DateTime.Now;
            var targetTime = now.Date.Add(startTime.TimeOfDay);

            if (targetTime <= now)
            {
                targetTime = targetTime.AddDays(1);
            }

            var secondsUntilTarget = (int)(targetTime - now).TotalSeconds;
            return Math.Max(secondsUntilTarget, 10);
        }

        return Math.Max(_options.CallDurationSeconds, 10);
    }
}
