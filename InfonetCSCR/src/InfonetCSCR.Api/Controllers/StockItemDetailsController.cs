namespace InfonetCSCR.Api.Controllers;

[Route("api/StockItemDetails")]
[ApiController]
[Authorize]

public class StockItemDetailsController : ControllerBase
{
  private readonly IStockItemDetailsService _service;

  public StockItemDetailsController(IStockItemDetailsService service)
  {
    _service = service;
  }

  [HttpPost]
  [Route("GetStockItemDetails")]
  public async Task<IActionResult> GetStockItemDetails(StockItemDetailsCommonDto model)
  {
    var data = await _service.GetStockItemDetails(model.StockCode, model.StoreID);
    return Ok(data);
  }

  [HttpPost]
  [Route("GetAssocNotInStockCodeAc")]
  public async Task<IActionResult> GetAssocNotInStockCodeAc(StockItemDetailsCommonDto model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    return Ok(await _service.GetAssocNotInStockCodeAc(model));
  }

  [HttpPost]
  [Route("GetTaxesAndCode")]
  public async Task<IActionResult> GetTaxesAndCode(StockItemDetailsCommonDto model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    return Ok(await _service.GetTaxesAndCode(model));
  }

  [HttpGet]
  [Route("GetChargesAndDeposits")]
  public async Task<IActionResult> GetChargesAndDeposits(string stockCode, string storeID)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    return Ok(await _service.GetChargesAndDeposits(stockCode, storeID));
  }

  // POST api/StockItemDetails/DeleteCharge
  [HttpPost]
  [Route("DeleteCharge")]
  public async Task<IActionResult> DeleteCharge([FromBody] ChargesDto model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    return Ok(await _service.DeleteCharge(model));
  }

  [HttpPost]
  [Route("InsertCharge")]
  public async Task<IActionResult> InsertCharge(ChargesDto model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    return Ok(await _service.InsertCharge(model));
  }

  [HttpPost]
  [Route("InsertTaxes")]
  public async Task<IActionResult> InsertTaxes(Tax model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    return Ok(await _service.InsertTaxes(model));
  }

  // POST api/StockItemDetails/DeleteTaxes
  [HttpPost]
  [Route("DeleteTaxes")]
  public async Task<IActionResult> DeleteTaxes([FromBody] Tax model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    return Ok(await _service.DeleteTaxes(model));
  }

  //POST  api/StockItemDetails/InsertStockAvailabilityByDay
  [HttpPost]
  [Route("InsertStockAvailabilityByDay")]
  public async Task<IActionResult> InsertStockAvailabilityByDay(StockCodeAndAvailabilityByDay model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    return Ok(await _service.InsertStockAvailabilityByDay(model));
  }
}
