using InfonetCSCR.Core.Dtos;

namespace InfonetCSCR.Web.Controllers
{
    [Authorize]
    public class RepTillCloseController : Controller
    {
        private readonly ILogger<RepTillCloseController> _logger;
        private readonly IPolicyService _policyService;
        private readonly IApplicationConfig _applicationConfig;
        private readonly IRepTillCloseService _repTillCloseService;
        private readonly IHttpService _http;

        public RepTillCloseController(ILogger<RepTillCloseController> logger,
            IPolicyService policyService, IApplicationConfig applicationConfig,
            IRepTillCloseService repTillCloseService,
            IHttpService http)
        {
            _logger = logger;
            _policyService = policyService;
            _applicationConfig = applicationConfig;
            _repTillCloseService = repTillCloseService;
              _http = http;
        }
        public IActionResult Index()
        {
            var policies = _repTillCloseService.GetPolicies();

            ViewData["TillCDays"] = policies.TillCDays;
            return View();
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

        public IActionResult GetExportToExcel()
        {
            var repData = _repTillCloseService.GetExportToExcel();

            var content = repData;
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "TillClose.xlsx";

            // Add the content disposition header to prompt download
            Response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");

            return File(content, contentType, fileName); // Ensure this returns IActionResult
        }
        public IActionResult GetPrintReport([FromBody] GetRepTillCloseDto model)
        {
            var repData = _repTillCloseService.GetPrintReport(model);

            return Ok();
        }
        public async Task<String> GetRepByShift([FromBody] GetRepTillCloseDto model)
        {
            var repData = await _repTillCloseService.GetRepByShift(model);

            return repData;

        }

        public async Task<IActionResult> GetShiftNos(string shiftdate)
        {
            DateTime sDate = DateTime.Now;

            if (!string.IsNullOrEmpty(shiftdate))
                sDate = DateTime.Parse(shiftdate);

            var shiftNos = await _repTillCloseService.GetShiftNos(sDate);

            return Json(shiftNos);

        }

        public async Task<String> GetRepByDates([FromBody] GetRepTillCloseDto model)
        {
            model.policies = _repTillCloseService.GetPolicies();
            var repData = await _repTillCloseService.GetRepByDates(model);

            return repData; ;

        }

        public async Task<IActionResult> GetRepTillCloses(string date1, string date2)
        {
            DateTime dDate1 = DateTime.Parse(date1);
            DateTime dDate2 = DateTime.Parse(date2);
            var repTillCloses = await _repTillCloseService.GetRepTillCloses(dDate1, dDate2);

            return Json(repTillCloses);
        }

        public async Task<String> GetRepByTillNo([FromBody] GetRepTillCloseDto model)
        {
            model.policies = _repTillCloseService.GetPolicies(); ;
            string repData = await _repTillCloseService.GetRepByTillNo(model);
            //ViewBag.FileContent = repData;

            return repData;
        }
        public Policies? GetPolicies()
        {
            Policies? pol = _repTillCloseService.GetPolicies();

            return pol;
        }
    }
}
