namespace InfonetCSCR.Reporting.Models
{
  public class ElectronicJournalReportData
  {
    public int SaleNo { get; set; } = default!;
    public int? Till { get; set; }
    public int? ID { get; set; }
    public string? ShiftDate { get; set; }
    public string? SaleDate { get; set; }
    public string? SaleTime { get; set; }
    public string? Type { get; set; }
    public double? Qty { get; set; }
    public string? Customer { get; set; }
    public string? Cashier { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public double? Disc { get; set; }
    public double? Price { get; set; }
    public double? Amount { get; set; }
    public int? PumpID { get; set; }
    public int? DelSeq { get; set; }
    public string? StoreID { get; set; }
  }
}