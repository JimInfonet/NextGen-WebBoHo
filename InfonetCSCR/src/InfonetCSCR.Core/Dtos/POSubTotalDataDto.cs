namespace InfonetCSCR.Core.Dtos;

public class POSubTotalDataDto
{
  public long PurchaseNo { get; set; }
  public double SubTotal { get; set; }
  public double ExtDiscount { get; set; }
  public double TotalCharges { get; set; }
  public double GSTTotal { get; set; }
}
