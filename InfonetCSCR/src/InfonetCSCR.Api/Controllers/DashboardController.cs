using Newtonsoft.Json;

namespace InfonetCSCR.Api.Controllers
{
    [Route("api/Dashboard")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private IDashboardChartService _dashboardChartService;

        public DashboardController(IDashboardChartService dashboardChartService)
        {
            _dashboardChartService = dashboardChartService;
        }

        // GET api/Dashboard/GetSalesAndTrans
        [HttpGet]
        [Route("GetSalesAndTrans")]
        public DashboardData GetSalesAndTrans(DateTime date)
        {
            //ViewBag.Today = DateTime.Today;
            DashboardData salesAndTransData = _dashboardChartService.GetSalesAndTransData(date).Result;
            //var json = JsonConvert.SerializeObject(salesAndTransData);
            return salesAndTransData;
        }

        // GET api/Dashboard/GetChartModel
        [HttpGet]
        [Route("GetChartModel")]
        public DashboardChart GetChartModel(DateTime date)
        {
            //ViewBag.Today = DateTime.Today;

            DashboardChart chartData = _dashboardChartService.GetChartModelData(date);
            return chartData;
        }
    }
}