using System;

namespace BillingMail.Plugin.Models;

/// <summary>
/// Represents a scheduled mail record queued in [tblScheduledMail].
/// </summary>
public class ScheduledMailItem
{
    public int Id { get; set; }
    public string ClientCode { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public DateTime StatementDate { get; set; }
    public DateTime ScheduledDateTime { get; set; }
    public DateTime? SentDate { get; set; }
    public string? EntityCode { get; set; }
    public string ContactMails { get; set; } = string.Empty;
    public string? CCMails { get; set; }
}
