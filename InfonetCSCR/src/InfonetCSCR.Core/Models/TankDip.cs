namespace InfonetCSCR.Core.Models;

public class TankDip
{
  public int TankID { get; set; } = default!;
  public decimal? DipValue { get; set; }
  public int? Grade { get; set; }
  public DateTime DipDate { get; set; }
}
