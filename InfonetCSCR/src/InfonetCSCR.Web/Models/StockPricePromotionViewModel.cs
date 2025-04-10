namespace InfonetCSCR.Web.Models;

public class StockPricePromotionViewModel
{
  public string? PromotionId { get; set; }
  public string? StockCode { get; set; }
  public string? Dept { get; set; }
  public string? SubDept { get; set; }
  public string? SubDetail { get; set; }
  public DateTime? EndDate { get; set; }
}
