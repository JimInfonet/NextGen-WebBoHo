namespace InfonetCSCR.Core.Dtos;

public class PurchasingAndReceivingPoliciesDto
{
  public bool Purchase { get; set; }
  public bool KitsInPoPolicy { get; set; }
  public int CostDecPolicy { get; set; }
  public bool PackCostPolicy { get; set; }
  public string ExptTaxPolicy { get; set; }
  public bool UpdateAllVendor { get; set; }
  public bool SetVend { get; set; }
  public bool PChangeInPO { get; set; }
  public bool ConfirmPC { get; set; }
  public bool DefaultMargin { get; set; }
}
