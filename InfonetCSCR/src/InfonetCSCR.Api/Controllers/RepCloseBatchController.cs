using System.Data;

namespace InfonetCSCR.Api.Controllers
{
    [Route("api/RepCloseBatch")]
    [ApiController]
    [Authorize]
    public class RepCloseBatchController : ControllerBase
    {
        private readonly IApplicationConfig _applicationConfig;
        private readonly IRepCloseBatchService _repCloseBatchService;
        private readonly ILogger<RepCloseBatchController> _logger;
        private readonly IPolicyService _policyService;

        const int PRINT_WIDTH = 40;
        public RepCloseBatchController(IApplicationConfig _applicationConfig, IRepCloseBatchService repCloseBatchService, ILogger<RepCloseBatchController> logger, IPolicyService policyService)
        {
            _applicationConfig = _applicationConfig;
            _repCloseBatchService = repCloseBatchService;
            _logger = logger;
            _policyService = policyService;
        }

        [HttpGet]
        [Route("GetRepCloseBatches")]
        public async Task<IActionResult> GetRepCloseBatches(DateTime date1, DateTime date2)
        {
            //var policies = _repCloseBatchService.GetPolicies();
            //ViewBag.Today = DateTime.Today;
            //_policyService.GetPolicy("closeBDays");

            try
            {
                IEnumerable<RepCloseBatch> repCloseBatches = await _repCloseBatchService.GetRepCloseBatches(date1, date2);
                return Ok(repCloseBatches);
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpGet]
        [Route("GetReportById")]
        public async Task<string?> GetReportById(string id)
        {
            try
            {
                var report = await _repCloseBatchService.GetReportById(id);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                return ex.Message;
            }
        }


        //[HttpPost]
        //[Route("GenerateReportData")]
        //public async Task<byte[]> GenerateReportData(GetRepCloseBatchDto param)
        //{
        //    try
        //    {
        //        byte[] reportData = await _repCloseBatchService.GenerateReportData(param.BatchNumber, param.TerminalId, param.BatchDate);

        //        return reportData;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(message: ex.Message);
        //        //return StatusCode((int)HttpStatusCode.InternalServerError);
        //        return Encoding.UTF8.GetBytes(ex.Message);
        //    }
        //}

        [HttpGet]
        [Route("GenerateReportData")]
        public async Task<DataTable> GenerateReportData(GetRepCloseBatchDto param)
        {
            DateTime date = DateTime.Now;
            try
            {
                date = date.AddDays(-param.policies.CloseBDays);

                DataTable reportData = await _repCloseBatchService.GenerateReportData(param.BatchNumber, param.BatchNumber, param.TerminalId);

                return reportData;
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);

                return null;

            }
        }

        //[HttpPost]
        //[Route("GenerateReportData")]
        //public async Task<DataTable> GenerateReportData(Policies model)
        //{
        //    try
        //    {
        //        DataTable reportData = await _repCloseBatchService.GenerateReportData(model);

        //        return reportData;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(message: ex.Message);

        //        return null;

        //    }
        //}

        //public Policies GetPolicies()
        //{
        //    Policies pol = _repCloseBatchService.GetPolicies();

        //    return pol;
        //}
    }
}
