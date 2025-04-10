namespace InfonetCSCR.Core.Models;

public class Grade
{
  public string? StoreId { get; set; }
  public int? Id { get; set; }
  public string? ShortName { get; set; }
  public string? FullName { get; set; }
  public string? StockCode { get; set; }
  public string? SubDept { get; set; }
  public int? Ratio { get; set; }
  public char? FeedStock { get; set; }
  public double? Price_diff { get; set; }
  public int? StockGradeID { get; set; }
  public string FuelType { get; set; } = default!;
}
