namespace InfonetCSCR.Web.Models;

public class PromotionTransferViewModel
{
  public int BatchId { get; set; }  
  public string PromoId { get; set; }
  public string? Description { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set;}
  public string StoreId { get; set; }
  public DateTime? TransDate { get; set; }
  public string? TransUser { get; set; }
  public string? TransFlag { get; set; }
  public string? TransType { get; set; }
}
