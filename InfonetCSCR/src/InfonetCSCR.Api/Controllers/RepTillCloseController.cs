namespace InfonetCSCR.Api.Controllers
{
    [Route("api/RepTillClose")]
    [ApiController]
    [Authorize]
    public class RepTillCloseController : ControllerBase
    {
        private readonly IApplicationConfig _applicationConfig;
        private readonly IRepTillCloseService _repTillCloseService;
        private readonly ILogger<RepTillCloseController> _logger;
        private readonly IPolicyService _policyService;

        const int PRINT_WIDTH = 40;
        public RepTillCloseController(IApplicationConfig _applicationConfig, IRepTillCloseService repTillCloseService, ILogger<RepTillCloseController> logger, IPolicyService policyService)
        {
            _applicationConfig = _applicationConfig;
            _repTillCloseService = repTillCloseService;
            _logger = logger;
            _policyService = policyService;
        }

        // GET api/RepTillClose/GetRepTillCloses
        [HttpGet]
        [Route("GetRepTillCloses")]
        public async Task<IActionResult> GetRepTillCloses(DateTime date1, DateTime date2)
        {
            //ViewBag.Today = DateTime.Today;

            try
            {
                IEnumerable<RepTillClose> repTillCloses = await _repTillCloseService.GetRepTillCloses(date1, date2);
                return Ok(repTillCloses);
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        // GET api/RepTillClose/GetShiftNos
        [HttpGet]
        [Route("GetShiftNos")]
        public async Task<IActionResult> GetShiftNos(DateTime ShiftDate)
        {
            //ViewBag.Today = DateTime.Today;
            try
            {
                if (ShiftDate != DateTime.MinValue)
                {
                    IEnumerable<int> shiftNos = await _repTillCloseService.GetShiftNos(ShiftDate);
                    return Ok(shiftNos);
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        // Post api/RepTillClose/GetRepByShift
        [HttpPost]
        [Route("GetRepByShift")]
        public async Task<string[]> GetRepByShift(GetRepTillCloseDto model)
        {
            //ViewBag.Today = DateTime.Today;
            try
            {
                string[] fileContents = await _repTillCloseService.GetRepByShift(model);

                return fileContents;
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                //return StatusCode((int)HttpStatusCode.InternalServerError);
                return new string[] { ex.Message };
            }

        }

        //Post api/RepTillClose/GetRepByDates
        [HttpPost]
        [Route("GetRepByDates")]
        public async Task<string[]> GetRepByDates(GetRepTillCloseDto model)
        {
            //ViewBag.Today = DateTime.Today;
            try
            {
                string[] fileContents = await _repTillCloseService.GetRepByDates(model);

                return fileContents;
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                //return StatusCode((int)HttpStatusCode.InternalServerError);
                return new string[] { ex.Message };
            }
        }

        [HttpPost]
        [Route("GetRepByTillNo")]
        public async Task<string[]> GetRepByTillNo(GetRepTillCloseDto model)
        {
            //ViewBag.Today = DateTime.Today;
            try
            {
                string[] fileContents = await _repTillCloseService.GetRepByTillNo(model);

                return fileContents;
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                //return StatusCode((int)HttpStatusCode.InternalServerError);
                return new string[] { ex.Message };
            }
        }
    }
}
