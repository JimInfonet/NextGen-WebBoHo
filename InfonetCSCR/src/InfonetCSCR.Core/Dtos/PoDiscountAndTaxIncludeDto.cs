namespace InfonetCSCR.Core.Dtos;

public class PoDiscountAndTaxIncludeDto
{
  public string VendorId { get; set; }
  public double PoDiscount { get; set; }
  public bool PoCostTaxInclude { get; set; }
}
