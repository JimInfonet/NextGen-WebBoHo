using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Resources;
using System.Text;
using System.Web.Configuration;
using InfonetCSCR.Reporting.Models;
using InfonetCSCR.Reporting.Properties;
using InfonetCSCR.Reporting.Repository;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;

namespace InfonetCSCR.Reporting.Service
{
    public class CategoryService
    {
        private DBHelper dbHelper;
        public CategoryService()
        {
            dbHelper = new DBHelper();
        }
        //public ReportFilter GetReportFilter(string token)
        //{
        //    return dbHelper.GetReportFilter();
        //}
        public ReportFilter GetReportFilter(string token)
        {
            string apiBaseUrl = WebConfigurationManager.AppSettings["ApiBaseUrl"];
            string url = $"{apiBaseUrl}Report/GetReportFilter";
            ReportFilter reportFilter = null;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                try
                {
                    var response = client.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string content = response.Content.ReadAsStringAsync().Result;
                        reportFilter = JsonConvert.DeserializeObject<ReportFilter>(content);
                    }
                    else
                    {
                        var result = $"{(int)response.StatusCode} ({response.ReasonPhrase})";
                        // logger.WriteEntry(result, EventLogEntryType.Error, 40);
                    }
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                }
            }
            return reportFilter;
        }
        public ReportViewer GetReport(string reportSource, string reportPath, DataTable dataTable)
        {
            ReportViewer reportViewer = new ReportViewer();

            reportViewer.LocalReport.ReportPath = reportPath;
            reportViewer.LocalReport.DataSources.Clear();
            ReportDataSource rdc = new ReportDataSource(reportSource, dataTable);
            reportViewer.LocalReport.DataSources.Add(rdc);
            reportViewer.LocalReport.Refresh();
            reportViewer.SizeToReportContent = true;
            return reportViewer;
        }
        public ReportViewer GetShelfLabelReport(string reportPath, string reportName, string dept, string subDept, string vendor, string codetype)
        {
            DataTable dataTable = dbHelper.GetShelfLabelReport(reportName, dept, subDept, vendor);
            switch (codetype)
            {
                case "1":
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        dataTable.Rows[i]["BarCode1"] = EAN13(dataTable.Rows[i]["STOCK_CODE1"].ToString());
                        dataTable.Rows[i]["BarCode2"] = EAN13(dataTable.Rows[i]["STOCK_CODE2"].ToString());
                        dataTable.Rows[i]["BarCode3"] = EAN13(dataTable.Rows[i]["STOCK_CODE3"].ToString());
                    }
                    break;
                case "2":
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        if (dataTable.Rows[i]["STOCK_CODE1"].ToString() != "")
                            dataTable.Rows[i]["STOCK_CODE1"] = "*" + dataTable.Rows[i]["STOCK_CODE1"].ToString() + "*";
                        if (dataTable.Rows[i]["STOCK_CODE2"].ToString() != "")
                            dataTable.Rows[i]["STOCK_CODE2"] = "*" + dataTable.Rows[i]["STOCK_CODE2"].ToString() + "*";
                        if (dataTable.Rows[i]["STOCK_CODE3"].ToString() != "")
                            dataTable.Rows[i]["STOCK_CODE3"] = "*" + dataTable.Rows[i]["STOCK_CODE3"].ToString() + "*";

                    }
                    break;
            }
            ReportViewer reportViewer = GetReport("ShelfLabelEAN13L", reportPath, dataTable);
            return reportViewer;
        }
        private string EAN13(string sStock_code)
        {
            string sCodeBarre = "";
            int iChecksum = 0;
            int iFirst;
            string sTemp;
            bool tableA;
            byte[] asc;
            try
            {
                //Check for 12 characters
                if (sStock_code.Length != 12)
                {
                    if (sStock_code.Length == 13)
                    {
                        sStock_code = sStock_code.Substring(1, 12);
                    }
                    else
                        return sCodeBarre;
                }
                //Check characters in sStock_code. They all should be digits.
                for (int j = 0; j < sStock_code.Length; j++)
                {
                    sTemp = sStock_code.Substring(j, 1);
                    asc = ASCIIEncoding.ASCII.GetBytes(sTemp);
                    if (asc[0] < 48 || asc[0] > 57)
                    {
                        return sCodeBarre;
                    }
                }
                //Calculate checksum
                for (int j = 11; j >= 0; j -= 2)
                {
                    iChecksum += int.Parse(sStock_code.Substring(j, 1));
                }
                iChecksum = iChecksum * 3;
                for (int j = 10; j >= 0; j -= 2)
                {
                    iChecksum += int.Parse(sStock_code.Substring(j, 1));
                }
                sStock_code = sStock_code + (10 - iChecksum % 10) % 10;

                //The first digit is taken just as it is, the second one come from table A
                sCodeBarre = sStock_code.Substring(0, 1) + (char)(65 + int.Parse(sStock_code.Substring(1, 1)));
                iFirst = int.Parse(sStock_code.Substring(0, 1));
                for (int j = 2; j < 7; j++)
                {
                    tableA = false;
                    switch (j)
                    {
                        case 2:
                            switch (iFirst)
                            {
                                case 0:
                                case 1:
                                case 2:
                                case 3:
                                    tableA = true;
                                    break;
                            }
                            break;
                        case 3:
                            switch (iFirst)
                            {
                                case 0:
                                case 4:
                                case 7:
                                case 8:
                                    tableA = true;
                                    break;
                            }
                            break;
                        case 4:
                            switch (iFirst)
                            {
                                case 0:
                                case 1:
                                case 4:
                                case 5:
                                case 9:
                                    tableA = true;
                                    break;
                            }
                            break;
                        case 5:
                            switch (iFirst)
                            {
                                case 0:
                                case 2:
                                case 5:
                                case 6:
                                case 7:
                                    tableA = true;
                                    break;
                            }
                            break;
                        case 6:
                            switch (iFirst)
                            {
                                case 0:
                                case 3:
                                case 6:
                                case 8:
                                case 9:
                                    tableA = true;
                                    break;
                            }
                            break;
                    }
                    if (tableA)
                    {
                        sCodeBarre = sCodeBarre + (char)(65 + int.Parse(sStock_code.Substring(j, 1)));
                    }
                    else
                    {
                        sCodeBarre = sCodeBarre + (char)(75 + int.Parse(sStock_code.Substring(j, 1)));
                    }
                }
                //Add middle separator
                sCodeBarre = sCodeBarre + "*";
                for (int j = 7; j < 13; j++)
                {
                    sCodeBarre = sCodeBarre + (char)(97 + int.Parse(sStock_code.Substring(j, 1)));
                }
                //Add end mark
                sCodeBarre = sCodeBarre + "+";
            }
            catch (Exception ex)
            {
                WriteLog.WriteLine("Error: Reporting.EAN13(): " + ex.Message);

            }
            return sCodeBarre;
        }
        public ReportViewer GetStockMovementReport(string reportName, string reportPath, string startDate, string endDate, string dept, string subDept, string subDetail, string stockCode, string store, string companyName)
        {
            DataTable dataTable = dbHelper.GetStockMovementReport(startDate, endDate, dept, subDept, subDetail, stockCode, store);
            ReportViewer reportViewer = GetReport("StockMovement", reportPath, dataTable);

            ReportParameter p1 = new ReportParameter("startDate", startDate);
            ReportParameter p2 = new ReportParameter("endDate", endDate);
            ReportParameter p3 = new ReportParameter("dept", dept);
            ReportParameter p4 = new ReportParameter("subDept", subDept);
            ReportParameter p5 = new ReportParameter("subDetail", subDetail);
            ReportParameter p6 = new ReportParameter("stockCode", stockCode);
            ReportParameter p7 = new ReportParameter("store", store);
            ReportParameter p8 = new ReportParameter("company", companyName);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4, p5, p6, p7, p8 });

            return reportViewer;
        }

        public ReportViewer GetNoStockMovementReport(string reportName, string reportPath, string startDate, string endDate, string store, string companyName)
        {
            DataTable dataTable = dbHelper.GetNoStockMovementReport(startDate, endDate, store);
            ReportViewer reportViewer = GetReport("NoStockMovement", reportPath, dataTable);

            ReportParameter p1 = new ReportParameter("startDate", startDate);
            ReportParameter p2 = new ReportParameter("endDate", endDate);
            ReportParameter p3 = new ReportParameter("store", store);
            ReportParameter p4 = new ReportParameter("company", companyName);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4 });

            return reportViewer;
        }

        public ReportViewer GetHistoricalInventoryLevelReport(string reportName, string reportPath, string date, string store, string companyName)
        {
            DataTable dataTable = dbHelper.GetHistoricalInventoryLevelReport(date, store);
            ReportViewer reportViewer = GetReport("HistoricalInventory", reportPath, dataTable);

            ReportParameter p1 = new ReportParameter("date", date);
            ReportParameter p2 = new ReportParameter("store", store);
            ReportParameter p3 = new ReportParameter("company", companyName);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3 });

            return reportViewer;
        }

        public ReportViewer GetDepartmentSalesReport(string reportName, string reportPath, string startDate, string endDate, string store, string companyName)
        {
            DataTable dataTable = dbHelper.GetDepartmentSalesReport(startDate, endDate, store);
            ReportViewer reportViewer = GetReport("DepartmentSales", reportPath, dataTable);

            ReportParameter p1 = new ReportParameter("startDate", startDate);
            ReportParameter p2 = new ReportParameter("endDate", endDate);
            ReportParameter p3 = new ReportParameter("store", store);
            ReportParameter p4 = new ReportParameter("company", companyName);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4 });

            return reportViewer;
        }

        public ReportViewer LoadDataToViewer(string reportName, string reportTitle, string reportPath, string store, string companyName,
            string date1, string date2, string sdate1, string sdate2, string dept1, string dept2, string subdept1, string subdept2, string subdetail1, string subdetail2,
            string stock1, string stock2, string class1, string class2, string cust1, string cust2, string emp1, string emp2,
            string vendor1, string vendor2, string till1, string till2, string tax1, string tax2, string orderno, string qtyoh)
        {
            DataTable dataTable = dbHelper.GetDataTable(reportName, date1: date1, date2: date2, sdate1: sdate1, sdate2,
                dept1: dept1, dept2: dept2, subdept1: subdept1, subdept2: subdept2, subdetail1: subdetail1,
                subdetail2: subdetail2, stock1: stock1, stock2: stock2, class1: class1, class2: class2, cust1: cust1, cust2: cust2, emp1: emp1, emp2: emp2,
                vendor1: vendor1, vendor2: vendor2, till1: till1, till2: till2, tax1: tax1, tax2: tax2, orderno: orderno, qtyoh: qtyoh);
            ReportViewer reportViewer = new ReportViewer();
            try
            {
                reportViewer.LocalReport.ReportPath = reportPath;
                reportViewer.LocalReport.DataSources.Clear();
                ReportDataSource rdc = new ReportDataSource(reportName.Substring(0, 6), dataTable);
                reportViewer.LocalReport.DataSources.Add(rdc);
                reportViewer.LocalReport.Refresh();
                reportViewer.SizeToReportContent = true;
                List<ReportParameter> paramList = new List<ReportParameter>();
                if (!String.IsNullOrEmpty(date1))
                    paramList.Add(new ReportParameter("date1", date1));
                if (!String.IsNullOrEmpty(date2))
                    paramList.Add(new ReportParameter("date2", date2));
                if (!String.IsNullOrEmpty(sdate1))
                    paramList.Add(new ReportParameter("sdate1", date1));
                if (!String.IsNullOrEmpty(sdate2))
                    paramList.Add(new ReportParameter("sdate2", date2));
                if (!String.IsNullOrEmpty(dept1))
                    paramList.Add(new ReportParameter("dept1", dept1));
                if (!String.IsNullOrEmpty(dept2))
                    paramList.Add(new ReportParameter("dept2", dept2));
                if (!String.IsNullOrEmpty(subdept1))
                    paramList.Add(new ReportParameter("subdept1", subdept1));
                if (!String.IsNullOrEmpty(subdept2))
                    paramList.Add(new ReportParameter("subdept2", subdept2));
                if (!String.IsNullOrEmpty(subdetail1))
                    paramList.Add(new ReportParameter("subdetail1", subdetail1));
                if (!String.IsNullOrEmpty(subdetail2))
                    paramList.Add(new ReportParameter("subdetail2", subdetail2));
                if (!String.IsNullOrEmpty(stock1))
                    paramList.Add(new ReportParameter("stock1", stock1));
                if (!String.IsNullOrEmpty(stock2))
                    paramList.Add(new ReportParameter("stock2", stock2));
                if (!String.IsNullOrEmpty(class1))
                    paramList.Add(new ReportParameter("class1", class1));
                if (!String.IsNullOrEmpty(class2))
                    paramList.Add(new ReportParameter("class2", class2));
                if (!String.IsNullOrEmpty(cust1))
                    paramList.Add(new ReportParameter("cust1", cust1));
                if (!String.IsNullOrEmpty(cust2))
                    paramList.Add(new ReportParameter("cust2", cust2));
                if (!String.IsNullOrEmpty(emp1))
                    paramList.Add(new ReportParameter("emp1", emp1));
                if (!String.IsNullOrEmpty(emp2))
                    paramList.Add(new ReportParameter("emp2", emp2));
                if (!String.IsNullOrEmpty(vendor1))
                    paramList.Add(new ReportParameter("vendor1", vendor1));
                if (!String.IsNullOrEmpty(vendor2))
                    paramList.Add(new ReportParameter("vendor2", vendor2));
                if (!String.IsNullOrEmpty(till1))
                    paramList.Add(new ReportParameter("till1", till1));
                if (!String.IsNullOrEmpty(till2))
                    paramList.Add(new ReportParameter("till2", till2));
                if (!String.IsNullOrEmpty(tax1))
                    paramList.Add(new ReportParameter("tax1", tax1));
                if (!String.IsNullOrEmpty(tax2))
                    paramList.Add(new ReportParameter("tax2", tax2));
                if (!String.IsNullOrEmpty(orderno))
                    paramList.Add(new ReportParameter("orderno", orderno));
                if (!String.IsNullOrEmpty(qtyoh))
                    paramList.Add(new ReportParameter("qtyoh", qtyoh));
                paramList.Add(new ReportParameter("store", store));
                paramList.Add(new ReportParameter("company", companyName));
                paramList.Add(new ReportParameter("reportTitle", reportTitle));
                paramList.Add(new ReportParameter("reportName", reportName));
                reportViewer.LocalReport.SetParameters(paramList);
                reportViewer.LocalReport.Refresh();
            }
            catch (Exception ex)
            {
                WriteLog.WriteLine("Error: " + ex.InnerException);
            }
            return reportViewer;
        }

        public ReportViewer GetHourlySalesReport(string reportName, string reportPath, string startDate, string endDate, string dept, string store, string companyName)
        {
            DataTable dataTable = dbHelper.GetHourlySalesReport(startDate, endDate, dept, store);
            ReportViewer reportViewer = GetReport("HourlySales", reportPath, dataTable);

            ReportParameter p1 = new ReportParameter("startDate", startDate);
            ReportParameter p2 = new ReportParameter("endDate", endDate);
            ReportParameter p3 = new ReportParameter("dept", dept);
            ReportParameter p4 = new ReportParameter("store", store);
            ReportParameter p5 = new ReportParameter("company", companyName);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4, p5 });

            return reportViewer;
        }

        public ReportViewer GetQuantityLeaderReport(string reportName, string reportPath, string startDate, string endDate, string store, string companyName)
        {
            DataTable dataTable = dbHelper.GetQuantityLeaderReport(startDate, endDate, store);
            ReportViewer reportViewer = GetReport("QuantityLeader", reportPath, dataTable);

            ReportParameter p1 = new ReportParameter("startDate", startDate);
            ReportParameter p2 = new ReportParameter("endDate", endDate);
            ReportParameter p3 = new ReportParameter("store", store);
            ReportParameter p4 = new ReportParameter("company", companyName);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4 });

            return reportViewer;
        }

        public ReportViewer GetDeptSalesLastSixWeeksReport(string reportName, string reportPath, string today, string store, string companyName)
        {
            DataTable dataTable = dbHelper.GetDeptSalesLastSixWeeksReport(today, store);
            //ReportViewer reportViewer = GetReport("DeptSalesLst6Weeks", reportPath, dataTable);
            ReportViewer reportViewer = GetReport(reportName, reportPath, dataTable);

            ReportParameter p1 = new ReportParameter("today", today);
            ReportParameter p2 = new ReportParameter("store", store);
            ReportParameter p3 = new ReportParameter("company", companyName);
            reportViewer.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3 });

            return reportViewer;
        }

        public List<FilterModel> GetStoreList()
        {
            return dbHelper.GetStoreList();
        }
        public List<ReportFilterModel> GetReportFilterList()
        {
            return dbHelper.GetReportFilterList();
        }
        public ReportFilter GetReportFilter()
        {
            return dbHelper.GetReportFilter();
        }

        public string GetReportTitle(string reportName)
        {
            return dbHelper.GetReportTitle(reportName);
        }
        public SalesReportViewModel GetSalesReportData(DateTime date, string store)
        {
            SalesReportViewModel salesReportViewModel = dbHelper.GetSalesReportData(date, store);
            salesReportViewModel.DateTime = DateTime.Now;
            return salesReportViewModel;
        }

        public List<SalesDetails> GetSalesDetails(DateTime date, string dept, string stockCode, string store)
        {
            return dbHelper.GetSalesDetails(date, dept, stockCode, store);
        }

        public List<SalesTaxDetails> GetSalesTaxesDetails(DateTime date, string taxName, string store)
        {
            return dbHelper.GetSalesTaxesDetails(date, taxName, store);
        }

        public List<SaleChargeDetails> GetSalesChargesDetails(DateTime date, string chargeName, string store)
        {
            return dbHelper.GetSalesChargesDetails(date, chargeName, store);
        }

        public List<SaleReceiptDetails> GetSalesReceiptDetails(DateTime date, string tendName, string store)
        {
            return dbHelper.GetSalesReceiptDetails(date, tendName, store);
        }
    }
}
