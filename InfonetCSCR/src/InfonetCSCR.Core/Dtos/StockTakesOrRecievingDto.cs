namespace InfonetCSCR.Core.Dtos;

public class StockTakesAndRecievingDto
{
  public string? Vendor { get; set; }
  public string? Department { get; set; }
  public string? SubDepartment { get; set; }
  public string? SubDetail { get; set; }
  public int SelectedMonth { get; set; }
  public bool SellInactivePolicy { get; set; }
  public short BatchId { get; set; }
  public decimal? Invoice { get; set; }
  public int Purpose { get; set; }
}
