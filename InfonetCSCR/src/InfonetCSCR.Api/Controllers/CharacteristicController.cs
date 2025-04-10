namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class CharacteristicController : ControllerBase
{
  private readonly ICharacteristicService _characteristicService;

  public CharacteristicController(ICharacteristicService characteristicService)
  {
    _characteristicService = characteristicService;
  }

  // GET api/Characteristic/GetAllCharacteristics
  [HttpGet]
  [Route("GetAllCharacteristics")]
  public async Task<IActionResult> GetAllCharacteristics()
  {
    IList<Characteristic> characteristics = await _characteristicService.GetAllCharacteristics();
    return Ok(characteristics);
  }

  // POST api/Characteristic/AddCharacteristic
  [HttpPost]
  [Route("AddCharacteristic")]
  public async Task<IActionResult> AddCharacteristic([FromBody] Characteristic characteristic)
  {
    bool isAdded = await _characteristicService.AddCharacteristic(characteristic);
    return Ok(isAdded);
  }

  // POST api/Characteristic/UpdateCharacteristic
  [HttpPost]
  [Route("UpdateCharacteristic")]
  public async Task<IActionResult> UpdateCharacteristic([FromBody] Characteristic characteristic)
  {
    bool isUpdated = await _characteristicService.UpdateCharacteristic(characteristic);
    return Ok(isUpdated);
  }

  // POST api/Characteristic/DeleteCharacteristic
  [HttpPost]
  [Route("DeleteCharacteristic")]
  public async Task<IActionResult> DeleteCharacteristic([FromBody] string characteristicId)
  {
    bool isDeleted = await _characteristicService.DeleteCharacteristic(characteristicId);
    return Ok(isDeleted);
  }
}
