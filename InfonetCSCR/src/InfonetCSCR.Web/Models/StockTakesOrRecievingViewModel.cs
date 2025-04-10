namespace InfonetCSCR.Web.Models;

public class StockTakesAndRecievingViewModel
{
  public string? Vendor { get; set; }
  public string? Department { get; set; }
  public string? SubDepartment { get; set; }
  public string? SubDetail { get; set; }
  public int SelectedMonth { get; set; }
  public bool SellInactivePolicy { get; set; }
  public int Purpose { get; set; }
  public decimal? Invoice { get; set; }
}
