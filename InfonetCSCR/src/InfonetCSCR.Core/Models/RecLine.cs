namespace InfonetCSCR.Core.Models;

public class RecLine
{
  public short? RecNum { get; set; }
  public string? Branch { get; set; }
  public string? InvNum { get; set; }
  public string? StockCode { get; set; }
  public string? Descript { get; set; }
  public decimal? Quantity { get; set; }
  public decimal? Cost { get; set; }
  public decimal? Amount { get; set; }
  public decimal? Assoc { get; set; }
  public decimal? Taxes { get; set; }
  public decimal? Total { get; set; }
  public decimal? SysQty { get; set; }
  public bool? RecUpdate { get; set; }
  public decimal? Purpose { get; set; }
  public bool? Print { get; set; }
  public string? ActiveVendorCode { get; set; }
  public decimal? InitialCount { get; set; }
  public DateTime? DateCount { get; set; }
  public int? Sequence { get; set; }
}
