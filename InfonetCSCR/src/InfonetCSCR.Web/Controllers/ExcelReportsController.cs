namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class ExcelReportsController : Controller
{
  private readonly IPolicyService _policyService;
  private readonly IExcelReportsService _excelReportsService;

  #region Private Properties

  private ExcelReportsPolicyRecordViewModel ExcelReportsPolicies
  {
    get
    {
      return new ExcelReportsPolicyRecordViewModel
      {
        IsTaxExempt = _policyService.GetPolicy(PolicyName.TAX_EXEMPT),
        TaxExemptType = _policyService.GetPolicy(PolicyName.TE_Type),
        MemberNo = _policyService.GetPolicy(PolicyName.MEMBER_IDENTITY),
        TEByRate = _policyService.GetPolicy(PolicyName.TE_ByRate), 
        Version = _policyService.GetPolicy(PolicyName.VERSION)
      };
    }
  }

  #endregion

  public ExcelReportsController(IExcelReportsService excelReportsService, IPolicyService policyService)
  {
    _excelReportsService = excelReportsService;
    _policyService = policyService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult GetExcelReportsPolicies()
  {
    return Ok(ExcelReportsPolicies);
  }

  public async Task<IActionResult> CheckSaleDataExists([FromBody] ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    excelReportGeneratorViewModel.MemberNo = ExcelReportsPolicies.MemberNo;
    excelReportGeneratorViewModel.VersionPolicy = ExcelReportsPolicies.Version;
    bool? isSaleDataExists = await _excelReportsService.CheckSaleDataExists(excelReportGeneratorViewModel);
    return Ok(isSaleDataExists);
  }

  public async Task<IActionResult> GenerateExcelReport(ExcelReportGeneratorViewModel excelReportGeneratorViewModel)
  {
    string teType = ExcelReportsPolicies.TaxExemptType;
    bool teByRate = ExcelReportsPolicies.TEByRate;

    if (teType == "SITE" && teByRate)
    {
      excelReportGeneratorViewModel.TaxExemptType = "BITE";
    } 
    else
    {
      excelReportGeneratorViewModel.TaxExemptType = teType;
    }

    TempData["ReportGenerated"] = false;
    FileBase64DataViewModel? fileBase64DataViewModel = new();
    string? fileName = "";
    int? reportNo = excelReportGeneratorViewModel.ReportNo;
    excelReportGeneratorViewModel.MemberNo = ExcelReportsPolicies.MemberNo;

    if (reportNo == (int)ExcelReportNo.FuelDailySalesReport)
    {
      fileBase64DataViewModel = await _excelReportsService.FuelDailySalesReport(excelReportGeneratorViewModel);
      fileName = "FuelDailySalesReport.xlsx";
    }
    else if (reportNo == (int)ExcelReportNo.FuelLitresSummaryReport || 
             reportNo == (int)ExcelReportNo.FuelDollarsSummary || 
             reportNo == (int)ExcelReportNo.DepartmentalSalesSummary || 
             reportNo == (int)ExcelReportNo.SalesSummaryFamilyGroup || 
             reportNo == (int)ExcelReportNo.salesDetailFamilyGroup) 
    {
      fileBase64DataViewModel = await _excelReportsService.FLOrFDOrDSSummaryReport(excelReportGeneratorViewModel);

      if (reportNo == (int)ExcelReportNo.FuelLitresSummaryReport)
      {
        fileName = "FuelLitresSummaryReport.xlsx";
      }
      else if (reportNo == (int)ExcelReportNo.FuelDollarsSummary)
      {
        fileName = "FuelDollarsSummaryReport.xlsx";
      }
      else if (reportNo == (int)ExcelReportNo.DepartmentalSalesSummary)
      {
        fileName = "DepartmentalSalesSummaryReport.xlsx";
      }
      else if (reportNo == (int)ExcelReportNo.SalesSummaryFamilyGroup)
      {
        fileName = "SalesSummaryFamilyGroupReport.xlsx";
      }
      else
      {
        fileName = "SalesDetailsFamilyGroupReport.xlsx";
      }
    }
    else if (reportNo == (int)ExcelReportNo.SalesSummaryFuelProducts)
    {
      fileBase64DataViewModel = await _excelReportsService.SalesSummaryFuelProductsReport(excelReportGeneratorViewModel);
      fileName = "SalesSummaryFuelProductsReport.xlsx";
    }
    else if (reportNo == (int)ExcelReportNo.North60FleetCardReport || reportNo == (int)ExcelReportNo.YtgFleetCardReport)
    {
      string today = DateTime.Now.ToString("MMMddyyyy");
      fileBase64DataViewModel = await _excelReportsService.North60OrYTGFleetCardReport(excelReportGeneratorViewModel);
      fileName = (reportNo == (int)ExcelReportNo.North60FleetCardReport ? $"North60FleetCardReport_{today}.xlsx" : $"YTGFleetCardReport_{today}.xlsx");
    }
    else if (reportNo == (int)ExcelReportNo.SalesByHourReport)
    {
      fileBase64DataViewModel = await _excelReportsService.SalesByHourReport(excelReportGeneratorViewModel);
      fileName = "SalesByHourReport.xlsx";
    }
    else if (reportNo == (int)ExcelReportNo.FngtrFuelSalesReport)
    {
      fileBase64DataViewModel = await _excelReportsService.FNGTRFuelSalesReport(excelReportGeneratorViewModel);
      fileName = "FNGTRFuelSalesReport.xlsx";
    }

    TempData["ReportGenerated"] = true;
    return File(fileBase64DataViewModel.ByteArray, "application/vnd.ms-excel", fileName);
  }

  public async Task<IActionResult> CheckReportDownloadStarted()
  {
    bool isStarted = TempData["ReportGenerated"] != null && (bool)TempData["ReportGenerated"];
    return Ok(isStarted);
  }

  public async Task<IActionResult> ConvertTextReportToExcel(TextReportParamsViewModel textReportParams)
  {
    var report = await _excelReportsService.ConvertTextReportToExcelAsync(textReportParams);
    return Ok(Convert.ToBase64String(report.ByteArray));
  }
}
