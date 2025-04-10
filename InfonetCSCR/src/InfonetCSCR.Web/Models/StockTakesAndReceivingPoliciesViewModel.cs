namespace InfonetCSCR.Web.Models;

public class StockTakesAndReceivingPoliciesViewModel
{
  public bool AutoInv { get; set; }
  public bool SetVendor { get; set; }
  public string HndheldFiles { get; set; } = default!;
  public string ExceptedTax { get; set; } = default!;
  public int CostDec { get; set; }
  public bool DefMargin { get; set; }
  public bool ConfirmPc { get; set; }
  public bool UpdateNetwork { get; set; }
  public bool IsMarginCalcEqualsCostMkup { get; set; }
  public int NumPrice { get; set; }
  public string CostType { get; set; } = default!;
}
