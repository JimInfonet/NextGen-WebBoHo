namespace InfonetCSCR.Api.Controllers;

[Route("api/Kits")]
[ApiController]
[Authorize]

public class KitsController : ControllerBase
{
  public readonly IKitMasterService _kitMasterService;

  public KitsController(IKitMasterService kitMasterService)
  {
    _kitMasterService = kitMasterService;
  }

  // GET api/Kits/GetKits
  [HttpGet]
  [Route("GetKits")]
  public async Task<IActionResult> GetKits()
  {
    IEnumerable<KitsDto> kits = await _kitMasterService.GetKits();
    return Ok(kits);
  }

  // GET api/Kits/GetKitItemsByKitCodeWithDesc
  [HttpGet]
  [Route("GetKitItemsByKitCodeWithDesc")]
  public async Task<IActionResult> GetKitItemsByKitCodeWithDesc(string stockCode)
  {
    IEnumerable<KitItemsDto> kitItems = await _kitMasterService.GetKitItemsByKitCodeWithDesc(stockCode);
    return Ok(kitItems);
  }

  // GET api/Kits/GetKitItems
  [HttpGet]
  [Route("GetKitItems")]
  public async Task<IActionResult> GetKitItems(bool sellInactivePolicy)
  {
    IEnumerable<KitItemsDto> kitItems = await _kitMasterService.GetKitItems(sellInactivePolicy);
    return Ok(kitItems);
  }

  // POST api/Kits/AddKitItem
  [HttpPost]
  [Route("AddKitItem")]
  public async Task<IActionResult> AddKitItem([FromBody] KitItemsDto kitItem)
  {
    bool isKitItemAdded = await _kitMasterService.AddKitItem(kitItem);
    return Ok(isKitItemAdded);
  }

  // GET api/Kits/IsKitItem
  [HttpGet]
  [Route("IsKitItem")]
  public async Task<IActionResult> IsKitItem(string stockCode)
  {
    bool isKitItem = await _kitMasterService.IsKitItem(stockCode);
    return Ok(isKitItem);
  }

  // POST api/Kits/IsItemAlreadyInKit
  [HttpPost]
  [Route("IsItemAlreadyInKit")]
  public async Task<IActionResult> IsItemAlreadyInKit([FromBody] KitItemsDto kitItem)
  {
    bool isItemAlreadyInKit = await _kitMasterService.IsItemAlreadyInKit(kitItem);
    return Ok(isItemAlreadyInKit);
  }

  // POST api/Kits/UpdateKitItem
  [HttpPost]
  [Route("UpdateKitItem")]
  public async Task<IActionResult> UpdateKitItem([FromBody] KitItemsDto kitItem)
  {
    bool isKitItemUpdated = await _kitMasterService.UpdateKitItem(kitItem);
    return Ok(isKitItemUpdated);
  }

  // POST api/Kits/DeleteKitItem
  [HttpPost]
  [Route("DeleteKitItem")]
  public async Task<IActionResult> DeleteKitItem([FromBody] KitItemsDto kitItem)
  {
    bool isKitItemDeleted = await _kitMasterService.DeleteKitItem(kitItem);
    return Ok(isKitItemDeleted);
  }
}
