namespace InfonetCSCR.Web.Models;

public class ExcelReportDataDtoViewModel
{
  public CompanyViewModel? Company { get; set; }
  public string? FuelDept { get; set; }
  public IList<GradeViewModel>? Grades { get; set; }
  public IList<SaleHeadAndSaleLineViewModel>? SaleHeadAndSaleLinesByStockCodes { get; set; }
  public IList<SaleHeadAndSaleLineViewModel>? SaleHeadAndSaleLinesByGradeIds { get; set; }
  public IList<FuelPriceHistoryViewModel>? FuelPriceHistories { get; set; }
  public decimal TotalDiscount { get; set; }
}
