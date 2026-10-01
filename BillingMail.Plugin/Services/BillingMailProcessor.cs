using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BillingMail.Plugin.Services;

public class BillingMailProcessor : IBillingMailProcessor
{
    private readonly IBillingRepository _repository;
    private readonly IStatementEmailBuilder _emailBuilder;
    private readonly IEmailDispatcher _emailDispatcher;
    private readonly ILogger<BillingMailProcessor> _logger;

    public BillingMailProcessor(
        IBillingRepository repository,
        IStatementEmailBuilder emailBuilder,
        IEmailDispatcher emailDispatcher,
        ILogger<BillingMailProcessor> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _emailBuilder = emailBuilder ?? throw new ArgumentNullException(nameof(emailBuilder));
        _emailDispatcher = emailDispatcher ?? throw new ArgumentNullException(nameof(emailDispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessPendingMailsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting scheduled billing email processing batch...");

        var successCount = 0;
        var errorCount = 0;
        var pendingCount = 0;

        try
        {
            var pendingMails = await _repository.GetPendingScheduledMailsAsync(cancellationToken);
            pendingCount = pendingMails.Count();
            _logger.LogInformation("Found {PendingCount} pending scheduled mail(s) to process", pendingCount);

            foreach (var mailItem in pendingMails)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Email processing cancelled by host request.");
                    break;
                }

                try
                {
                    _logger.LogInformation("Processing statement email for Client: {ClientCode} ({ClientName})", mailItem.ClientCode, mailItem.ClientName);

                    var (subject, htmlBody) = await _emailBuilder.BuildStatementEmailAsync(mailItem, cancellationToken);
                    
                    await _emailDispatcher.SendEmailAsync(mailItem.ContactMails, mailItem.CCMails, subject, htmlBody, cancellationToken);

                    await _repository.MarkMailAsSentAsync(mailItem.ClientCode, mailItem.StatementDate, cancellationToken);

                    _logger.LogInformation("Successfully processed and marked as sent: {ClientCode}", mailItem.ClientCode);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing statement email for Client: {ClientCode}", mailItem.ClientCode);
                    errorCount++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical failure during pending email query or batch processing.");
        }

        _logger.LogInformation("Batch summary: Pending={PendingCount}, Sent={SuccessCount}, Errors={ErrorCount}", pendingCount, successCount, errorCount);
        _logger.LogInformation("Finished scheduled billing email processing batch.");
    }
}
