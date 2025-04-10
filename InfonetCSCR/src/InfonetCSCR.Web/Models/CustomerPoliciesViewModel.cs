namespace InfonetCSCR.Web.Models;

public class CustomerPoliciesViewModel
{
  public bool CredTerm { get; set; }
  public string? CustMem { get; set; }
  public bool EnableBandAccT { get; set; }
  public bool FuelRebate { get; set; }
  public bool RSTRProfile { get; set; }
  public bool SyncCustInfo { get; set; }
  public bool TaxExempt { get; set; }
  public string? TEType { get; set; }
  public bool TEPayPump { get; set; }
  public bool UCLStatus{ get; set; }
  public bool UseDataCard { get; set; }
  public bool UseKickBack { get; set; }
  public bool CashBonus { get; set; }
  public bool UseARInvoices { get; set; }
  public bool TimeSheets { get; set; }
  public bool HaveUserId { get; set; }
  public bool UCHGPass { get; set; }
}
