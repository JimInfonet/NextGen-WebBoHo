namespace InfonetCSCR.Core.Dtos;

public class CustomerCardDeleteDto
{
  public string CustomerCardNumber { get; set; }
  public bool? IsSyncCustInfo { get; set; }
  public bool? IsArCustomer { get; set; }
}
