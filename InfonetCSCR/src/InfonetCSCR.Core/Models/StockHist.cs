namespace InfonetCSCR.Core.Models;

public class StockHist
{
  public string? UserId { get; set; }
  public DateTime? BatchDate { get; set; }
  public int? Batchid { get; set; }
  public string? StockCode { get; set; }
  public string? Description { get; set; }
  public int? Quantity { get; set; }
  public int? BookQty { get; set; }
  public decimal? Purpose { get; set; }
  public string? InvNum { get; set; }
  public decimal? Cost { get; set; }
  public decimal? Amount { get; set; }
  public decimal? Assoc { get; set; }
  public decimal? Taxes { get; set; }
  public decimal? Total { get; set; }
  public DateTime? DateCount { get; set; }
  public decimal? InitialCount { get; set; }
  public int? Sequence { get; set; }
}

