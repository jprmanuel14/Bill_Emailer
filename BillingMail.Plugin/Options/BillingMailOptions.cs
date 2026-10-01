namespace BillingMail.Plugin.Options;

/// <summary>
/// Strongly-typed configuration options for Billing Mail plugin.
/// </summary>
public class BillingMailOptions
{
    public const string SectionName = "BillingMail";

    /// <summary>
    /// Database Provider: "PostgreSQL" or "SqlServer" (Default: "PostgreSQL").
    /// </summary>
    public string DatabaseProvider { get; set; } = "PostgreSQL";

    /// <summary>
    /// Connection string to the BCAT database (PostgreSQL or SQL Server).
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Call type trigger mode:
    /// 1 = Daily fixed time execution (via StartTime)
    /// 2 = Recurring interval execution (via CallDurationSeconds)
    /// </summary>
    public int CallType { get; set; } = 2;

    /// <summary>
    /// Interval duration in seconds when CallType = 2 (Default: 900 seconds = 15 mins).
    /// </summary>
    public int CallDurationSeconds { get; set; } = 900;

    /// <summary>
    /// Daily execution time string when CallType = 1 (e.g., "05:00 PM").
    /// </summary>
    public string StartTime { get; set; } = "05:00 PM";

    /// <summary>
    /// SMTP Relay Host address.
    /// </summary>
    public string SmtpHost { get; set; } = "10.139.108.22";

    /// <summary>
    /// SMTP Relay Port (Default: 25).
    /// </summary>
    public int SmtpPort { get; set; } = 25;

    /// <summary>
    /// Enable SSL/TLS for SMTP connection.
    /// </summary>
    public bool EnableSsl { get; set; } = false;

    /// <summary>
    /// Sender email address (From).
    /// </summary>
    public string SenderEmail { get; set; } = "no-reply@billcollectiontool.ph.pwc.com";

    /// <summary>
    /// Optional sender display name.
    /// </summary>
    public string SenderName { get; set; } = "PwC Billing Collections";

    /// <summary>
    /// Support / Contact email address displayed in HTML body footer.
    /// </summary>
    public string SupportEmail { get; set; } = "ph_pwc_collections@pwc.com";

    /// <summary>
    /// When set, emails are saved as .eml files in this directory instead of being sent via SMTP.
    /// Useful for testing without an actual SMTP server.
    /// </summary>
    public string? EmailCapturePath { get; set; }
}
