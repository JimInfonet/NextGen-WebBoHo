namespace InfonetCSCR.Core.Dtos;

public class StockItemDetailsDto
{
  public int TotalActiveTaxes { get; set; }
  public string? StockItemName { get; set; }
  public int TotalActiveCharges { get; set; }
  public IEnumerable<ChargesAndDeposits>? Charges { get; set; }
  public IEnumerable<Tax>? Taxes { get; set; }
  public List<StockAvailabilityByDay>? AvailabilityByDays { get; set; }
}
