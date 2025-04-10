namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class TaxController : Controller
{
  private readonly ITaxService _taxService;

  public TaxController(ITaxService taxService)
  {
    _taxService = taxService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public IActionResult GetPolicies()
  {
    return Json(_taxService.GetTaxPolicies());
  }

  public async Task<IActionResult> GetTaxes()
  {
    var taxes = await _taxService.GetTaxes();
    return Ok(taxes);
  }

  public async Task<IActionResult> SaveTax([FromBody] TaxMasterViewModel tax, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _taxService.UpdateTax(tax) : _taxService.AddTax(tax));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteTax(string taxName)
  {
    var isDeleted = await _taxService.DeleteTax(taxName);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> TaxNameAlreadyExists(string taxName)
  {
    var alreadyExists = await _taxService.TaxNameAlreadyExists(taxName);
    return Ok(alreadyExists);
  }

  public async Task<IActionResult> GetTaxUsageCount(string taxName, string? taxCode)
  {
    var taxUsageCount = await _taxService.GetTaxUsageCount(taxName, taxCode);
    return Ok(taxUsageCount);
  }

  public async Task<IActionResult> GetTaxRatesByTaxName(string taxName)
  {
    var taxRates = await _taxService.GetTaxRatesByTaxName(taxName);
    return Ok(taxRates);
  }

  public async Task<IActionResult> SaveTaxRate([FromBody] TaxRateViewModel taxRate, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _taxService.UpdateTaxRate(taxRate) : _taxService.AddTaxRate(taxRate));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteTaxRate(string taxName, string taxCode)
  {
    var isDeleted = await _taxService.DeleteTaxRate(taxName, taxCode);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> TaxCodeAlreadyExists(string taxName, string taxCode)
  {
    var alreadyExists = await _taxService.TaxCodeAlreadyExists(taxName, taxCode);
    return Ok(alreadyExists);
  }

  public async Task<IActionResult> GetTaxExemptReasonsListByType(string reasonType)
  {
    var taxExemptReasonsList = await _taxService.GetTaxExemptReasonsListByTypeAsync(reasonType);
    return Ok(taxExemptReasonsList);
  }

  public async Task<IActionResult> GetTaxesOrderByTaxName()
  {
    var taxesList = await _taxService.GetTaxesOrderByTaxNameAsync();
    return Ok(taxesList);
  }
}
