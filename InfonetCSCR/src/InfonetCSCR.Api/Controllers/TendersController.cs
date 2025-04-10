namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class TendersController : ControllerBase
{
  private readonly IApplicationConfig _applicationConfig;
  private readonly ITillsSaleTendService _tillsSaleTendService;
  private readonly ITransSaleTendService _transSaleTendService;
  private readonly ITendersService _tendersService;

  public TendersController(
    ITendersService tendersService,
    IApplicationConfig applicationConfig,
    ITillsSaleTendService tillsSaleTendService,
    ITransSaleTendService transSaleTendService)
  {
    _tillsSaleTendService = tillsSaleTendService;
    _transSaleTendService = transSaleTendService;
    _tendersService = tendersService;
    _applicationConfig = applicationConfig;
  }

  // GET api/Tenders/GetTendersWithCardCode
  [HttpGet]
  [Route("GetTendersWithCardCode")]
  public async Task<IActionResult> GetTendersWithCardCode()
  {
    IEnumerable<Tender> tenders = await _tendersService.GetTendersWithCardCodeAsync();
    return Ok(tenders);
  }

  [HttpGet]
  [Route("GetSingleTender")]
  public async Task<IActionResult> GetSingleTender(string tendCode)
  {
    Tender tender = await _tendersService.GetSingleTender(tendCode);
    return Ok(tender);
  }

  [HttpGet]
  [Route("GetPayout")]
  public async Task<IActionResult> GetPayout()
  {
    IEnumerable<Payout> payouts = await _tendersService.GetPayout();
    return Ok(payouts);
  }

  [HttpGet]
  [Route("GetTenderClasses")]
  public async Task<IActionResult> GetTenderClasses()
  {
    IEnumerable<TenderClass> tenderClasses = await _tendersService.GetTenderClasses();
    return Ok(tenderClasses);
  }

  [HttpPost]
  [Route("UpdateTender")]
  public async Task<IActionResult> UpdateTender(Tender tender)
  {
    bool isUpdated = await _tendersService.UpdateTender(tender);
    return Ok(isUpdated);
  }

  [HttpPost]
  [Route("AddTender")]
  public async Task<IActionResult> AddTender(Tender tender)
  {
    bool isAdded = await _tendersService.AddTender(tender);
    return Ok(isAdded);
  }

  [HttpGet]
  [Route("CheckTranscationExistsForTendName/{tendName}")]
  public async Task<IActionResult> CheckTranscationExistsForTendName(string tendName)
  {
    bool isTranscationExistsForTendName = false;
    if (_applicationConfig.IsBackOffice)
    {
      isTranscationExistsForTendName = await _tillsSaleTendService.CheckTranscationExistsForTendName(tendName);
      if (isTranscationExistsForTendName)
      {
        return Ok(isTranscationExistsForTendName);
      }
    }

    isTranscationExistsForTendName = await _transSaleTendService.CheckTranscationExistsForTendName(tendName);
    return Ok(isTranscationExistsForTendName);
  }

  // POST api/tenders/DeleteTender
  [HttpPost]
  [Route("DeleteTender")]
  public async Task<IActionResult> DeleteTender([FromBody] string tendCode)
  {
    bool isTenderDeleted = await _tendersService.DeleteTender(tendCode);
    return Ok(isTenderDeleted);
  }

  [HttpGet]
  [Route("isTenderCodeExist")]
  public async Task<IActionResult> IsTenderCodeExist(string tendCode)
  {
    bool isTenderCodeExist = await _tendersService.IsTenderCodeExist(tendCode);
    return Ok(isTenderCodeExist);
  }

  [HttpGet]
  [Route("isFullNameExist")]
  public async Task<IActionResult> IsFullNameExist(string fullName)
  {
    bool isFullNameExist = await _tendersService.IsFullNameExist(fullName);
    return Ok(isFullNameExist);
  }

  [HttpGet]
  [Route("ResetSequence")]
  public async Task<IActionResult> ResetSequence()
  {
    bool isSequenceReseted = await _tendersService.ResetSequence();
    return Ok(isSequenceReseted);
  }

  // GET api/Tenders/GetTendersList
  [HttpGet]
  [Route("GetTendersList")]
  public async Task<IActionResult> GetTendersList()
  {
    return Ok(await _tendersService.GetTendersListAsync());
  }

  // GET api/Tenders/GetTendersListByClassName
  [HttpGet]
  [Route("GetTendersListByClassName")]
  public async Task<IActionResult> GetTendersListByClassName(string tendClass)
  {
    return Ok(await _tendersService.GetTendersListByClassNameAsync(tendClass));
  }
}
