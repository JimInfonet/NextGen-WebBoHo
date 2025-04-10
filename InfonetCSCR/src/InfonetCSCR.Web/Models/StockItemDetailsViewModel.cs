namespace InfonetCSCR.Web.Models;

public class StockItemDetailsViewModel
{
  public int TotalActiveTaxes { get; set; }
  public int TotalActiveCharges { get; set; }
  public string? StockItemName { get; set; }
  public IEnumerable<ChargesAndDepositsViewModel>? Charges { get; set; }
  public IEnumerable<TaxViewModel>? Taxes { get; set; }
  public IEnumerable<StockAvailabilityByDayViewModel>? AvailabilityByDays { get; set; }
}
