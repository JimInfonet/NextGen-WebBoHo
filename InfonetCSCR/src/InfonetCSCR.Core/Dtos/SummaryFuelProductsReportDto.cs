namespace InfonetCSCR.Core.Dtos;

public class SummaryFuelProductsReportDto
{
  public int? Month { get; set; }
  public int? Year { get; set; }
  public string? StockCode { get; set; }
  public string? Description { get; set; }
  public Decimal? Quantity { get; set; }
  public Decimal? Amount { get; set; }
  public Decimal? Rebate { get; set; }
  public Decimal? SalesLineIncludedTax { get; set; }
  public Decimal? TaxCreditLineIncludedTax { get; set; }
  public string? GroupID { get; set; }
  public bool? TaxExempt { get; set; }
  public string SalesLineTaxName { get; set; } = default!;
  public string TaxCreditLineTaxName { get; set; } = default!;
}
