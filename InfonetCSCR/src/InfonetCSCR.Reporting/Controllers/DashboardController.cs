using InfonetCSCR.Reporting.Models;
using InfonetCSCR.Reporting.Service;
using Newtonsoft.Json;
using System;
using System.Web.Mvc;

namespace InfonetCSCR.Reporting.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly DashboardService dashboardService;
        private readonly DateTime today ;

        public DashboardController()
        {
            dashboardService = new DashboardService();
            today = DateTime.Today;
        }

        // GET: Dashboard
        public ActionResult Index()
        {
            DashboardData salesAndTransData = dashboardService.GetSalesAndTransData(today);
            ViewBag.Today = today.ToShortDateString();

            return View(salesAndTransData);
        }

        public JsonResult GetChartModelData()
        {
            DashboardChart chartData = dashboardService.GetChartModelData(today);
            var json = JsonConvert.SerializeObject(chartData);
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}