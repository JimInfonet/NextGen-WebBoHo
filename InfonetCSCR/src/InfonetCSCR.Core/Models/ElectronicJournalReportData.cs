namespace InfonetCSCR.Core.Models;

public class ElectronicJournalReportData
{
  public int SaleNo { get; set; } = default!;
  public int? Till { get; set; } = default!;
  public int? ID { get; set; } = default!;
  public string? ShiftDate { get; set; }
  public string? SaleDate { get; set; }
  public string? Type { get; set; }
  public double? Qty { get; set; }
  public string? Customer { get; set; }
  public string? Cashier { get; set; }
  public string? SaleTime { get; set; }
  public string? Code { get; set; }
  public string? Description { get; set; }
  public double? Disc { get; set; }
  public double? Price { get; set; }
  public double? Amount { get; set; }
  public DateTime SDT { get; set; }
  public int? Source { get; set; }
  public string? PumpID { get; set; }
  public int? DelSeq { get; set; }
  public string? StoreID { get; set; }
}
