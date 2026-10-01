namespace BillingMail.Plugin.Models;

/// <summary>
/// Represents a distinct practice entity and bank mapping for a client statement.
/// </summary>
public class PracticeEntity
{
    public string EntityCode { get; set; } = string.Empty;
    public string vcPracticeName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public int? ReportHierarchy { get; set; }
}
