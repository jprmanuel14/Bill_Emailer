using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BillingMail.Plugin.Models;

namespace BillingMail.Plugin.Services;

/// <summary>
/// Asynchronous database repository interface for billing mail queries.
/// </summary>
public interface IBillingRepository
{
    /// <summary>
    /// Gets pending scheduled emails that are ready to be sent and not excluded.
    /// </summary>
    Task<IEnumerable<ScheduledMailItem>> GetPendingScheduledMailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets distinct practice entities for a client statement.
    /// </summary>
    Task<IEnumerable<PracticeEntity>> GetPracticeEntitiesAsync(string clientId, DateTime statementDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets billing statement line items for a client and practice entity.
    /// </summary>
    Task<IEnumerable<StatementLineItem>> GetStatementLineItemsAsync(string clientId, DateTime statementDate, string practiceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets bank account remittance details for an entity code.
    /// </summary>
    Task<IEnumerable<BankDetail>> GetBankDetailsAsync(string accountCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a scheduled email item as sent.
    /// </summary>
    Task MarkMailAsSentAsync(string clientId, DateTime statementDate, CancellationToken cancellationToken = default);
}
