namespace InfonetCSCR.Api.Controllers;

[Route("api/AddItemsToThePO")]
[ApiController]
[Authorize]
public class AddItemsToThePOController : ControllerBase
{
  private readonly IAddItemsToThePOService _additemsToThePOService;

  public AddItemsToThePOController(IAddItemsToThePOService additemsToThePOService)
  {
    _additemsToThePOService = additemsToThePOService;
  }

  // GET api/AddItemsToThePO/GetVendorParts
  [HttpGet]
  [Route("GetVendorParts")]
  public async Task<IActionResult> GetVendorParts([FromQuery] string vendorId, [FromQuery] AddItemsToThePOSelectedCriteria selectedCriteria)
  {
    List<VendorPartDetailsDto> vendorParts = await _additemsToThePOService.GetVendorParts(vendorId, selectedCriteria);
    return Ok(vendorParts);
  }

  // POST api/AddItemsToThePO/GetExistingPartNos
  [HttpPost]
  [Route("GetExistingPartNos")]
  public async Task<IActionResult> GetExistingPartNos([FromBody] List<string> partNos, [FromQuery] string purchaseNo)
  {
    List<string> existingPartNos = await _additemsToThePOService.GetExistingPartNos(purchaseNo, partNos);
    return Ok(existingPartNos);
  }

  // POST api/AddItemsToThePO/AddVendorParts
  [HttpPost]
  [Route("AddVendorParts")]
  public async Task<IActionResult> AddVendorParts([FromBody] AddVendorPartsDto model)
  {
    await _additemsToThePOService.AddItemsToThePO(model);
    return Ok(true);
  }
}
