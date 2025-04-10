using InfonetCSCR.Reporting.Models;
using InfonetCSCR.Reporting.Services;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;

namespace InfonetCSCR.Reporting.Controllers
{
  [Authorize]
  public class ReportController : Controller
  {
    private ReportService _reportService;
    private DataTransferReportsService _dataTransferReportService;

    public ReportController()
    {
      _reportService = new ReportService();
      _dataTransferReportService = new DataTransferReportsService();
    }

    public ActionResult Index()
    {
      string WebBaseUrl = WebConfigurationManager.AppSettings["WebBaseUrl"];
      ViewBag.WebBaseUrl = WebBaseUrl;
      return View();
    }

    public ReportViewer HandleReportHeaderOptions(ReportViewer reportViewer)
    {
      reportViewer.ShowZoomControl = true;
      reportViewer.ShowBackButton = false;
      reportViewer.ShowFindControls = true;
      reportViewer.ShowRefreshButton = false;

      return reportViewer;
    }

    // Stock Item Reports
    [HttpPost]
    public async Task<PartialViewResult> GetStockItemHistoryReport(
      string stockCode,
      string stockDescription,
      string startDate,
      string endDate,
      string companyName,
      string language = "en")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}StockItemReports/GetStockItemHistory?stockcode={stockCode}&startdate={startDate}&enddate={endDate}";
      var jwtToken = Request.Cookies.Get("Token").Value;
      List<StockItemHistory> stockItemHistories = await _reportService.GetDataAsync<List<StockItemHistory>>(url, jwtToken);
      stockItemHistories ??= new List<StockItemHistory>
        {
          new StockItemHistory()
          {
            StockCode = stockCode,
          }
        };
      string reportPath = Server.MapPath("~/Reports/StockItemHistoryReport.rdlc");
      ReportViewer reportViewer = _reportService.GetStockItemReport(
        stockItemHistories,
        reportPath,
        stockCode,
        stockDescription,
        language,
        startDate,
        endDate,
        companyName);
      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public async Task<PartialViewResult> GetSalesAnalysisByProductReport(
        string stockCode,
        string stockDescription,
        string startDate,
        string endDate,
        string companyName,
        string language = "en")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}StockItemReports/GetSalesAnalysisByProduct?stockcode={stockCode}&startdate={startDate}&enddate={endDate}";
      var jwtToken = Request.Cookies.Get("Token").Value;

      List<SalesAnalysisByProduct> salesAnalysisByProducts = await _reportService.GetDataAsync<List<SalesAnalysisByProduct>>(url, jwtToken);

      salesAnalysisByProducts ??= new List<SalesAnalysisByProduct>
        {
          new SalesAnalysisByProduct()
          {
            TransactionType = null,
          }
        };
      string reportPath = Server.MapPath("~/Reports/SalesAnalysisByProductReport.rdlc");
      ReportViewer reportViewer = _reportService.GetSalesAnalysisByProductReport(
        salesAnalysisByProducts,
        reportPath,
        stockCode,
        stockDescription,
        language,
        startDate,
        endDate,
        companyName);
      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public async Task<PartialViewResult> GetVendorPurchasesByProductReport(
      string stockCode,
      string stockDescription,
      string startDate,
      string endDate,
      string companyName,
      string language = "en")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}StockItemReports/GetVendorPurchasesByProduct?stockcode={stockCode}&startdate={startDate}&enddate={endDate}";
      var jwtToken = Request.Cookies.Get("Token").Value;

      List<VendorPurchasesByProduct> vendorPurchasesByProducts = await _reportService.GetDataAsync<List<VendorPurchasesByProduct>>(url, jwtToken);

      vendorPurchasesByProducts ??= new List<VendorPurchasesByProduct>
        {
          new VendorPurchasesByProduct()
          {
            VendorName = null,
          }
        };

      string reportPath = Server.MapPath("~/Reports/VendorPurchasesByProductReport.rdlc");
      ReportViewer reportViewer = _reportService.GetVendorPurchasesByProductReport(
        vendorPurchasesByProducts,
        reportPath,
        stockCode,
        stockDescription,
        language,
        startDate,
        endDate,
        companyName);
      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    //Data Transfer Reports

    [HttpPost]
    public PartialViewResult GetPriceChangeReport(
      List<PriceChangeReportDto> priceChangeReport,
      string language = "en",
      bool isBackOffice = false)
    {
      priceChangeReport ??= new List<PriceChangeReportDto>
        {
          new PriceChangeReportDto()
          {
            ProductId=null,
          }
        };
      string reportPath = Server.MapPath("~/Reports/PriceChangeReport.rdlc");
      ReportViewer reportViewer = _dataTransferReportService.GetPriceChangeReport(
        priceChangeReport,
        reportPath,
        language,
        isBackOffice);

      reportViewer = HandleReportHeaderOptions(reportViewer);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public PartialViewResult GetStockTransferReport(
      List<StockTransferReportDto> stockTransferData,
      string language = "en")
    {
      stockTransferData ??= new List<StockTransferReportDto>
        {
          new StockTransferReportDto()
          {
            ProductId = null,
          }
        };

      string reportPath = Server.MapPath("~/Reports/StockTransferReport.rdlc");
      ReportViewer reportViewer = _dataTransferReportService.GetStockTransferReport(
        stockTransferData,
        reportPath,
        language);

      reportViewer = HandleReportHeaderOptions(reportViewer);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public PartialViewResult GetHotButtonsReport(
      List<HotButtonsReportDto> hotButtonsReport,
      string language = "en")
    {
      hotButtonsReport ??= new List<HotButtonsReportDto>
        {
          new HotButtonsReportDto()
          {
            ProductId=null,
          }
        };

      string reportPath = Server.MapPath("~/Reports/HotButtonsReport.rdlc");
      ReportViewer reportViewer = _dataTransferReportService.GetHotButtonsReport(
        hotButtonsReport,
        reportPath,
        language);

      reportViewer = HandleReportHeaderOptions(reportViewer);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public PartialViewResult GetCostChangeReport(
      List<CostChangeReportDto> costChangeReport,
      string language = "en",
      bool isBackOffice = false)
    {
      costChangeReport ??= new List<CostChangeReportDto>
        {
          new CostChangeReportDto()
          {
            ProductId=null,
          }
        };

      string reportPath = Server.MapPath("~/Reports/CostChangeReport.rdlc");
      ReportViewer reportViewer = _dataTransferReportService.GetCostChangeReport(
        costChangeReport,
        reportPath,
        language,
        isBackOffice);

      reportViewer = HandleReportHeaderOptions(reportViewer);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public PartialViewResult GetCommodityTransferReport(
      List<CommodityTransferReportDto> commodityTransferData,
      string language = "en")
    {
      commodityTransferData ??= new List<CommodityTransferReportDto>
        {
          new CommodityTransferReportDto()
          {
            StoreId = null,
          }
        };

      string reportPath = Server.MapPath("~/Reports/CommodityTransferReport.rdlc");
      ReportViewer reportViewer = _dataTransferReportService.GetCommodityTransferReport(
        commodityTransferData,
        reportPath,
        language);

      reportViewer = HandleReportHeaderOptions(reportViewer);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public PartialViewResult GetPromotionalDiscountTransferReport(
      List<PromotionalDiscountTransferReportDto> commodityTransferData,
      string language = "en")
    {
      commodityTransferData ??= new List<PromotionalDiscountTransferReportDto>
        {
          new PromotionalDiscountTransferReportDto()
          {
            StoreId = null,
          }
        };

      string reportPath = Server.MapPath("~/Reports/PromotionalDiscountTransferReport.rdlc");
      ReportViewer reportViewer = _dataTransferReportService.GetPromotionalDiscountTransferReport(
        commodityTransferData,
        reportPath,
        language);

      reportViewer = HandleReportHeaderOptions(reportViewer);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    // Stock Takes Reports
    [HttpPost]
    public async Task<PartialViewResult> GetStockTakesAndReceivingReport(int purpose, string companyName, string language = "en")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}StockTakesAndReceiving/GetStockTakesAndReceivingReportData?purpose={purpose}";
      var jwtToken = Request.Cookies.Get("Token").Value;

      List<StockTakesAndReceivingReportDto> stockTakesAndReceivingReportData = await _reportService.GetDataAsync<List<StockTakesAndReceivingReportDto>>(url, jwtToken);
      stockTakesAndReceivingReportData ??= new List<StockTakesAndReceivingReportDto>
        {
          new StockTakesAndReceivingReportDto()
          {
            StockCode = "",
          }
        };
      string reportPath = purpose == 1 ? Server.MapPath("~/Reports/StockReceivingReport.rdlc")
                                       : Server.MapPath("~/Reports/StockTakingReport.rdlc");

      ReportViewer reportViewer = purpose == 1 ? _reportService.GetStockReceivingReport(
        stockTakesAndReceivingReportData,
        reportPath,
        language,
        companyName) : _reportService.GetStockTakesReport(
        stockTakesAndReceivingReportData,
        reportPath,
        language,
        companyName)
        ;
      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    // TimeSheet Report
    [HttpPost]
    public async Task<PartialViewResult> GetTimeSheetReport(
      string payPeriod,
      DateTime startingDate,
      long reportId,
      string userCode,
      string companyName,
      double priorOS,
      int overtimeHPolicy,
      int overtimeSPolicy,
      string language = "en")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}TimesheetReports/GetTimeSheetReportData?reportId={reportId}&userCode={userCode}&shiftDate={startingDate}";
      var jwtToken = Request.Cookies.Get("Token").Value;

      TimeSheetReportDto timesheetReportData = await _reportService.GetDataAsync<TimeSheetReportDto>(url, jwtToken);
      timesheetReportData ??= new TimeSheetReportDto()
      {
        TimeSheetMainReportData = new List<TimeSheetMainReportDto>
        {
          new TimeSheetMainReportDto()
          {
            ReportID= reportId,
          }
        },
        ShortageData = new List<ShortageDto>
        {
          new ShortageDto()
          {
            ReportId=reportId,
          }
        },
      };
      timesheetReportData.PriorOS = priorOS;
      timesheetReportData.OvertimeHPolicy = overtimeHPolicy;
      timesheetReportData.OvertimeSPolicy = overtimeSPolicy;
      string reportPath = Server.MapPath("~/Reports/TimeSheetReport.rdlc");

      ReportViewer reportViewer = _reportService.GetTimeSheetReport(
        timesheetReportData,
        payPeriod,
        reportPath,
        language,
        companyName)
        ;
      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    // Purchasing and Receiving Report
    [HttpPost]
    public async Task<PartialViewResult> GetPurchasingAndReceivingReport(int purchaseNo, string language = "en")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}PurchasingAndReceiving/GetPurchasingAndReceivingReportData/{purchaseNo}";
      var jwtToken = Request.Cookies.Get("Token").Value;

      List<PurchasingAndReceivingReportDto> purchasingAndReceivingReportData = await _reportService.GetDataAsync<List<PurchasingAndReceivingReportDto>>(url, jwtToken);
      purchasingAndReceivingReportData ??= new List<PurchasingAndReceivingReportDto>
        {
          new PurchasingAndReceivingReportDto()
          {
            PurchaseNo = 0,
          }
        };
      string reportPath = Server.MapPath("~/Reports/PurchasingAndReceivingReport.rdlc");

      ReportViewer reportViewer = _reportService.GetPurchasingAndReceivingReport(
        purchasingAndReceivingReportData,
        reportPath,
        language);
      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    // Fuel Price Change Report
    [HttpPost]
    public async Task<PartialViewResult> GetFuelPriceChangeReport(string storeIds, bool isFuelCp, string emplId, string companyName, string language = "en")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}FuelPriceChange/GetFuelPriceChangeReportData?storeIds={storeIds}&isFuelCp={isFuelCp}";
      var jwtToken = Request.Cookies.Get("Token").Value;

      List<FuelPriceChangeReportDto> fuelPriceChangeReportDatas = await _reportService.GetDataAsync<List<FuelPriceChangeReportDto>>(url, jwtToken);

      string reportPath = Server.MapPath("~/Reports/FuelPriceChangeReport.rdlc");

      foreach (var store in fuelPriceChangeReportDatas)
      {
        store.DateTime?.ToString("MM/dd/yyyy hh:mm tt");
      }
      string printDate = DateTime.Today.ToString("MM/dd/yyyy");

      ReportViewer reportViewer = _reportService.GetFuelPriceChangeReport(
        fuelPriceChangeReportDatas,
        reportPath,
        language,
        emplId,
        printDate,
        companyName);
      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    private int GetEJSubreportVisibility(ElectronicJournalReportData electronicJournalReport)
    {
      return
       (electronicJournalReport.Type == "CANCEL" ||
        electronicJournalReport.Type == "VOID" ||
        electronicJournalReport.Type == "MARKDOWN" ||
        electronicJournalReport.Type == "RUNAWAY") ? 1 : 0;
    }

    [HttpPost]
    public async Task<PartialViewResult> GetElectronicJournalReport(
      int saleNo,
      string companyName,
      bool isFromTills = false,
      string language = "en",
      string storeID = null)
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}ElectronicJournal/GetElectronicJournalReportData?saleNo={saleNo}&isFromTills={isFromTills}";
      string subreportURL = $"{apiBaseUrl}ElectronicJournal/GetEJSubreportData?saleNo={saleNo}&isFromTills={isFromTills}";
      var jwtToken = Request.Cookies.Get("Token").Value;

      List<ElectronicJournalReportData> electronicJournalReport = await _reportService.GetDataAsync<List<ElectronicJournalReportData>>(url, jwtToken);
      List<ElectronicJournalSubreport> ejSubreport = await _reportService.GetDataAsync<List<ElectronicJournalSubreport>>(subreportURL, jwtToken);

      if (electronicJournalReport == null || !electronicJournalReport.Any())
      {
        electronicJournalReport = new List<ElectronicJournalReportData> { new ElectronicJournalReportData()
          { SaleNo = saleNo }
        };
      }

      if (ejSubreport == null || !ejSubreport.Any())
      {
        ejSubreport = new List<ElectronicJournalSubreport> { new ElectronicJournalSubreport()
          {
            SaleNo = saleNo,
            IsHidden = 1
          }
        };
      }

      ejSubreport[0].IsHidden = GetEJSubreportVisibility(electronicJournalReport[0]);

      string reportPath = Server.MapPath("~/Reports/ElectronicJournalReport.rdlc");
      ReportViewer reportViewer = _reportService.GetElectronicJournalReport(
        electronicJournalReport,
        ejSubreport,
        reportPath,
        companyName,
        language,
        storeID);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public async Task<PartialViewResult> GetElectronicJournalSummaryReport(string companyName, string language = "en")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}ElectronicJournal/GetElectronicJournalSummaryReportData";
      string subReportAURL = $"{apiBaseUrl}ElectronicJournal/GetEJSummarySubReportAData";
      string subReportTURL = $"{apiBaseUrl}ElectronicJournal/GetEJSummarySubReportTData";
      var jwtToken = Request.Cookies.Get("Token").Value;

      List<ElectronicJournalSummaryReport> ejSummaryReports = await _reportService.GetDataAsync<List<ElectronicJournalSummaryReport>>(url, jwtToken);
      EJSummarySubreportAData ejSummarySubReportAData = await _reportService.GetDataAsync<EJSummarySubreportAData>(subReportAURL, jwtToken);
      List<EJSummarySubreportTData?> ejSummarySubReportTData = await _reportService.GetDataAsync<List<EJSummarySubreportTData?>>(subReportTURL, jwtToken);

      string reportPath = Server.MapPath("~/Reports/ElectronicJournalSummaryReport.rdlc");
      ReportViewer reportViewer = _reportService.GetElectronicJournalSummaryReport(
        ejSummaryReports,
        ejSummarySubReportAData,
        ejSummarySubReportTData,
        reportPath,
        companyName,
        language);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public async Task<PartialViewResult> GetElectronicJournalDetailsReport(
      string companyName,
      string language = "en")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}ElectronicJournal/GetEJDetailsReportData";
      var jwtToken = Request.Cookies.Get("Token").Value;

      List<ElectronicJournalDetailsReport> electronicJournalReport = await _reportService.GetDataAsync<List<ElectronicJournalDetailsReport>>(url, jwtToken);

      string reportPath = Server.MapPath("~/Reports/ElectronicJournalDetailsReport.rdlc");
      ReportViewer reportViewer = _reportService.GetElectronicJournalDetailsReport(
        electronicJournalReport,
        reportPath,
        companyName,
        language);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public async Task<PartialViewResult> GetElectronicJournalTenderDetailsReport(
      int saleNo,
      string companyName,
      string language = "en")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}ElectronicJournal/GetEJTenderDetailsReportData?saleNo={saleNo}";
      var jwtToken = Request.Cookies.Get("Token").Value;

      string? ejTenderDetailsReportData = await _reportService.GetDataAsync<string?>(url, jwtToken);

      string reportPath = Server.MapPath("~/Reports/ElectronicJournalTenderDetailsReport.rdlc");
      ReportViewer reportViewer = _reportService.GetElectronicJournalTenderDetailsReport(
        ejTenderDetailsReportData,
        reportPath,
        companyName,
        language);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }

    [HttpPost]
    public async Task<PartialViewResult> GetLabelPrintingReport(string language = "en", string format = "", string orientation = "")
    {
      string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
      string url = $"{apiBaseUrl}PricebookManagement/GetLabelPrintingReportData";
      var jwtToken = Request.Cookies.Get("Token").Value;

      if(format == "STANDARD")
      {
        url = $"{apiBaseUrl}PricebookManagement/GetLabelPrintingStandardReportData";
      }

      List<LabelPrintDto> labelPrintingReport = await _reportService.GetDataAsync<List<LabelPrintDto>>(url, jwtToken);

      string reportPath = Server.MapPath("~/Reports/LabelPrintingReportStandard.rdlc");

      if (format != "STANDARD") {
        labelPrintingReport = labelPrintingReport.Where(x => x.StockType == 'V').ToList();
      }

      if (format == "STANDARD" && orientation == "LANDSCAPE") reportPath = Server.MapPath("~/Reports/LabelPrintingReportStandardLandscape.rdlc");
      if (format == "LARGE") reportPath = Server.MapPath("~/Reports/LabelPrintingReportLarge.rdlc");
      if (format == "SMALL") reportPath = Server.MapPath("~/Reports/LabelPrintingReportSmall.rdlc");

      ReportViewer reportViewer = _reportService.GetLabelPrintingReport(
        labelPrintingReport,
        reportPath,
        language);

      ViewBag.RDLCReport = reportViewer;
      return PartialView("_RDLCView");
    }
  }
}
