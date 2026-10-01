using System.Threading;
using System.Threading.Tasks;
using BillingMail.Plugin.Models;

namespace BillingMail.Plugin.Services;

/// <summary>
/// Interface for building HTML email body and subject line for client statements.
/// </summary>
public interface IStatementEmailBuilder
{
    /// <summary>
    /// Generates the HTML statement email body and calculates email metadata.
    /// </summary>
    Task<(string Subject, string HtmlBody)> BuildStatementEmailAsync(ScheduledMailItem mailItem, CancellationToken cancellationToken = default);
}
