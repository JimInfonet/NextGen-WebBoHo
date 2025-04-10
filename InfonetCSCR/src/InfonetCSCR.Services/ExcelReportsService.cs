using InfonetCSCR.Core.Models;

namespace InfonetCSCR.Services;

public class ExcelReportsService : IExcelReportsService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _applicationConfig;

  public ExcelReportsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCTransConnectionString);
    this._applicationConfig = applicationConfig;
  }

  public async Task<bool> CheckSaleDataExists(ExcelReportGenerator excelReportGenerator)
  {
    IUnitOfWork uomMaster = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
    bool isOk = false;
    string? fuelDept = await uomMaster.Fuels.GetFuelDepartmentAsync();

    if (excelReportGenerator.ReportNo == 9)
    {
      DateTime shiftDate = excelReportGenerator.SelectedDate ?? DateTime.Now;
      IList<SaleHeadAndSaleLine>? saleHeadAndSaleLines = await _unitOfWork.SaleHeads.GetSaleData(shiftDate, fuelDept);

      isOk = saleHeadAndSaleLines != null && saleHeadAndSaleLines.Any();
    }
    else if (excelReportGenerator.ReportNo == 1 || excelReportGenerator.ReportNo == 2)
    {
      await _unitOfWork.Sales.ClearSaleDataAsync();

      excelReportGenerator.Department = await uomMaster.Fuels.GetFuelDepartmentAsync();

      await _unitOfWork.Sales.CreateFuelNonStatusDataAsync(excelReportGenerator);
      await _unitOfWork.Sales.CreateFuelStatusDataAsync(excelReportGenerator);
      isOk = await _unitOfWork.Sales.IsSaleDataExistsAsync();
    }
    else if (excelReportGenerator.ReportNo == 3)
    {
      await _unitOfWork.Sales.ClearSaleDataAsync();
      await _unitOfWork.Sales.CreateSaleNonStatusDataAsync(excelReportGenerator);
      await _unitOfWork.Sales.CreateSaleStatusDataAsync(excelReportGenerator);
      isOk = await _unitOfWork.Sales.IsSaleDataExistsAsync();
    }
    else if (excelReportGenerator.ReportNo == 4 || excelReportGenerator.ReportNo == 5)
    {
      await _unitOfWork.Sales.ClearSaleDataAsync();
      await _unitOfWork.Sales.CreateSaleNonStatusDataByFamilyGroupAsync(excelReportGenerator);
      await _unitOfWork.Sales.CreateSaleStatusDataByFamilyGroupAsync(excelReportGenerator);
      isOk = await _unitOfWork.Sales.IsSaleDataExistsAsync();
    }
    else if (excelReportGenerator.ReportNo == 6)
    {
      IList<SummaryFuelProductsReportDto> saleData = await _unitOfWork.ExcelReports.GetSummaryFuelProductsReportDataAsync(excelReportGenerator);
      _unitOfWork.Commit();
      isOk = saleData.Any();
    }
    else if (excelReportGenerator.ReportNo == 7 || excelReportGenerator.ReportNo == 8)
    {
      bool isYTG = (excelReportGenerator.ReportNo == 7 ? false : true);
      excelReportGenerator.Department = fuelDept;
      IList<FleetCardReportDto> data = await _unitOfWork.ExcelReports.GetNorth60OrYTGFleetCardReportDataAsync(excelReportGenerator, isYTG);
      _unitOfWork.Commit();
      isOk = data.Any();
    }
    else if (excelReportGenerator.ReportNo == 10)
    {
      IList<SaleHead>? data = await _unitOfWork.SaleHeads.GetSalesByHourReportAsync(excelReportGenerator);
      _unitOfWork.Commit();
      isOk = data.Any();
    }
    else if (excelReportGenerator.ReportNo == 11)
    {
      await _unitOfWork.ExcelReports.CreateFNGTRSalesReportDataAsync(excelReportGenerator);
      IList<ReportData>? reportData = await _unitOfWork.ExcelReports.GetAllReportDataAsync();
      _unitOfWork.Commit();
      isOk = reportData.Any();
    }

    uomMaster.Commit();
    _unitOfWork.Commit();

    return isOk;
  }

  public async Task<ExcelReportDataDto?> GetFuelDailySalesReportData(DateTime shiftDate)
  {
    ExcelReportDataDto excelReportData = new();

    IUnitOfWork _unitOfWorkCSCAdmin = new UnitOfWork(_applicationConfig, _applicationConfig.CSCAdminConnectionString);
    excelReportData.Company = await _unitOfWorkCSCAdmin.CompanySetups.GetCompanyAsync();
    _unitOfWorkCSCAdmin.Commit();

    IUnitOfWork _unitOfWorkCSCMaster = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
    excelReportData.FuelDept = await _unitOfWorkCSCMaster.Fuels.GetFuelDepartmentAsync();
    _unitOfWorkCSCMaster.Commit();

    string? fuelDept = excelReportData.FuelDept;
    IUnitOfWork _unitOfWorkCSCPump = new UnitOfWork(_applicationConfig, _applicationConfig.CSCPumpConnectionString);
    excelReportData.Grades = await _unitOfWorkCSCPump.Grades.GetGradeByJoiningGradeSaleLineSaleHeadTables(shiftDate, fuelDept);

    IList<string?> stockCodes = excelReportData.Grades?.Select(g => g.StockCode).ToList() ?? new List<string?>();
    IList<int?> gradeIds = excelReportData.Grades?.Select(g => g.Id).ToList() ?? new List<int?>();

    excelReportData.FuelPriceHistories = await _unitOfWorkCSCPump.FuelPriceHistory.GetFuelPriceHistoryByGradeId(gradeIds);
    _unitOfWorkCSCPump.Commit();

    excelReportData.SaleHeadAndSaleLinesByStockCodes = await _unitOfWork.SaleHeads.GetSaleHeadAndSaleLineDataByStockCodesAsync(stockCodes, shiftDate, fuelDept);
    excelReportData.SaleHeadAndSaleLinesByGradeIds = await _unitOfWork.SaleHeads.GetSaleHeadAndSaleLineDataByGradeIdsAsync(gradeIds, shiftDate, fuelDept);
    excelReportData.TotalDiscount = await _unitOfWork.SaleHeads.GetTotalDiscountByShiftDateAsync(shiftDate, fuelDept);
    _unitOfWork.Commit();

    return excelReportData;
  }

  public async Task<IList<string>?> GetUniqueStockCodes()
  {
    IUnitOfWork _unitOfWorkCSCPump = new UnitOfWork(_applicationConfig, _applicationConfig.CSCPumpConnectionString);
    IList<string>? stockCodes = await _unitOfWorkCSCPump.Grades.GetUniqueStockCodesAsync();
    _unitOfWorkCSCPump.Commit();

    return stockCodes;
  }

  public async Task<FuelLitresSummaryReportDto> GetFuelLitresOrDollarsSummaryReportData()
  {
    FuelLitresSummaryReportDto fuelLitresSummaryReportData = new FuelLitresSummaryReportDto();
    fuelLitresSummaryReportData.SalesData = await _unitOfWork.Sales.GetAllSaleDataAsync();
    _unitOfWork.Commit();

    IUnitOfWork _unitOfWorkCSCAdmin = new UnitOfWork(_applicationConfig, _applicationConfig.CSCAdminConnectionString);
    fuelLitresSummaryReportData.CompanyDetails = await _unitOfWorkCSCAdmin.CompanySetups.GetCompanyAsync();
    _unitOfWorkCSCAdmin.Commit();

    return fuelLitresSummaryReportData;
  }

  public async Task<IList<StockItem>?> GetAllStockMaster()
  {
    IUnitOfWork _unitOfWorkCSCMaster = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
    IEnumerable<StockItem>? stockMasters = await _unitOfWorkCSCMaster.StockItems.GetAllStockMasterAsync();
    _unitOfWorkCSCMaster.Commit();

    return stockMasters.ToList();
  }

  public async Task<IList<Grade>?> GetGradesByFuelType()
  {
    IUnitOfWork _unitOfWorkCSCPump = new UnitOfWork(_applicationConfig, _applicationConfig.CSCPumpConnectionString);
    IList<Grade>? grades = await _unitOfWorkCSCPump.Grades.GetGradesByFuelTypeAsync();
    _unitOfWorkCSCPump.Commit();

    return grades;
  }

  public async Task<IList<SaleHeadDto>?> GetDistinctDatesFromSaleHead(ExcelReportGenerator excelReportGenerator)
  {
    IList<SaleHeadDto>? dates = await _unitOfWork.SaleHeads.GetDistinctDatesFromSaleHeadAsync(excelReportGenerator);
    _unitOfWork.Commit();

    return dates;
  }

  public async Task<IList<SummaryFuelProductsReportDto>?> GetSummaryFuelProductsReportData(ExcelReportGenerator excelReportGenerator)
  {
    IList<SummaryFuelProductsReportDto>? data = await _unitOfWork.ExcelReports.GetSummaryFuelProductsReportDataAsync(excelReportGenerator);
    _unitOfWork.Commit();

    return data;
  }
  
  public async Task<IList<FleetCardReportDto>?> GetNorth60OrYTGFleetCardReportData(ExcelReportGenerator excelReportGenerator, bool isYTG)
  {
    IUnitOfWork _unitOfWorkCSCMaster = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
    string? fuelDept = await _unitOfWorkCSCMaster.Fuels.GetFuelDepartmentAsync();
    _unitOfWorkCSCMaster.Commit();

    excelReportGenerator.Department = fuelDept;
    IList<FleetCardReportDto>? data = await _unitOfWork.ExcelReports.GetNorth60OrYTGFleetCardReportDataAsync(excelReportGenerator, isYTG);
    _unitOfWork.Commit();

    return data;
  }

  public async Task<IList<SaleLine>?> GetAmountFromSaleLine()
  {
    IUnitOfWork _unitOfWorkCSCMaster = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
    string? fuelDept = await _unitOfWorkCSCMaster.Fuels.GetFuelDepartmentAsync();
    _unitOfWorkCSCMaster.Commit();

    IList<SaleLine>? amounts = await _unitOfWork.ExcelReports.GetAmountFromSaleLineAsync(fuelDept);
    _unitOfWork.Commit();

    return amounts;
  }
  
  public async Task<IList<SaleHead>?> GetSalesByHourReport(ExcelReportGenerator excelReportGenerator)
  {
    IList<SaleHead>? data = await _unitOfWork.SaleHeads.GetSalesByHourReportAsync(excelReportGenerator);
    _unitOfWork.Commit();

    return data;
  }
  
  public async Task<IList<ReportData>?> GetAllReportData()
  {
    IList<ReportData>? reportData = await _unitOfWork.ExcelReports.GetAllReportDataAsync();
    _unitOfWork.Commit();

    return reportData;
  }
}
