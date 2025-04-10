namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class AssociatedChargesController : Controller
{
  private readonly IAssociatedChargesService _associatedChargesService;

  public AssociatedChargesController(IAssociatedChargesService associatedChargesService)
  {
    _associatedChargesService = associatedChargesService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> GetAssociations()
  {
    var taxes = await _associatedChargesService.GetAssociations();
    return Ok(taxes);
  }

  public async Task<IActionResult> SaveAssociation([FromBody] AssocViewModel assocViewModel, bool isUpdate = false)
  {
    var isAddedOrUpdated = await (isUpdate ? _associatedChargesService.UpdateAssociation(assocViewModel) : _associatedChargesService.AddAssociation(assocViewModel));
    return Ok(isAddedOrUpdated);
  }

  public async Task<IActionResult> DeleteAssociation(string code)
  {
    var isDeleted = await _associatedChargesService.DeleteAssociation(code);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> IsAssociationExists(string code)
  {
    var alreadyExists = await _associatedChargesService.IsAssociationExists(code);
    return Ok(alreadyExists);
  }

  public async Task<IActionResult> GetTaxes(string code)
  {
    var taxes = await _associatedChargesService.GetTaxes(code);
    return Ok(taxes);
  }

  public async Task<IActionResult> GetAvailableTaxes(string code)
  {
    var taxes = await _associatedChargesService.GetAvailableTaxes(code);
    return Ok(taxes);
  }

  public async Task<IActionResult> AddTax([FromBody] AssocTaxViewModel assocTax)
  {
    var isAdded = await _associatedChargesService.AddTax(assocTax);
    return Ok(isAdded);
  }

  public async Task<IActionResult> DeleteTax([FromBody] AssocTaxViewModel assocTax)
  {
    var isDeleted = await _associatedChargesService.DeleteTax(assocTax);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> IsChargeInUse(string code)
  {
    var isChargeInUse = await _associatedChargesService.IsChargeInUse(code);
    return Ok(isChargeInUse);
  }
}
