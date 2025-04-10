namespace InfonetCSCR.Core.Dtos;

public class ExcelReportDataDto
{
  public Company? Company { get; set; }
  public string? FuelDept { get; set; }
  public IList<Grade>? Grades { get; set; }
  public IList<SaleHeadAndSaleLine>? SaleHeadAndSaleLinesByStockCodes { get; set; }
  public IList<SaleHeadAndSaleLine>? SaleHeadAndSaleLinesByGradeIds { get; set; }
  public IList<FuelPriceHistory>? FuelPriceHistories { get; set; }
  public decimal TotalDiscount { get; set; }
}
