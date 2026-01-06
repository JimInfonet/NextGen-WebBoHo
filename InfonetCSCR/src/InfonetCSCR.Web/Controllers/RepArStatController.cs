using InfonetCSCR.Core.Dtos;

namespace InfonetCSCR.Web.Controllers
{
    [Authorize]
    public class RepArStatController : Controller
    {
        private readonly ILogger<RepArStatController> _logger;
        //private readonly IPolicyService _policyService;
        //private readonly IApplicationConfig _applicationConfig;
        private readonly IRepArStatService _repArStatService;
        private readonly IHttpService _http;

        public RepArStatController(
              ILogger<RepArStatController> logger,
              //IPolicyService policyService,
              //IApplicationConfig applicationConfig,
              IRepArStatService repArStatService,
              IHttpService http)
        {
            _logger = logger;
            //_policyService = policyService;
            //_applicationConfig = applicationConfig;
            _repArStatService = repArStatService;
              _http = http;
        }
        public IActionResult Index()
        {
            var policies = _repArStatService.GetPolicies();

            ViewData["CUST_MEM"] = policies.CustMem;
            return View();
        }
        public async Task<IActionResult> GetAllCustomers()
        {
            var repArStats = await _repArStatService.GetAllCustomers();

            return Json(repArStats);

        }
        public async Task<IActionResult> GetStores()
        {
            IEnumerable<StoreViewModel>? stores = null;

            var url = "store/GetAllStores";
            var response = await _http.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                stores = await response.Content.ReadFromJsonAsync<IEnumerable<StoreViewModel>>();
            }
            else
            {
                _logger.LogError(Convert.ToString(response.StatusCode));
            }

            return Json(stores);
        }

        // Get api/RepArStat/GetRepArStat
        //[HttpGet]
        //[Route("GetRepArStat")]
        public async Task<ArStatementDto> GetRepArStat(String client, DateTime dDate)
        {
            //ViewBag.Today = DateTime.Today;

            try
            {
                var results = await _repArStatService.GetRepArStat(client, dDate);

                return results ?? new ArStatementDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                return new ArStatementDto();
            }
        }

        // Get api/RepArStat/GenMonthlyClosing
        //[HttpGet]
        //[Route("GenMonthlyClosing")]
        public async Task<bool> GenMonthlyClosing(String client, DateTime dDate)
        {
            //ViewBag.Today = DateTime.Today;

            try
            {
                var results = await _repArStatService.GenMonthlyClosing(client, dDate);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                return false;
            }
        }


        //public IActionResult GetExportToExcel()
        //{
        //    var repData = _repArStatService.GetExportToExcel();

        //    var content = repData;
        //    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //    var fileName = "ArStatement.xlsx";

        //    // Add the content disposition header to prompt download
        //    Response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");

        //    return File(content, contentType, fileName); // Ensure this returns IActionResult
        //}
        //public IActionResult GetPrintReport([FromBody] GetRepArStatDto model)
        //{
        //    var repData = _repArStatService.GetPrintReport(model);

        //    return Ok();
        //}

        //public async Task<IActionResult> GetShiftNos(string shiftdate)
        //{
        //    DateTime sDate = DateTime.Parse(shiftdate);

        //    var shiftNos = await _repArStatService.GetShiftNos(sDate);

        //    return Json(shiftNos);

        //}

        //public async Task<String> GetRepByDates([FromBody] GetRepArStatDto model)
        //{
        //    var repData = await _repArStatService.GetRepByDates(model);

        //    return repData; ;

        //}

        //public async Task<IActionResult> GetRepArStats(string date1, string date2)
        //{
        //    DateTime dDate1 = DateTime.Parse(date1);
        //    DateTime dDate2 = DateTime.Parse(date2);
        //    var repArStats = await _repArStatService.GetArStats(dDate1, dDate2);

        //    return Json(repArStats);

        //}

        //public Policies? GetPolicies()
        //{
        //    Policies? pol = _repArStatService.GetPolicies();

        //    return pol;
        //}
    }
}
