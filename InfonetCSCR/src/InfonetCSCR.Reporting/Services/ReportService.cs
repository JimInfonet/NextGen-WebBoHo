using InfonetCSCR.Reporting.Models;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace InfonetCSCR.Reporting.Services
{
  public class ReportService
  {
    public async Task<T> GetDataAsync<T>(string url, string jwtToken)
    {
      T data = default(T);
      using (HttpClient client = new HttpClient())
      {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        try
        {
          var response = await client.GetAsync(url);
          if (response.IsSuccessStatusCode)
          {
            string content = await response.Content.ReadAsStringAsync();
            if (response.Content.Headers.ContentType?.MediaType == "text/plain")
            {
              data = (T)(object)content;
            }
            else
            {
              data = JsonConvert.DeserializeObject<T>(content);
            }
          }
        }
        catch (Exception ex)
        {
          var message = ex.Message;
        }
      }
      return data;
    }

    public ReportViewer GetStockItemReport(
      List<StockItemHistory> reportData,
      string reportpath,
      string stockCode,
      string stockDescription,
      string language,
      string startDate,
      string endDate,
      string companyName)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter paramCompanyName = new ReportParameter("companyName", companyName);
      ReportParameter date = new ReportParameter("date", Resources.GlobalStrings.date);
      ReportParameter transactionID = new ReportParameter("transactionID", Resources.GlobalStrings.transactionID);
      ReportParameter transactionType = new ReportParameter("transactionType", Resources.GlobalStrings.transactionType);
      ReportParameter quantity = new ReportParameter("quantity", Resources.GlobalStrings.quantity);
      ReportParameter cost = new ReportParameter("cost", Resources.GlobalStrings.cost);
      ReportParameter totalCost = new ReportParameter("totalCost", Resources.GlobalStrings.totalCost);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      ReportParameter stockCodeDescription = new ReportParameter("stockCodeDescription", Resources.GlobalStrings.stockCodeDescription);
      ReportParameter reportTotals = new ReportParameter("reportTotals", Resources.GlobalStrings.reportTotals);
      ReportParameter dateRange = new ReportParameter("dateRange", Resources.GlobalStrings.dateRange);
      ReportParameter stCode = new ReportParameter("stockCode", stockCode);
      ReportParameter stName = new ReportParameter("stockName", stockDescription);
      ReportParameter startDateReportParameter = new ReportParameter("startDate", DateTime.Parse(startDate).ToString("MM/dd/yyyy"));
      ReportParameter endDateReportParameter = new ReportParameter("endDate", DateTime.Parse(endDate).ToString("MM/dd/yyyy"));
      ReportParameter reportTitle;

      string DSName = "StockItems";
      reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.stockItemHistory);

      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { paramCompanyName,
                                                                                    reportTitle,
                                                                                    stCode,
                                                                                    stName,
                                                                                    date,
                                                                                    transactionID,
                                                                                    transactionType,
                                                                                    quantity,
                                                                                    cost,
                                                                                    totalCost,
                                                                                    printedOn,
                                                                                    stockCodeDescription,
                                                                                    reportTotals,
                                                                                    dateRange,
                                                                                    startDateReportParameter,
                                                                                    endDateReportParameter};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetSalesAnalysisByProductReport(
      List<SalesAnalysisByProduct> reportData,
      string reportpath,
      string stockCode,
      string stockDescription,
      string language,
      string startDate,
      string endDate,
      string companyName)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter paramCompanyName = new ReportParameter("companyName", companyName);
      ReportParameter shiftDate = new ReportParameter("shiftDate", Resources.GlobalStrings.shiftDate);
      ReportParameter totalAmount = new ReportParameter("totalAmount", Resources.GlobalStrings.totalAmount);
      ReportParameter cost = new ReportParameter("cost", Resources.GlobalStrings.cost);
      ReportParameter saleNo = new ReportParameter("saleNo", Resources.GlobalStrings.saleNo);
      ReportParameter customer = new ReportParameter("customer", Resources.GlobalStrings.customer);
      ReportParameter quantity = new ReportParameter("quantity", Resources.GlobalStrings.quantity);
      ReportParameter price = new ReportParameter("price", Resources.GlobalStrings.price);
      ReportParameter totalCost = new ReportParameter("totalCost", Resources.GlobalStrings.totalCost);
      ReportParameter grossProfit = new ReportParameter("grossProfit", Resources.GlobalStrings.grossProfit);
      ReportParameter margin = new ReportParameter("margin", Resources.GlobalStrings.margin);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      ReportParameter stockCodeDescription = new ReportParameter("stockCodeDescription", Resources.GlobalStrings.stockCodeDescription);
      ReportParameter reportTotals = new ReportParameter("reportTotals", Resources.GlobalStrings.reportTotals);
      ReportParameter dateRange = new ReportParameter("dateRange", Resources.GlobalStrings.dateRange);
      ReportParameter stCode = new ReportParameter("stockCode", stockCode);
      ReportParameter stName = new ReportParameter("stockName", stockDescription);
      ReportParameter startDateReportParameter = new ReportParameter("startDate", DateTime.Parse(startDate).ToString("MM/dd/yyyy"));
      ReportParameter endDateReportParameter = new ReportParameter("endDate", DateTime.Parse(endDate).ToString("MM/dd/yyyy"));
      ReportParameter reportTitle;

      string DSName = "SalesAnalysisByProductReportDataSet";
      reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.salesAnalysisByProduct);

      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData.ToDataTable());
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { paramCompanyName,
                                                                                    reportTitle,
                                                                                    stCode,
                                                                                    stName,
                                                                                    shiftDate,
                                                                                    saleNo,
                                                                                    customer,
                                                                                    quantity,
                                                                                    price,
                                                                                    totalAmount,
                                                                                    cost,
                                                                                    totalCost,
                                                                                    grossProfit,
                                                                                    margin,
                                                                                    printedOn,
                                                                                    stockCodeDescription,
                                                                                    reportTotals,
                                                                                    dateRange,
                                                                                    startDateReportParameter,
                                                                                    endDateReportParameter};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetVendorPurchasesByProductReport(
      List<VendorPurchasesByProduct> reportData,
      string reportpath,
      string stockCode,
      string stockDescription,
      string language,
      string startDate,
      string endDate,
      string companyName)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter paramCompanyName = new ReportParameter("companyName", companyName);
      ReportParameter vendor = new ReportParameter("vendor", Resources.GlobalStrings.vendor);
      ReportParameter date = new ReportParameter("date", Resources.GlobalStrings.date);
      ReportParameter quantityReceived = new ReportParameter("quantityReceived", Resources.GlobalStrings.quantityReceived);
      ReportParameter cost = new ReportParameter("cost", Resources.GlobalStrings.cost);
      ReportParameter totalCost = new ReportParameter("totalCost", Resources.GlobalStrings.totalCost);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      ReportParameter stockCodeDescription = new ReportParameter("stockCodeDescription", Resources.GlobalStrings.stockCodeDescription);
      ReportParameter reportTotals = new ReportParameter("reportTotals", Resources.GlobalStrings.reportTotals);
      ReportParameter dateRange = new ReportParameter("dateRange", Resources.GlobalStrings.dateRange);
      ReportParameter stCode = new ReportParameter("stockCode", stockCode);
      ReportParameter stName = new ReportParameter("stockName", stockDescription);
      ReportParameter startDateReportParameter = new ReportParameter("startDate", DateTime.Parse(startDate).ToString("MM/dd/yyyy"));
      ReportParameter endDateReportParameter = new ReportParameter("endDate", DateTime.Parse(endDate).ToString("MM/dd/yyyy"));

      string DSName = "VendorPurchasesByProduct";
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.vendorPurchasesByProduct);

      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { paramCompanyName,
                                                                                    reportTitle,
                                                                                    stCode,
                                                                                    stName,
                                                                                    vendor,
                                                                                    date,
                                                                                    quantityReceived,
                                                                                    cost,
                                                                                    totalCost,
                                                                                    printedOn,
                                                                                    stockCodeDescription,
                                                                                    reportTotals,
                                                                                    dateRange,
                                                                                    startDateReportParameter,
                                                                                    endDateReportParameter};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetStockReceivingReport(
      List<StockTakesAndReceivingReportDto> reportData,
      string reportpath,
      string language,
      string? companyName)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter paramCompanyName = new ReportParameter("companyName", companyName);
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.stockReceivingReport);
      ReportParameter stockCode = new ReportParameter("stockCode", Resources.GlobalStrings.stockCode);
      ReportParameter description = new ReportParameter("description", Resources.GlobalStrings.description);
      ReportParameter quantity = new ReportParameter("quantity", Resources.GlobalStrings.quantity);
      ReportParameter unitCost = new ReportParameter("unitCost", Resources.GlobalStrings.unitCost);
      ReportParameter charges = new ReportParameter("charges", Resources.GlobalStrings.charges);
      ReportParameter taxes = new ReportParameter("taxes", Resources.GlobalStrings.taxes);
      ReportParameter total = new ReportParameter("total", Resources.GlobalStrings.total);
      ReportParameter batchNumber = new ReportParameter("batchNumber", Resources.GlobalStrings.batchNumber);
      ReportParameter invoiceNumber = new ReportParameter("invoiceNumber", Resources.GlobalStrings.invoiceNumber);
      ReportParameter totalForInvoiceNumber = new ReportParameter("totalForInvoiceNumber", Resources.GlobalStrings.totalForInvoiceNumber);
      ReportParameter totalForBatchNumber = new ReportParameter("totalForBatchNumber", Resources.GlobalStrings.totalForBatchNumber);
      ReportParameter grandTotal = new ReportParameter("grandTotal", Resources.GlobalStrings.grandTotal);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);

      string DSName = "StockReceivingReportDataSet";

      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { paramCompanyName,
                                                                                    reportTitle,
                                                                                    stockCode,
                                                                                    description,
                                                                                    quantity,
                                                                                    unitCost,
                                                                                    charges,
                                                                                    taxes,
                                                                                    total,
                                                                                    batchNumber,
                                                                                    invoiceNumber,
                                                                                    totalForInvoiceNumber,
                                                                                    totalForBatchNumber,
                                                                                    grandTotal,
                                                                                    printedOn};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetStockTakesReport(
      List<StockTakesAndReceivingReportDto> reportData,
      string reportpath,
      string language,
      string companyName)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter paramCompanyName = new ReportParameter("companyName", companyName);
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.stockTakingReport);
      ReportParameter stockCode = new ReportParameter("stockCode", Resources.GlobalStrings.stockCode);
      ReportParameter description = new ReportParameter("description", Resources.GlobalStrings.description);
      ReportParameter batchNumber = new ReportParameter("batchNumber", Resources.GlobalStrings.batchNumber);
      ReportParameter total = new ReportParameter("total", Resources.GlobalStrings.totalFor);

      ReportParameter count = new ReportParameter("count", Resources.GlobalStrings.count);
      ReportParameter system = new ReportParameter("system", Resources.GlobalStrings.system);
      ReportParameter cost = new ReportParameter("cost", Resources.GlobalStrings.cost);
      ReportParameter quantityDiff = new ReportParameter("quantityDiff", Resources.GlobalStrings.quantityDiff);

      ReportParameter quantityInDollarDiff = new ReportParameter("quantityInDollarDiff", Resources.GlobalStrings.quantityInDollarDiff);
      ReportParameter batchTotal = new ReportParameter("batchTotal", Resources.GlobalStrings.batchTotal);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);

      string DSName = "StockTakingReportDataset";

      reportViewer.LocalReport.ReportPath = reportpath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { paramCompanyName,
                                                                                    reportTitle,
                                                                                    stockCode,
                                                                                    description,
                                                                                    count,
                                                                                    system,
                                                                                    cost,
                                                                                    quantityDiff,
                                                                                    total,
                                                                                    batchNumber,
                                                                                    quantityInDollarDiff,
                                                                                    batchTotal,
                                                                                    printedOn };

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetTimeSheetReport(
      TimeSheetReportDto timesheetReportData,
      string payPeriodTypeString,
      string reportPath,
      string language,
      string companyNameString)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      decimal overtimeLimit = timesheetReportData.TimeSheetMainReportData.First().Type == "Salary" ? timesheetReportData.OvertimeSPolicy : timesheetReportData.OvertimeHPolicy;
      decimal statHours = timesheetReportData.TimeSheetMainReportData.Sum(item => item.Stat == true ? item.Hours : 0);
      decimal totalSum = (decimal)timesheetReportData.PriorOS + timesheetReportData.TimeSheetMainReportData.Sum(item => item.Shortages) + timesheetReportData.TimeSheetMainReportData.First().UniformPurchases;

      decimal overtimeHours = timesheetReportData.TimeSheetMainReportData
                              .GroupBy(item => item.BiWeeklyGroupingColumn)
                              .Select(group => group.Sum(data => data.Hours))
                              .Select(totalHours => totalHours > overtimeLimit ? totalHours - overtimeLimit : 0)
                              .Sum();
      decimal totalHours = timesheetReportData.TimeSheetMainReportData.Sum(item => item.Hours);
      decimal regularHours = totalHours - statHours - overtimeHours;
      decimal statutoryHours = statHours;
      decimal totalPaidHours = timesheetReportData.TimeSheetMainReportData.First().Type == "Salary"
       ? (overtimeHours * (decimal)0.5) + totalHours
       : (overtimeHours * (decimal)0.5) + totalHours + (statHours * (decimal)0.5);


      ReportViewer reportViewer = new ReportViewer();
      ReportParameter regularHoursParameter = new ReportParameter("regularHours", regularHours.ToString());
      ReportParameter overtimeHoursParameter = new ReportParameter("overtimeHours", overtimeHours.ToString());
      ReportParameter statutoryHoursParameter = new ReportParameter("statutoryHours", statutoryHours.ToString());
      ReportParameter totalPaidHoursParameter = new ReportParameter("totalPaidHours", totalPaidHours.ToString());
      ReportParameter totalSumParameter = new ReportParameter("totalSum", totalSum.ToString());
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.semiMonthlyTimeReport);
      ReportParameter payPeriod = new ReportParameter("payPeriod", Resources.GlobalStrings.payPeriod);
      ReportParameter employeeId = new ReportParameter("employeeId", Resources.GlobalStrings.employeeId);
      ReportParameter employee = new ReportParameter("employee", Resources.GlobalStrings.employee);
      ReportParameter shiftDate = new ReportParameter("shiftDate", Resources.GlobalStrings.shiftDate);
      ReportParameter shortage = new ReportParameter("shortage", Resources.GlobalStrings.shortage);
      ReportParameter stat = new ReportParameter("stat", Resources.GlobalStrings.stat);
      ReportParameter timeIn = new ReportParameter("timeIn", Resources.GlobalStrings.timeIn);
      ReportParameter timeOut = new ReportParameter("timeOut", Resources.GlobalStrings.timeOut);
      ReportParameter hours = new ReportParameter("hours", Resources.GlobalStrings.hours);
      ReportParameter commission = new ReportParameter("commission", Resources.GlobalStrings.commission);
      ReportParameter training = new ReportParameter("training", Resources.GlobalStrings.training);
      ReportParameter comment = new ReportParameter("comment", Resources.GlobalStrings.comment);
      ReportParameter summaryForOfficeUseOnly = new ReportParameter("summaryForOfficeUseOnly", Resources.GlobalStrings.summaryForOfficeUseOnly);
      ReportParameter subTotal = new ReportParameter("subTotal", Resources.GlobalStrings.subTotal);
      ReportParameter total = new ReportParameter("total", Resources.GlobalStrings.total);
      ReportParameter cashOrProductShortages = new ReportParameter("cashOrProductShortages", Resources.GlobalStrings.cashOrProductShortages);
      ReportParameter uniformPurchases = new ReportParameter("uniformPurchases", Resources.GlobalStrings.uniformPurchases);
      ReportParameter priorOS = new ReportParameter("priorOS", Resources.GlobalStrings.priorOS);
      ReportParameter regularHoursParam = new ReportParameter("localizedRegularHours", Resources.GlobalStrings.regularHours);
      ReportParameter overtimeHoursParam = new ReportParameter("localizedOvertimeHours", Resources.GlobalStrings.overtimeHours);
      ReportParameter statutoryHoursParam = new ReportParameter("localizedStatutoryHours", Resources.GlobalStrings.statutoryHours);
      ReportParameter totalHoursParam = new ReportParameter("localizedTotalHours", Resources.GlobalStrings.totalHours);
      ReportParameter totalPaidHoursParam = new ReportParameter("localizedTotalPaidHours", Resources.GlobalStrings.totalPaidHours);
      ReportParameter employeeSignature = new ReportParameter("employeeSignature", Resources.GlobalStrings.employeeSignature);
      ReportParameter managerSignature = new ReportParameter("managerSignature", Resources.GlobalStrings.managerSignature);
      ReportParameter to = new ReportParameter("to", Resources.GlobalStrings.to);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      ReportParameter amountsToBeDeductedFromPay = new ReportParameter("amountsToBeDeductedFromPay", Resources.GlobalStrings.amountsToBeDeductedFromPay);
      ReportParameter semiMonthlyTimeReport = new ReportParameter("semiMonthlyTimeReport", Resources.GlobalStrings.semiMonthlyTimeReport);
      ReportParameter biWeeklyTimeReport = new ReportParameter("biWeeklyTimeReport", Resources.GlobalStrings.biWeeklyTimeReport);
      ReportParameter companyName = new ReportParameter("companyName", companyNameString);
      ReportParameter authorizeStatement = new ReportParameter("authorizeStatement", Resources.GlobalStrings.authorizeStatement);
      ReportParameter payPeriodType = new ReportParameter("payPeriodType", payPeriodTypeString);
      ReportParameter payDate = new ReportParameter("payDate", Resources.GlobalStrings.payDate);
      ReportParameter priorOsShortage = new ReportParameter("priorOsShortage", Resources.GlobalStrings.priorOsShortage);
      ReportParameter priorOSValue = new ReportParameter("priorOSValue", timesheetReportData.PriorOS.ToString());
      ReportParameter reportIdIs = new ReportParameter("reportIdIs", Resources.GlobalStrings.reportIdIs + " " + timesheetReportData.TimeSheetMainReportData.First().ReportID);

      reportViewer.LocalReport.ReportPath = reportPath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource mainReportDataSource = new ReportDataSource("TimeSheetReportDataset", timesheetReportData.TimeSheetMainReportData);
      ReportDataSource shortagesReportDataSource = new ReportDataSource("ShortageDataset", timesheetReportData.ShortageData);
      ReportDataSource TimeSheetDetailDataSource = new ReportDataSource("TimeSheetDetailDataset", timesheetReportData.TimesheetDetailsData);
      reportViewer.LocalReport.DataSources.Add(mainReportDataSource);
      reportViewer.LocalReport.DataSources.Add(shortagesReportDataSource);
      reportViewer.LocalReport.DataSources.Add(TimeSheetDetailDataSource);
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { regularHoursParameter,
                                                                                    overtimeHoursParameter,
                                                                                    statutoryHoursParameter,
                                                                                    totalPaidHoursParameter,
                                                                                    totalSumParameter,
                                                                                    reportTitle,
                                                                                    payPeriod,
                                                                                    employeeId,
                                                                                    employee,
                                                                                    shiftDate,
                                                                                    shortage,
                                                                                    stat,
                                                                                    timeIn,
                                                                                    timeOut,
                                                                                    hours,
                                                                                    commission,
                                                                                    training,
                                                                                    comment,
                                                                                    summaryForOfficeUseOnly,
                                                                                    subTotal,
                                                                                    total,
                                                                                    cashOrProductShortages,
                                                                                    uniformPurchases,
                                                                                    priorOS,
                                                                                    regularHoursParam,
                                                                                    overtimeHoursParam,
                                                                                    statutoryHoursParam,
                                                                                    totalHoursParam,
                                                                                    totalPaidHoursParam,
                                                                                    employeeSignature,
                                                                                    managerSignature,
                                                                                    to,
                                                                                    payPeriodType,
                                                                                    companyName,
                                                                                    amountsToBeDeductedFromPay,
                                                                                    semiMonthlyTimeReport,
                                                                                    biWeeklyTimeReport,
                                                                                    authorizeStatement,
                                                                                    reportIdIs,
                                                                                    payDate,
                                                                                    priorOsShortage,
                                                                                    priorOSValue,
                                                                                    printedOn };

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.LocalReport.Refresh();
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetPurchasingAndReceivingReport(
      List<PurchasingAndReceivingReportDto> purchasingAndReceivingReportData,
      string reportPath,
      string language)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.purchasingAndReceivingReport);
      ReportParameter to = new ReportParameter("to", Resources.GlobalStrings.to);
      ReportParameter from = new ReportParameter("from", Resources.GlobalStrings.from);
      ReportParameter phone = new ReportParameter("phone", Resources.GlobalStrings.phone);
      ReportParameter fax = new ReportParameter("fax", Resources.GlobalStrings.fax);
      ReportParameter account = new ReportParameter("account", Resources.GlobalStrings.account);
      ReportParameter custAccount = new ReportParameter("custAccount", Resources.GlobalStrings.custAccount);
      ReportParameter poNumber = new ReportParameter("poNumber", Resources.GlobalStrings.poNumber);
      ReportParameter poDate = new ReportParameter("poDate", Resources.GlobalStrings.poDate);
      ReportParameter deliveryDate = new ReportParameter("deliveryDate", Resources.GlobalStrings.deliveryDate);
      ReportParameter dateReceived = new ReportParameter("dateReceived", Resources.GlobalStrings.dateReceived);
      ReportParameter terms = new ReportParameter("terms", Resources.GlobalStrings.terms);
      ReportParameter buyer = new ReportParameter("buyer", Resources.GlobalStrings.buyer);
      ReportParameter shipVia = new ReportParameter("shipVia", Resources.GlobalStrings.shipVia);
      ReportParameter fob = new ReportParameter("fob", Resources.GlobalStrings.fob);
      ReportParameter freight = new ReportParameter("freight", Resources.GlobalStrings.freight);
      ReportParameter cost = new ReportParameter("cost", Resources.GlobalStrings.cost);
      ReportParameter surchargeOrDiscount = new ReportParameter("surchargeOrDiscount", Resources.GlobalStrings.surchargeOrDiscount);
      ReportParameter totalTax = new ReportParameter("totalTax", Resources.GlobalStrings.totalTax);
      ReportParameter total = new ReportParameter("total", Resources.GlobalStrings.total);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);

      reportViewer.LocalReport.ReportPath = reportPath;
      reportViewer.LocalReport.DataSources.Clear();
      reportViewer.LocalReport.DataSources.Add(new ReportDataSource("PurchasingAndReceivingReportDataset", purchasingAndReceivingReportData));
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { reportTitle,
                                                                                    to,
                                                                                    from,
                                                                                    phone,
                                                                                    fax,
                                                                                    account,
                                                                                    custAccount,
                                                                                    poNumber,
                                                                                    poDate,
                                                                                    deliveryDate,
                                                                                    dateReceived,
                                                                                    terms,
                                                                                    buyer,
                                                                                    shipVia,
                                                                                    fob,
                                                                                    cost,
                                                                                    surchargeOrDiscount,
                                                                                    totalTax,
                                                                                    total,
                                                                                    printedOn,
                                                                                    freight};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.LocalReport.Refresh();
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetElectronicJournalReport(
      List<ElectronicJournalReportData> reportData,
      List<ElectronicJournalSubreport> ejSubreportData,
      string reportPath,
      string companyName,
      string language,
      string storeID)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      if (string.IsNullOrEmpty(storeID))
        storeID = "null";

      ReportViewer reportViewer = new ReportViewer();
      #region report parameters
      ReportParameter paramCompanyName = new ReportParameter("companyName", companyName);
      ReportParameter saleNoReportParameter = new ReportParameter("saleNo", Resources.GlobalStrings.saleNo);
      ReportParameter tillReportParameter = new ReportParameter("till", Resources.GlobalStrings.till);
      ReportParameter cashierReportParameter = new ReportParameter("cashier", Resources.GlobalStrings.cashier);
      ReportParameter typeReportParameter = new ReportParameter("type", Resources.GlobalStrings.type);
      ReportParameter saleTimeReportParameter = new ReportParameter("saleTime", Resources.GlobalStrings.saleTime);
      ReportParameter customerReportParameter = new ReportParameter("customer", Resources.GlobalStrings.customer);
      ReportParameter idReportParameter = new ReportParameter("id", Resources.GlobalStrings.id);
      ReportParameter codeReportParameter = new ReportParameter("code", Resources.GlobalStrings.code);
      ReportParameter descriptionReportParameter = new ReportParameter("description", Resources.GlobalStrings.description);
      ReportParameter qtyReportParameter = new ReportParameter("qty", Resources.GlobalStrings.qty);
      ReportParameter priceReportParameter = new ReportParameter("price", Resources.GlobalStrings.price);
      ReportParameter discReportParameter = new ReportParameter("disc", Resources.GlobalStrings.disc);
      ReportParameter amountReportParameter = new ReportParameter("amount", Resources.GlobalStrings.amount);
      ReportParameter saleDateReportParameter = new ReportParameter("saleDate", Resources.GlobalStrings.saleDate);
      ReportParameter shiftDateReportParameter = new ReportParameter("shiftDate", Resources.GlobalStrings.shiftDate);
      ReportParameter pumpIDReportParameter = new ReportParameter("pumpID", Resources.GlobalStrings.pumpID);
      ReportParameter summaryReportParameter = new ReportParameter("summary", Resources.GlobalStrings.summary);
      ReportParameter totalAmountReportParameter = new ReportParameter("totalAmount", Resources.GlobalStrings.totalAmount);
      ReportParameter totalTenderedReportParameter = new ReportParameter("totalTendered", Resources.GlobalStrings.totalTendered);
      ReportParameter totalChangeReportParameter = new ReportParameter("totalChange", Resources.GlobalStrings.totalChange);
      ReportParameter printedOnReportParameter = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      ReportParameter invoiceDiscountReportParameter = new ReportParameter("invoiceDiscount", Resources.GlobalStrings.invoiceDiscount);
      ReportParameter includedReportParameter = new ReportParameter("included", Resources.GlobalStrings.included);
      ReportParameter overpaymentReportParameter = new ReportParameter("overpayment", Resources.GlobalStrings.overpayment);
      ReportParameter storeReportParameter = new ReportParameter("store", Resources.GlobalStrings.store);
      ReportParameter storeIDReportParameter = new ReportParameter("storeID", storeID);
      #endregion

      string DSName = "ElectronicJournalDataSet";
      string subreportDSName = "ElectronicJournalSubreportDataSet";

      foreach( var data in ejSubreportData)
        data.CardNumber = GetMaskedCardNum(data.CardNumber);

      reportViewer.LocalReport.ReportPath = reportPath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);
      ReportDataSource subreportDataSource = new ReportDataSource(subreportDSName, ejSubreportData);
      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.DataSources.Add(subreportDataSource);
      reportViewer.LocalReport.Refresh();

      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { paramCompanyName,
                                                                                    saleNoReportParameter,
                                                                                    tillReportParameter,
                                                                                    cashierReportParameter,
                                                                                    typeReportParameter,
                                                                                    saleTimeReportParameter,
                                                                                    customerReportParameter,
                                                                                    idReportParameter,
                                                                                    codeReportParameter,
                                                                                    descriptionReportParameter,
                                                                                    qtyReportParameter,
                                                                                    priceReportParameter,
                                                                                    discReportParameter,
                                                                                    amountReportParameter,
                                                                                    saleDateReportParameter,
                                                                                    shiftDateReportParameter,
                                                                                    pumpIDReportParameter,
                                                                                    summaryReportParameter,
                                                                                    totalAmountReportParameter,
                                                                                    totalTenderedReportParameter,
                                                                                    totalChangeReportParameter,
                                                                                    invoiceDiscountReportParameter,
                                                                                    includedReportParameter,
                                                                                    overpaymentReportParameter,
                                                                                    printedOnReportParameter,
                                                                                    storeIDReportParameter,
                                                                                    storeReportParameter};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetElectronicJournalSummaryReport(
      List<ElectronicJournalSummaryReport> ejSummaryReportData,
      EJSummarySubreportAData ejSummarySubReportAData,
      List<EJSummarySubreportTData> ejSummarySubReportTData,
      string reportPath,
      string companyName,
      string language)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      #region report parameters
      ReportParameter paramCompanyName = new ReportParameter("companyName", companyName);
      ReportParameter saleSummaryReportParameter = new ReportParameter("saleSummary", Resources.GlobalStrings.saleSummary);
      ReportParameter departmentReportParameter = new ReportParameter("department", Resources.GlobalStrings.department);
      ReportParameter descriptionReportParameter = new ReportParameter("description", Resources.GlobalStrings.description);
      ReportParameter salesReportParameter = new ReportParameter("sales", Resources.GlobalStrings.sales);
      ReportParameter totalsReportParameter = new ReportParameter("totals", Resources.GlobalStrings.totals);
      ReportParameter amountReportParameter = new ReportParameter("amount", Resources.GlobalStrings.amount);
      ReportParameter productSalesReportParameter = new ReportParameter("productSales", Resources.GlobalStrings.productSales);
      ReportParameter lineDiscountsReportParameter = new ReportParameter("lineDiscounts", Resources.GlobalStrings.lineDiscounts);
      ReportParameter invoiceDiscountsReportParameter = new ReportParameter("invoiceDiscounts", Resources.GlobalStrings.invoiceDiscounts);
      ReportParameter salesAfterDiscountsReportParameter = new ReportParameter("salesAfterDiscounts", Resources.GlobalStrings.salesAfterDiscounts);
      ReportParameter taxesReportParameter = new ReportParameter("taxes", Resources.GlobalStrings.taxes);
      ReportParameter chargesReportParameter = new ReportParameter("charges", Resources.GlobalStrings.charges);
      ReportParameter totalReceiptsReportParameter = new ReportParameter("totalReceipts", Resources.GlobalStrings.totalReceipts);
      ReportParameter tendersReportParameter = new ReportParameter("tenders", Resources.GlobalStrings.tenders);
      ReportParameter printedOn = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      #endregion

      string DSName = "ElectronicJournalSummaryDataSet";
      string subReportADSName = "EJSummarySubReportADataSet";
      string subReportTDSName = "EJSummarySubreportTDataSet";

      reportViewer.LocalReport.ReportPath = reportPath;
      reportViewer.LocalReport.DataSources.Clear();

      ReportDataSource reportDataSource = new ReportDataSource(DSName, ejSummaryReportData);
      ReportDataSource subreportADataSource = new ReportDataSource(subReportADSName, new List<EJSummarySubreportAData?> { ejSummarySubReportAData });
      ReportDataSource subreportTDataSource = new ReportDataSource(subReportTDSName, ejSummarySubReportTData);

      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.DataSources.Add(subreportADataSource);
      reportViewer.LocalReport.DataSources.Add(subreportTDataSource);

      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { paramCompanyName,
                                                                                    saleSummaryReportParameter,
                                                                                    departmentReportParameter,
                                                                                    descriptionReportParameter,
                                                                                    salesReportParameter,
                                                                                    totalsReportParameter,
                                                                                    amountReportParameter,
                                                                                    productSalesReportParameter,
                                                                                    lineDiscountsReportParameter,
                                                                                    invoiceDiscountsReportParameter,
                                                                                    salesAfterDiscountsReportParameter,
                                                                                    taxesReportParameter,
                                                                                    chargesReportParameter,
                                                                                    totalReceiptsReportParameter,
                                                                                    tendersReportParameter,
                                                                                    printedOn};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetElectronicJournalTenderDetailsReport(
      string? reportData,
      string reportPath,
      string companyName,
      string language)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();

      if (string.IsNullOrEmpty(reportData))
        reportData = Resources.GlobalStrings.noData;

      ReportParameter message = new ReportParameter("message", reportData);

      reportViewer.LocalReport.ReportPath = reportPath;
      reportViewer.LocalReport.DataSources.Clear();
      reportViewer.LocalReport.Refresh();

      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() {message};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetElectronicJournalDetailsReport(
      List<ElectronicJournalDetailsReport> reportData,
      string reportPath,
      string companyName,
      string language)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      #region report parameters
      ReportParameter paramCompanyName = new ReportParameter("companyName", companyName);
      ReportParameter saleNoReportParameter = new ReportParameter("saleNo", Resources.GlobalStrings.saleNo);
      ReportParameter tillReportParameter = new ReportParameter("till", Resources.GlobalStrings.till);
      ReportParameter cashierReportParameter = new ReportParameter("cashier", Resources.GlobalStrings.cashier);
      ReportParameter typeReportParameter = new ReportParameter("type", Resources.GlobalStrings.type);
      ReportParameter saleTimeReportParameter = new ReportParameter("saleTime", Resources.GlobalStrings.saleTime);
      ReportParameter customerReportParameter = new ReportParameter("customer", Resources.GlobalStrings.customer);
      ReportParameter idReportParameter = new ReportParameter("id", Resources.GlobalStrings.id);
      ReportParameter codeReportParameter = new ReportParameter("code", Resources.GlobalStrings.code);
      ReportParameter descriptionReportParameter = new ReportParameter("description", Resources.GlobalStrings.description);
      ReportParameter qtyReportParameter = new ReportParameter("qty", Resources.GlobalStrings.qty);
      ReportParameter priceReportParameter = new ReportParameter("price", Resources.GlobalStrings.price);
      ReportParameter discReportParameter = new ReportParameter("disc", Resources.GlobalStrings.disc);
      ReportParameter amountReportParameter = new ReportParameter("amount", Resources.GlobalStrings.amount);
      ReportParameter saleDateReportParameter = new ReportParameter("saleDate", Resources.GlobalStrings.saleDate);
      ReportParameter shiftDateReportParameter = new ReportParameter("shiftDate", Resources.GlobalStrings.shiftDate);
      ReportParameter pumpIDReportParameter = new ReportParameter("pumpID", Resources.GlobalStrings.pumpID);
      ReportParameter summaryReportParameter = new ReportParameter("summary", Resources.GlobalStrings.summary);
      ReportParameter totalAmountReportParameter = new ReportParameter("totalAmount", Resources.GlobalStrings.totalAmount);
      ReportParameter totalTenderedReportParameter = new ReportParameter("totalTendered", Resources.GlobalStrings.totalTendered);
      ReportParameter totalChangeReportParameter = new ReportParameter("totalChange", Resources.GlobalStrings.totalChange);
      ReportParameter invoiceDiscountReportParameter = new ReportParameter("invoiceDiscount", Resources.GlobalStrings.invoiceDiscount);
      ReportParameter includedReportParameter = new ReportParameter("included", Resources.GlobalStrings.included);
      ReportParameter overpaymentReportParameter = new ReportParameter("overpayment", Resources.GlobalStrings.overpayment);
      ReportParameter printedOnReportParameter = new ReportParameter("printedOn", Resources.GlobalStrings.printedOn);
      #endregion

      string DSName = "EJDetailsReportDataSet";

      foreach (var data in reportData)
        data.CardNumber = GetMaskedCardNum(data.CardNumber);

      reportViewer.LocalReport.ReportPath = reportPath;
      reportViewer.LocalReport.DataSources.Clear();
      ReportDataSource reportDataSource = new ReportDataSource(DSName, reportData);

      reportViewer.LocalReport.DataSources.Add(reportDataSource);
      reportViewer.LocalReport.Refresh();

      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { paramCompanyName,
                                                                                    saleNoReportParameter,
                                                                                    tillReportParameter,
                                                                                    cashierReportParameter,
                                                                                    typeReportParameter,
                                                                                    saleTimeReportParameter,
                                                                                    customerReportParameter,
                                                                                    idReportParameter,
                                                                                    codeReportParameter,
                                                                                    descriptionReportParameter,
                                                                                    qtyReportParameter,
                                                                                    priceReportParameter,
                                                                                    discReportParameter,
                                                                                    amountReportParameter,
                                                                                    saleDateReportParameter,
                                                                                    shiftDateReportParameter,
                                                                                    pumpIDReportParameter,
                                                                                    summaryReportParameter,
                                                                                    totalAmountReportParameter,
                                                                                    totalTenderedReportParameter,
                                                                                    invoiceDiscountReportParameter,
                                                                                    includedReportParameter,
                                                                                    overpaymentReportParameter,
                                                                                    printedOnReportParameter,
                                                                                    totalChangeReportParameter};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetFuelPriceChangeReport(
      List<FuelPriceChangeReportDto> fuelPriceChangeReportDatas,
      string reportPath,
      string language,
      string emplIdString,
      string printDateString,
      string companyNameString)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();
      ReportParameter reportTitle = new ReportParameter("reportTitle", Resources.GlobalStrings.fuelPriceChangeReport);
      ReportParameter grade = new ReportParameter("grade", Resources.GlobalStrings.grade);
      ReportParameter tier = new ReportParameter("tier", Resources.GlobalStrings.tier);
      ReportParameter level = new ReportParameter("level", Resources.GlobalStrings.level);
      ReportParameter cashPrice = new ReportParameter("cashPrice", Resources.GlobalStrings.cashPrice);
      ReportParameter creditPrice = new ReportParameter("creditPrice", Resources.GlobalStrings.creditPrice);
      ReportParameter dateTimeParam = new ReportParameter("dateTimeParam", Resources.GlobalStrings.dateTime);
      ReportParameter userID = new ReportParameter("userID", Resources.GlobalStrings.userID);
      ReportParameter companyName = new ReportParameter("companyName", companyNameString);
      ReportParameter emplId = new ReportParameter("emplId", emplIdString);
      ReportParameter printDate = new ReportParameter("printDate", printDateString);

      reportViewer.LocalReport.ReportPath = reportPath;
      reportViewer.LocalReport.DataSources.Clear();
      reportViewer.LocalReport.DataSources.Add(new ReportDataSource("FuelPriceChangeReportDataset", fuelPriceChangeReportDatas));
      IEnumerable<ReportParameter> reportParameters = new List<ReportParameter>() { reportTitle,
                                                                                    grade,
                                                                                    tier,
                                                                                    level,
                                                                                    cashPrice,
                                                                                    creditPrice,
                                                                                    userID,
                                                                                    emplId,
                                                                                    dateTimeParam,
                                                                                    printDate,
                                                                                    companyName};

      reportViewer.LocalReport.SetParameters(reportParameters);
      reportViewer.LocalReport.Refresh();
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    public ReportViewer GetLabelPrintingReport(
      List<LabelPrintDto> labelPrintingReport,
      string reportPath,
      string language)
    {
      Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(language);
      Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

      ReportViewer reportViewer = new ReportViewer();

      reportViewer.LocalReport.ReportPath = reportPath;
      reportViewer.LocalReport.DataSources.Clear();
      reportViewer.LocalReport.DataSources.Add(new ReportDataSource("LabelPrintingReportDataset", labelPrintingReport));

      reportViewer.LocalReport.Refresh();
      reportViewer.SizeToReportContent = true;
      reportViewer.Width = Unit.Percentage(100);
      reportViewer.Height = Unit.Percentage(100);

      return reportViewer;
    }

    private string? GetMaskedCardNum(string? cardNum)
    {
      if (string.IsNullOrEmpty(cardNum))
        return cardNum;

      string maskedCard = cardNum;

      if (cardNum.Length > 8)
      {
        string mask = new string('*', cardNum.Length - 8);
        maskedCard = $"{cardNum.Substring(0, 4)}{mask}{cardNum.Substring(cardNum.Length - 4)}";
      }

      return maskedCard;
    }
  }
}
