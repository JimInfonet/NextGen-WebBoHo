namespace InfonetCSCR.Web.Models
{
  public class TankDipViewModel
  {
    public int TankID { get; set; } = default!;
    public decimal? DipValue { get; set; }
    public int? Grade { get; set; }
    public DateTime DipDate { get; set; }
  }
}
