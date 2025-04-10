namespace InfonetCSCR.Core.Models;

public class StockTakesAndReceivingResult
{
  public int totalItems { get; set; }
  public IEnumerable<StockTakesAndReceiveDto>? receiveData { get; set; }
}
