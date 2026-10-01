using System;

namespace BillingMail.Plugin.Models;

/// <summary>
/// Represents a transactional billing record from [tbBCATData].
/// </summary>
public class StatementLineItem
{
    public DateTime TransDate { get; set; }
    public string BillNo { get; set; } = string.Empty;
    public string ReferenceNo { get; set; } = string.Empty;
    public decimal Age { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Dollar { get; set; }
    public decimal Peso { get; set; }
}
