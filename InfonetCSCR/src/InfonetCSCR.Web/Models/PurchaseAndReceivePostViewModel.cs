namespace InfonetCSCR.Web.Models;

public class PurchaseAndReceivePostViewModel
{
  public long? PurchaseNo { get; set; }
  public int? ShiftID { get; set; }
  public int? ShiftNumber { get; set; }
  public decimal? SubTotal { get; set; }
  public decimal? Discount { get; set; }
  public decimal? GSTAmount { get; set; }
  public decimal? Total { get; set; }
  public string? PaymentType { get; set; }
  public bool? IsPaid { get; set; }
  public string? UserCode { get; set; }
  public bool? CostChange { get; set; }
  public PurchasingAndReceivingPoliciesViewModel? PurchasingAndReceivingPolicies { get; set; }
}