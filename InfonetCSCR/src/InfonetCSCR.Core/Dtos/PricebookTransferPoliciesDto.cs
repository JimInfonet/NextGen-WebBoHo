namespace InfonetCSCR.Core.Dtos;

public class PricebookTransferPoliciesDto
{
  public int NumberOfPrice { get; set; }
  public bool MarginCalcCostMkup { get; set; }
  public bool SetVend { get; set; }
  public bool UpdateAllVendor { get; set; }
  public bool DefMargin { get; set; }
  public bool ConfirmPc { get; set; }
  public bool PackCost { get; set; }
}
