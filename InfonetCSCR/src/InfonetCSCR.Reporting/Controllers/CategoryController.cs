using InfonetCSCR.Reporting.Models;
using InfonetCSCR.Reporting.Service;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Web.Mvc;

namespace InfonetCSCR.Reporting.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly CategoryService categoryService;
        private readonly DateTime today ;
        private readonly string companyName;

        public CategoryController()
        {
            categoryService = new CategoryService();
            companyName = "Infonet-Technology.com";
            today = DateTime.Today;
        }

        // GET: Dashboard
        public ActionResult Index()
        {
            bool.TryParse(ConfigurationManager.AppSettings["IsHeadOffice"], out bool isHeadOffice);
            ViewBag.IsHeadOffice = isHeadOffice;
            ViewBag.Today = today.ToShortDateString();

            return View();
        }
        public ActionResult GoBack()
        {
            // Redirect to another URL
            string WebBaseUrl = "https://localhost:7068";

            //PostCategory();

            return Redirect(WebBaseUrl);
        }
        public JsonResult GetReportFilter()
        {
            ReportFilter reportFilter = categoryService.GetReportFilter();

            reportFilter.Dept.Insert(0, new DeptFilterModel { Id = "All", Name = "All" });
            reportFilter.DeptWithFuel.Insert(0, new DeptFilterModel { Id = "All", Name = "All" });
            reportFilter.SubDept.Insert(0, new StockMovementFilterModel { Id = "All", Name = "All" });
            reportFilter.SubDetail.Insert(0, new StockMovementFilterModel { Id = "All", Name = "All" });
            reportFilter.StockCode.Insert(0, new StockMovementFilterModel { Id = "All", Name = "All" });
            reportFilter.Vendor.Insert(0, new FilterModel { Id = "All", Name = "All" });
            return Json(reportFilter);
        }

        [HttpGet]
        public PartialViewResult GetDailySalesReport(string date, string reportName, string store)
        {
            return PartialView("_DailySalesReport");
        }

        [HttpGet]
        public PartialViewResult LoadReports(string reportName, string store, string date1, string date2, string sdate1, string sdate2,
            string dept1, string dept2, string subdept1, string subdept2, string subdetail1, string subdetail2,
            string stock1, string stock2, string class1, string class2, string cust1, string cust2, string emp1, string emp2,
            string vendor1, string vendor2, string till1, string till2, string tax1, string tax2, string orderno, string qtyoh)
        {
            ReportViewer reportViewer;
            string reportPath = Server.MapPath("~/Reports/" + reportName + ".rdlc");
            string reportTitle = categoryService.GetReportTitle(reportName);
            string codetype = "";

            //for Dailysale report
            if (reportName == "SA7010")
            {
                return PartialView("_DailySalesReport");
            }
            //StockMovementReport
            else if (reportName == "SK4020")
            {
                reportName = "StockMovementReport";
                reportPath = Server.MapPath("~/Reports/" + reportName + ".rdlc");
                reportViewer = categoryService.GetStockMovementReport(reportName, reportPath, date1, date2, dept1, subdept1, subdetail1, stock1, store, companyName);
            }
            //NoStockMovementReport
            else if (reportName == "SK4030")
            {
                reportName = "NoStockMovementReport";
                reportPath = Server.MapPath("~/Reports/" + reportName + ".rdlc");
                reportViewer = categoryService.GetNoStockMovementReport(reportName, reportPath, date1, date2, store, companyName);
            }
            //GetHistoricalInventoryLevelReport
            //else if (reportName == "SK5020")
            //{
            //    reportName = "HistoricalInventoryReport";
            //    reportPath = Server.MapPath("~/Reports/" + reportName + ".rdlc");
            //    reportViewer = categoryService.GetHistoricalInventoryLevelReport(reportName, reportPath, date1, store, companyName);
            //}
            //for Label reports
            else if ((reportName == "SK5030") || (reportName == "SK5040"))
            {
                switch (reportName)
                {
                    case "SK5030":
                        codetype = "1";
                        reportName = "ShelfLabelEAN13L";
                        break;
                    case "SK5040":
                        codetype = "2";
                        reportName = "ShelfLabelCode39";
                        break;
                }
                reportPath = Server.MapPath("~/Reports/" + reportName + ".rdlc");
                reportViewer = categoryService.GetShelfLabelReport(reportPath, reportName, dept1, subdept1, vendor1, codetype);
            }
            //for all other reports
            else
            {
                reportViewer = categoryService.LoadDataToViewer(reportName, reportTitle, reportPath, store, companyName,
                                date1: date1, date2: date2, sdate1: sdate1, sdate2: sdate2, dept1: dept1, dept2: dept2, subdept1: subdept1, subdept2: subdept2,
                                subdetail1: subdetail1, subdetail2: subdetail2, stock1: stock1, stock2: stock2, class1: class1, class2: class2, cust1: cust1, cust2: cust2, emp1: emp1, emp2: emp2,
                                vendor1: vendor1, vendor2: vendor2, till1: till1, till2: till2, tax1: tax1, tax2: tax2, orderno: orderno, qtyoh: qtyoh);
            }
            ViewBag.RDLCReport = reportViewer;
            return PartialView("_RDLCView");
        }


    }
}