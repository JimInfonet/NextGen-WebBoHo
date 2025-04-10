using InfonetCSCR.Reporting.Models;
using Microsoft.Reporting.WebForms;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI.WebControls;

namespace InfonetCSCR.Reporting.Services
{
  public class DataTransferReportsService
  {
    public ReportViewer GetPriceChangeReport(
      List<PriceChangeReportDto> reportData,
      string reportpath,
      string language,
      bool isBackOffice = false)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);
      ReportViewer reportViewer = new ReportViewer();
      ReportParameter batch = new ReportParameter("batch", Resources.GlobalStrings.batchId);
      ReportParameter productId = new ReportParameter("productId", Resources.GlobalStrings.productId);
      ReportParameter description = new ReportParameter("description", Resources.GlobalStrings.description);
      ReportParameter priceType = new ReportParameter("priceType", Resources.GlobalStrings.priceType);
      ReportParameter unit = new ReportParameter("unit", Resources.GlobalStrings.unit);
      ReportParameter startDate = new ReportParameter("startDate", Resources.GlobalStrings.startDate);
      ReportParameter endDate = new ReportParameter("endDate", Resources.GlobalStrings.endDate);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      ReportParameter price = new ReportParameter("price", Resources.GlobalStrings.price);
      ReportParameter margin = new ReportParameter("margin", Resources.GlobalStrings.margin);
      ReportParameter from = new ReportParameter("from", Resources.GlobalStrings.from);
      ReportParameter to = new ReportParameter("to", Resources.GlobalStrings.to);
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.priceChangeReport);
      ReportParameter current = new ReportParameter("current", Resources.GlobalStrings.current);
      ReportParameter store = new ReportParameter("store", Resources.GlobalStrings.store);
      ReportParameter transferDate = new ReportParameter("transferDate", Resources.GlobalStrings.transferDate);
      ReportParameter vendor = new ReportParameter("vendor", Resources.GlobalStrings.vendor);
      ReportParameter futurePrice = new ReportParameter("futurePrice", Resources.GlobalStrings.futurePrice);
      ReportParameter isBackOfficeReportParameter = new ReportParameter("isBackOffice", isBackOffice ? "true" : "false");

      string DSName = "PriceTransferReportDataSet";
      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { batch,
                                                                                    reportTitle,
                                                                                    productId,
                                                                                    description,
                                                                                    priceType,
                                                                                    unit,
                                                                                    startDate,
                                                                                    endDate,
                                                                                    price,
                                                                                    margin,
                                                                                    printedOn,
                                                                                    from,
                                                                                    to,
                                                                                    store,
                                                                                    transferDate,
                                                                                    vendor,
                                                                                    current,
                                                                                    futurePrice,
                                                                                    isBackOfficeReportParameter};
      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);
      return reportViewer;
    }

    public ReportViewer GetHotButtonsReport(
      List<HotButtonsReportDto> reportData,
      string reportpath,
      string language)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter batch = new ReportParameter("batch", Resources.GlobalStrings.batchId);
      ReportParameter productId = new ReportParameter("productId", Resources.GlobalStrings.productId);
      ReportParameter description = new ReportParameter("description", Resources.GlobalStrings.description);
      ReportParameter button = new ReportParameter("button", Resources.GlobalStrings.button);
      ReportParameter caption1 = new ReportParameter("caption1", Resources.GlobalStrings.caption1);
      ReportParameter caption2 = new ReportParameter("caption2", Resources.GlobalStrings.caption2);
      ReportParameter transType = new ReportParameter("transType", Resources.GlobalStrings.transType);
      ReportParameter store = new ReportParameter("store", Resources.GlobalStrings.store);
      ReportParameter transferDate = new ReportParameter("transferDate", Resources.GlobalStrings.transferDate);
      ReportParameter transferUser = new ReportParameter("transferUser", Resources.GlobalStrings.transferUser);
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.hotButtonsReport);

      string DSName = "HotButtonsReportDataset";

      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() {reportTitle,
                                                                                   batch,
                                                                                   button,
                                                                                   productId,
                                                                                   description,
                                                                                   caption1,
                                                                                   caption2,
                                                                                   transType,
                                                                                   store,
                                                                                   transferDate,
                                                                                   transferUser};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetStockTransferReport(
      List<StockTransferReportDto> reportData,
      string reportpath,
      string language)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter batch = new ReportParameter("batch", Resources.GlobalStrings.batchId);
      ReportParameter productId = new ReportParameter("productId", Resources.GlobalStrings.productId);
      ReportParameter description = new ReportParameter("description", Resources.GlobalStrings.description);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.stockTransferReport);
      ReportParameter transType = new ReportParameter("transType", Resources.GlobalStrings.transType);
      ReportParameter store = new ReportParameter("store", Resources.GlobalStrings.store);
      ReportParameter transferDate = new ReportParameter("transferDate", Resources.GlobalStrings.transferDate);
      ReportParameter transferUser = new ReportParameter("transferUser", Resources.GlobalStrings.transferUser);

      string DSName = "StockTransferReportDataSet";

      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { batch,
                                                                                    reportTitle,
                                                                                    productId,
                                                                                    description,
                                                                                    transType,
                                                                                    store,
                                                                                    transferDate,
                                                                                    transferUser,
                                                                                    printedOn};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetCostChangeReport(
      List<CostChangeReportDto> reportData,
      string reportpath,
      string language,
      bool isBackOffice)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter batch = new ReportParameter("batch", Resources.GlobalStrings.batchId);
      ReportParameter vendor = new ReportParameter("vendor", Resources.GlobalStrings.vendor);
      ReportParameter user = new ReportParameter("user", Resources.GlobalStrings.user);
      ReportParameter startDate = new ReportParameter("startDate", Resources.GlobalStrings.startDate);
      ReportParameter endDate = new ReportParameter("endDate", Resources.GlobalStrings.endDate);
      ReportParameter currentCost = new ReportParameter("currentCost", Resources.GlobalStrings.currentCost);
      ReportParameter newCost = new ReportParameter("newCost", Resources.GlobalStrings.newCost);
      ReportParameter transferDate = new ReportParameter("transferDate", Resources.GlobalStrings.transferDate);
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.costChangeReport);
      ReportParameter store = new ReportParameter("store", Resources.GlobalStrings.store);
      ReportParameter product = new ReportParameter("product", Resources.GlobalStrings.product);
      ReportParameter isBackOfficeReportParameter = new ReportParameter("isBackOffice", isBackOffice ? "true" : "false");
      ReportParameter vendorIdName = new ReportParameter("vendorIdName", Resources.GlobalStrings.vendorIdName);

      string DSName = "CostChangeReportDataSet";

      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { batch,
                                                                                    reportTitle,
                                                                                    vendor,
                                                                                    user,
                                                                                    startDate,
                                                                                    endDate,
                                                                                    currentCost,
                                                                                    newCost,
                                                                                    store,
                                                                                    product,
                                                                                    transferDate,
                                                                                    isBackOfficeReportParameter,
                                                                                    vendorIdName};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetCommodityTransferReport(
      List<CommodityTransferReportDto> reportData,
      string reportpath,
      string language)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter batch = new ReportParameter("batch", Resources.GlobalStrings.batchId);
      ReportParameter dept = new ReportParameter("dept", Resources.GlobalStrings.dept);
      ReportParameter subDept = new ReportParameter("subDept", Resources.GlobalStrings.subDept);
      ReportParameter subDetail = new ReportParameter("subDetail", Resources.GlobalStrings.subDetail);
      ReportParameter description = new ReportParameter("description", Resources.GlobalStrings.description);
      ReportParameter transType = new ReportParameter("transType", Resources.GlobalStrings.transType);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.commodity);
      ReportParameter store = new ReportParameter("store", Resources.GlobalStrings.store);
      ReportParameter transferDate = new ReportParameter("transferDate", Resources.GlobalStrings.transferDate);
      ReportParameter transferUser = new ReportParameter("transferUser", Resources.GlobalStrings.transferUser);

      string DSName = "CommodityTransferReportDataSet";

      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { batch,
                                                                                    reportTitle,
                                                                                    dept,
                                                                                    subDept,
                                                                                    subDetail,
                                                                                    description,
                                                                                    transType,
                                                                                    store,
                                                                                    transferDate,
                                                                                    transferUser,
                                                                                    printedOn};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetPromotionalDiscountTransferReport(
      List<PromotionalDiscountTransferReportDto> reportData,
      string reportpath,
      string language)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter storeTotal = new ReportParameter("storeTotal", Resources.GlobalStrings.storeTotal);
      ReportParameter description = new ReportParameter("description", Resources.GlobalStrings.description);
      ReportParameter transactionId = new ReportParameter("transactionId", Resources.GlobalStrings.transactionID);
      ReportParameter transactionType = new ReportParameter("transactionType", Resources.GlobalStrings.transactionType);
      ReportParameter price = new ReportParameter("price", Resources.GlobalStrings.price);
      ReportParameter startDate = new ReportParameter("startDate", Resources.GlobalStrings.startDate);
      ReportParameter endDate = new ReportParameter("endDate", Resources.GlobalStrings.endDate);
      ReportParameter totalByVendor = new ReportParameter("totalByVendor", Resources.GlobalStrings.totalByVendor);
      ReportParameter transType = new ReportParameter("transType", Resources.GlobalStrings.transType);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.promotions);
      ReportParameter store = new ReportParameter("store", Resources.GlobalStrings.store);
      ReportParameter transferDate = new ReportParameter("transferDate", Resources.GlobalStrings.transferDate);
      ReportParameter transferUser = new ReportParameter("transferUser", Resources.GlobalStrings.transferUser);
      ReportParameter batchId = new ReportParameter("batchId", Resources.GlobalStrings.batchId);

      string DSName = "PromotionalDiscountTransferReportDataSet";

      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { storeTotal,
                                                                                    reportTitle,
                                                                                    description,
                                                                                    transType,
                                                                                    transactionId,
                                                                                    transactionType,
                                                                                    price,
                                                                                    startDate,
                                                                                    endDate,
                                                                                    totalByVendor,
                                                                                    store,
                                                                                    transferDate,
                                                                                    transferUser,
                                                                                    batchId,
                                                                                    printedOn};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }
  }
}
