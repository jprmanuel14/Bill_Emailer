using System.Threading;
using System.Threading.Tasks;

namespace BillingMail.Plugin.Services;

/// <summary>
/// Core workflow processor interface for polling pending emails, generating statements, and dispatching.
/// </summary>
public interface IBillingMailProcessor
{
    /// <summary>
    /// Executes one iteration of pending email processing.
    /// </summary>
    Task ProcessPendingMailsAsync(CancellationToken cancellationToken = default);
}
