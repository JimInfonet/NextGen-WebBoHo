namespace InfonetCSCR.Web.Models;

public class StocktakesAndReceivingResultViewModel
{
  public int totalItems { get; set; }
  public IEnumerable<ReceiveViewModel>? receiveData { get; set; }
}
