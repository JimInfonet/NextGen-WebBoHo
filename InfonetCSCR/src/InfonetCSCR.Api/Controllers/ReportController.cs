namespace InfonetCSCR.Api.Controllers
{
    [Route("api/Report")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET api/Report/GetReportFilter
        [HttpGet]
        [Route("GetReportFilter")]
        public ReportFilter GetReportFilter()
        {
            //ViewBag.Today = DateTime.Today;
            ReportFilter reportFilter = _reportService.GetReportFilter().Result;
            //var json = JsonConvert.SerializeObject(salesAndTransData);
            return reportFilter;
        }

    }
}
