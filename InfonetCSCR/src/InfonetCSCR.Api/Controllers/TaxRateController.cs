namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class TaxRateController : ControllerBase
{
  private readonly ITaxRateService _taxRateService;

  public TaxRateController(ITaxRateService taxService)
  {
    _taxRateService = taxService;
  }

  // GET api/TaxRate/GetTaxRatesByTaxName
  [HttpPost]
  [Route("GetTaxRatesByTaxName")]
  public async Task<IActionResult> GetTaxRatesByTaxName([FromBody] string taxName)
  {
    var taxRates = await _taxRateService.GetTaxRatesByTaxName(taxName);
    return Ok(taxRates);
  }

  // POST api/TaxRate/AddTaxRate
  [HttpPost]
  [Route("AddTaxRate")]
  public async Task<IActionResult> AddTaxRate([FromBody] TaxRate taxRate)
  {
    bool isAdded = await _taxRateService.AddTaxRate(taxRate);
    return Ok(isAdded);
  }

  // POST api/TaxRate/UpdateTaxRate
  [HttpPost]
  [Route("UpdateTaxRate")]
  public async Task<IActionResult> UpdateTaxRate([FromBody] TaxRate taxRate)
  {
    bool isUpdated = await _taxRateService.UpdateTaxRate(taxRate);
    return Ok(isUpdated);
  }

  // POST api/TaxRate/DeleteTaxRate
  [HttpPost]
  [Route("DeleteTaxRate")]
  public async Task<IActionResult> DeleteTaxRate(string taxName, string taxCode)
  {
    bool isDeleted = await _taxRateService.DeleteTaxRate(taxName, taxCode);
    return Ok(isDeleted);
  }

  // POST api/TaxRateCodeAlreadyExists
  [HttpPost]
  [Route("TaxCodeAlreadyExists")]
  public async Task<IActionResult> TaxCodeAlreadyExists([FromBody] TaxRate taxRate)
  {
    bool alreadyExists = await _taxRateService.TaxCodeAlreadyExists(taxRate);
    return Ok(alreadyExists);
  }
}
