namespace InfonetCSCR.Web.Models;

public class ReceiveDataRequestViewModel
{
  public int Purpose { get; set; }
  public int Length { get; set; }
  public int Start { get; set; }
  public string? SearchValue { get; set; }
  public string? SearchColumnName { get; set; }
  public string? StockCode { get; set; }
}
