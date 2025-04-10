using System;

namespace InfonetCSCR.Reporting.Models
{
  public class StockTakesAndReceivingReportDto
  {
    public int? RecNum { get; set; }
    public DateTime? RecDate { get; set; }
    public decimal? InvNum { get; set; }
    public string? StockCode { get; set; }
    public string? Descript { get; set; }
    public decimal? IQtyRecv { get; set; }
    public decimal? Cost { get; set; }
    public decimal? Assoc { get; set; }
    public decimal? Taxes { get; set; }
    public decimal? Total { get; set; }
    public int? Sequence { get; set; }
    public decimal? SystemQuantity { get; set; }
    public decimal? Quantity { get; set; }
    public string? StockType { get; set; }
    public decimal? StockCost { get; set; }
    public decimal? InStock { get; set; }
    public string? KitItemStockCode { get; set; }
    public string? GroupingColumn { get; set; }
    public int? GroupCount { get; set; }
    public decimal? QuantityDifference { get; set; }
    public decimal? QuantityDifferenceInDollar { get; set; }
  }
}
