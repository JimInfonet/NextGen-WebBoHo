namespace InfonetCSCR.Api.Controllers;

[Route("api/AssociatedCharges")]
[ApiController]
[Authorize]
public class AssociationController : ControllerBase
{
  private readonly IAssociationService _associationService;

  public AssociationController(IAssociationService associationService)
  {
    _associationService = associationService;
  }

  // GET api/AssociatedCharges/GetAssociations
  [HttpGet]
  [Route("GetAssociations")]
  public async Task<IActionResult> GetAssociations()
  {
    IEnumerable<Assoc> items = await _associationService.GetAssociations();
    return Ok(items);
  }

  // POST api/AssociatedCharges/AddAssociation
  [HttpPost]
  [Route("AddAssociation")]
  public async Task<IActionResult> AddAssociation([FromBody] Assoc assoc)
  {
    bool isAdded = await _associationService.AddAssociation(assoc);
    return Ok(isAdded);
  }

  // POST api/AssociatedCharges/UpdateAssociation
  [HttpPost]
  [Route("UpdateAssociation")]
  public async Task<IActionResult> UpdateAssociation([FromBody] Assoc assoc)
  {
    bool isUpdated = await _associationService.UpdateAssociation(assoc);
    return Ok(isUpdated);
  }

  // POST api/AssociatedCharges/DeleteAssociation
  [HttpPost]
  [Route("DeleteAssociation")]
  public async Task<IActionResult> DeleteAssociation([FromBody] string code)
  {
    bool isDeleted = await _associationService.DeleteAssociation(code);
    return Ok(isDeleted);
  }

  // POST api/AssociatedCharges/IsAssociationExists
  [HttpPost]
  [Route("IsAssociationExists")]
  public async Task<IActionResult> IsAssociationExists([FromBody] string code)
  {
    bool alreadyExists = await _associationService.IsAssociationExists(code);
    return Ok(alreadyExists);
  }

  // POST api/AssociatedCharges/GetTaxes
  [HttpPost]
  [Route("GetTaxes")]
  public async Task<IActionResult> GetTaxes([FromBody] string code)
  {
    var taxes = await _associationService.GetTaxes(code);
    return Ok(taxes);
  }

  // POST api/AssociatedCharges/GetAvailableTaxes
  [HttpPost]
  [Route("GetAvailableTaxes")]
  public async Task<IActionResult> GetAvailableTaxes([FromBody] string code)
  {
    var taxes = await _associationService.GetAvailableTaxes(code);
    return Ok(taxes);
  }

  // POST api/AssociatedCharges/AddTax
  [HttpPost]
  [Route("AddTax")]
  public async Task<IActionResult> AddTax([FromBody] AssocTax assocTax)
  {
    var taxes = await _associationService.AddTax(assocTax);
    return Ok(taxes);
  }

  // POST api/AssociatedCharges/DeleteTax
  [HttpPost]
  [Route("DeleteTax")]
  public async Task<IActionResult> DeleteTax([FromBody] AssocTax assocTax)
  {
    var taxes = await _associationService.DeleteTax(assocTax);
    return Ok(taxes);
  }

  // POST api/AssociatedCharges/IsChargeInUse
  [HttpPost]
  [Route("IsChargeInUse")]
  public async Task<IActionResult> IsChargeInUse([FromBody] string code)
  {
    var isChargeInUse = await _associationService.IsChargeInUse(code);
    return Ok(isChargeInUse);
  }
}
