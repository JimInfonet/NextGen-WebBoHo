namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class TaxMasterController : ControllerBase
{
  private readonly ITaxMasterService _taxMasterService;

  public TaxMasterController(ITaxMasterService taxMasterService)
  {
    _taxMasterService = taxMasterService;
  }

  // GET api/TaxMaster/GetProductTaxes
  [HttpGet]
  [Route("GetProductTaxes")]
  public async Task<IActionResult> GetProductTaxes()
  {
    IEnumerable<ProductTaxDto> productTaxes = await _taxMasterService.GetProductTaxes();
    return Ok(productTaxes);
  }


  // GET api/Tax/GetTaxes
  [HttpGet]
  [Route("GetTaxes")]
  public async Task<IActionResult> GetTaxes()
  {
    IEnumerable<TaxMaster> items = await _taxMasterService.GetTaxes();
    return Ok(items);
  }

  // GET api/Tax/AddTax
  [HttpPost]
  [Route("AddTax")]
  public async Task<IActionResult> AddTax([FromBody] TaxMaster tax)
  {
    bool isAdded = await _taxMasterService.AddTax(tax);
    return Ok(isAdded);
  }

  // GET api/Tax/UpdateTax
  [HttpPost]
  [Route("UpdateTax")]
  public async Task<IActionResult> UpdateTax([FromBody] TaxMaster tax)
  {
    bool isUpdated = await _taxMasterService.UpdateTax(tax);
    return Ok(isUpdated);
  }

  // GET api/Tax/DeleteTax
  [HttpPost]
  [Route("DeleteTax")]
  public async Task<IActionResult> DeleteTax([FromBody] string taxName)
  {
    bool isDeleted = await _taxMasterService.DeleteTax(taxName);
    return Ok(isDeleted);
  }

  // GET api/Tax/TaxNameAlreadyExists
  [HttpPost]
  [Route("TaxNameAlreadyExists")]
  public async Task<IActionResult> TaxNameAlreadyExists([FromBody] string taxName)
  {
    bool alreadyExists = await _taxMasterService.TaxNameAlreadyExists(taxName);
    return Ok(alreadyExists);
  }

  // POST api/Tax/GetTaxUsageCount
  [HttpPost]
  [Route("GetTaxUsageCount")]
  public async Task<IActionResult> GetTaxUsageCount([FromBody] string taxName, string? taxCode)
  {
    int usageCount = await _taxMasterService.GetTaxUsageCount(taxName, taxCode);
    return Ok(usageCount);
  }

  // GET api/Tax/GetTaxesOrderByTaxName
  [HttpGet]
  [Route("GetTaxesOrderByTaxName")]
  public async Task<IActionResult> GetTaxesOrderByTaxName()
  {
    return Ok(await _taxMasterService.GetTaxesOrderByTaxNameAsync());
  }
}