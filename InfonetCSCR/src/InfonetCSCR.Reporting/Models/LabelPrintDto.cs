namespace InfonetCSCR.Reporting.Models
{
  public class LabelPrintDto
  {
    public string StockCode { get; set; } = default!;
    public string? Description { get; set; }
    public double Price { get; set; }
    public string? PartNo { get; set; }
    public char? StockType { get; set; }
  }
}