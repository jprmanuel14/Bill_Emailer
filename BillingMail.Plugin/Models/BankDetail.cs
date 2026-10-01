namespace BillingMail.Plugin.Models;

/// <summary>
/// Represents bank account instructions for remittance from [Banks].
/// </summary>
public class BankDetail
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string SwiftCode { get; set; } = string.Empty;
}
