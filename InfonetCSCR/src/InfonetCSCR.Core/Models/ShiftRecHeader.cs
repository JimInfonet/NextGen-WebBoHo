namespace InfonetCSCR.Core.Models;

public class ShiftRecHeader
{
  public int ShiftID { get; set; }
  public DateTime? ShiftDate { get; set; }
  public DateTime? Start { get; set; }
  public DateTime? End { get; set; }
  public DateTime? Created { get; set; }
  public int Shift { get; set; }
  public decimal? Gas { get; set; }
  public decimal? CStore { get; set; }
  public decimal? CarWash { get; set; }
  public decimal? MiscSales1 { get; set; }
  public decimal? MiscSales2 { get; set; }
  public decimal? TotalStore { get; set; }
  public decimal? CreditCard { get; set; }
  public decimal? AccountCharge { get; set; }
  public decimal? AccountPayment { get; set; }
  public decimal? PaidOuts { get; set; }
  public decimal? GC { get; set; }
  public decimal? CarWashCoupons { get; set; }
  public decimal? VendorCoupons { get; set; }
  public string? DescMiscAmount { get; set; }
  public decimal? MiscAmount { get; set; }
  public decimal? AccountFor { get; set; }
  public decimal? CashOnHand { get; set; }
  public decimal? ShortOver { get; set; }
  public bool? Closed { get; set; }
  public string? EmployeeName { get; set; }
  public DateTime? BCDate { get; set; }
  public decimal? DriveOff { get; set; }
  public decimal? ProductShortOver { get; set; }
  public decimal? Shortage { get; set; }
  public decimal? CumulativShortage { get; set; }
  public decimal? NetCumulative { get; set; }
  public decimal? GasPatP { get; set; }
  public decimal? CCPatP { get; set; }
  public bool? Reposted { get; set; }
  public string? ConfirmR { get; set; }
  public decimal? GasAdj { get; set; }
  public int? Till { get; set; }
}
