namespace InfonetCSCR.Web.Models;

public class TillViewModel
{
  public int TillNum { get; set; } = default!;
  public bool Active { get; set; } = default!;
  public bool Processing {  get; set; } = default!;
  public decimal? Float { get; set; }
  public decimal? Cash { get; set; }
  public decimal? CashBonusFloat { get; set; }
  public decimal? CashBonus { get; set; }
  public DateTime? OpenDate { get; set; }
  public DateTime? OpenTime { get; set; }
  public DateTime? ShiftDate { get; set; }
  public int? Shift { get; set; }
  public int? POSID { get; set; }
  public string? UserLoggedOn { get; set; }
}
