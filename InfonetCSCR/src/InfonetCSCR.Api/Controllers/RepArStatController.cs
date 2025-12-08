using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace InfonetCSCR.Api.Controllers
{
    [Route("api/RepArStat")]
    [ApiController]
    
    public class RepArStatController : ControllerBase
    {
        private readonly IApplicationConfig _applicationConfig;
        private readonly IRepArStatService _repArStatService;
        private readonly ILogger<RepArStatController> _logger;
        private readonly IPolicyService _policyService;

        const int PRINT_WIDTH = 40;
        public RepArStatController(IApplicationConfig applicationConfig, IRepArStatService repArStatService, ILogger<RepArStatController> logger, IPolicyService policyService)
        {
            _applicationConfig = applicationConfig;
            _repArStatService = repArStatService;
            _logger = logger;
            _policyService = policyService;
        }
        // GET api/Customers/GetAllCustomers
        [HttpGet]
        [Route("GetAllCustomers")]
        public async Task<IActionResult> GetAllCustomers(bool orderByCode)
        {
            IEnumerable<CustomersDto> customers = await _repArStatService.GetAllCustomers(orderByCode);
            return Ok(customers);
        }

        // Get api/RepArStat/GetRepArStat
        [HttpGet]
        [Route("GetRepArStat")]                
        public async Task<ArStatementDto> GetRepArStat(String client, DateTime dDate)
        {
            //ViewBag.Today = DateTime.Today;

            try
            {
                ArStatementDto results = await _repArStatService.GetRepArStat(client, dDate);

                return results ?? new ArStatementDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                return new ArStatementDto();
            }
        }

        // Get api/RepArStat/GenMonthClosing
        [HttpGet]
        [Route("GenMonthClosing")]
        public async Task<bool> GenMonthClosing(String client, DateTime dDate)
        {
            //ViewBag.Today = DateTime.Today;

            try
            {
                return await _repArStatService.GenMonthClosing(client, dDate);

            }
            catch (Exception ex)
            {
                _logger.LogError(message: ex.Message);
                return false;
            }
        }
    }
}
