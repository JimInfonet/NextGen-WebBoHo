namespace InfonetCSCR.Web.Models;

public class StockPricePoliciesViewModel
{
  public int NumberOfPrice { get; set; }
  public string? CostType { get; set; }
  public bool UseTaxExempt { get; set; }
  public bool PackCost { get; set; }
  public int PriceDec { get; set; }
  public int CostDec { get; set; }
  public bool SetVendor { get; set; }
  public bool MarginCalcCostMkup { get; set; }
  public bool AllowNegativePrice { get; set; }
  public bool DefMargin { get; set; }
  public bool DefPrice { get; set; }
  public bool Warn0Price { get; set; }
  public bool ConfirmPriceChange { get; set; }
  public bool UpdateNetWork { get; set; }
  public bool UpdateAllStores { get; set; }
  public bool UseArinovices { get; set; }
  public bool UpdateAll { get; set; }
  public bool UpdVends { get; set; }
  public bool UserAccessStockCode { get; set; }
  public bool ForceCost { get; set; }
}
