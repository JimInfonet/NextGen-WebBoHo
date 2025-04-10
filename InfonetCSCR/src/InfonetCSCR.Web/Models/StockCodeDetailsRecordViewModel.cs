namespace InfonetCSCR.Web.Models;

public class StockCodeDetailsRecordViewModel
{
  public string? StockCode { get; set; }
  public string? Department { get; set; }
  public Decimal? Status { get; set; }
  public Decimal? NonStatus { get; set; }
  public Decimal? Member { get; set; }
  public Decimal? NonMember { get; set; }
  public DateTime? ShiftDate { get; set; }
  public Decimal? MemberExemptedTax { get; set; }
  public Decimal? NonMemberExemptedTax { get; set; }
  public Decimal? StatusExemptedTax { get; set; }
  public Decimal? NonStatusVolume { get; set; }
  public Decimal? NonStatusCost { get; set; }
  public Decimal? StatusVolume { get; set; }
  public Decimal? StatusCost { get; set; }
  public Decimal? NonMemberVolume { get; set; }
  public Decimal? NonMemberCost { get; set; }
  public Decimal? MemberVolume { get; set; }
  public Decimal? MemberCost { get; set; }
}
