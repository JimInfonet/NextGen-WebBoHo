namespace InfonetCSCR.Web.Models;

public class ProductClassificationTaxViewModel
{
  public IList<TaxNameAndCodeViewModel>? TaxNameAndCodes { get; set; }
  public IList<string>? StockCodes { get; set; }
}
