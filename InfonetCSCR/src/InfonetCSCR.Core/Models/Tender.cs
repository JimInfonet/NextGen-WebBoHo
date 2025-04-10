namespace InfonetCSCR.Core.Models;

public class Tender
{
  public string Code { get; set; } = default!;
  public string? ShortName { get; set; }
  public string? FullName { get; set; }
  public string? Class { get; set; }
  public decimal? ExchangeRate { get; set; }
  public byte? Sequence { get; set; }
  public int? CardCode { get; set; }
  public bool? ExactChange { get; set; }
  public bool? GiveChange { get; set; }
  public bool? AcceptAsPayment { get; set; }
  public bool? GiveAsRefund { get; set; }
  public bool? CanAdjust { get; set; }
  public bool? Bankable { get; set; }
  public bool? IncludeInClose { get; set; }
  public bool? PutInDrawer { get; set; }
  public bool? Signature { get; set; }
  public bool? Inactive { get; set; }
  public decimal? MinAmount { get; set; }
  public decimal? MaxAmount { get; set; }
  public decimal? SmallestUnit { get; set; }
  public double? PrintCopies { get; set; }
  public byte? Instances { get; set; }
  public string? PayoutCode { get; set; }
  public string? AcctCode { get; set; }
}
