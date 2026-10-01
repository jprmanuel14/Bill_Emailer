using System.Threading;
using System.Threading.Tasks;

namespace BillingMail.Plugin.Services;

/// <summary>
/// Cross-platform mail transport interface using MailKit.
/// </summary>
public interface IEmailDispatcher
{
    /// <summary>
    /// Dispatches an email message asynchronously.
    /// </summary>
    Task SendEmailAsync(string toAddress, string? ccAddresses, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
