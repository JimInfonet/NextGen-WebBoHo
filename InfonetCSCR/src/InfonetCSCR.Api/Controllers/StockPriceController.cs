namespace InfonetCSCR.Api.Controllers;

[Route("api/StockPrice")]
[ApiController]
[Authorize]

public class StockPriceController : ControllerBase
{
  private readonly IStockPriceService _stockPriceService;

  public StockPriceController(IStockPriceService stockPriceService)
  {
    _stockPriceService = stockPriceService;
  }

  [HttpPost]
  public async Task<IActionResult> GetStockPrice(StockPriceDto dto, bool isUseARInvoicePolicy = false)
  {

    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.GetStockPrice(dto, isUseARInvoicePolicy);
    return Ok(data);
  }

  // GET api/StockPrice/GetStockPriceInfo
  [HttpGet]
  [Route("GetStockPriceInfo")]
  public async Task<IActionResult> GetStockPriceInfo([FromQuery] GetStockPriceInfoDto model)
  {
    StockPriceInfo stockPriceInfo = await _stockPriceService.GetStockPriceInfo(model);
    return Ok(stockPriceInfo);
  }

  // GET api/StockPrice/GetStockPriceInfoBatch
  [HttpPost]
  [Route("GetStockPriceInfoBatch")]
  public async Task<IActionResult> GetStockPriceInfoBatch([FromBody] List<GetStockPriceInfoDto> models)
  {
    IEnumerable<StockPriceInfo> stockPrices = await _stockPriceService.GetStockPriceInfoBatch(models);
    return Ok(stockPrices);
  }

  [HttpGet]
  [Route("PriceGroup")]
  public async Task<IActionResult> GetPriceGroup()
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.GetPriceGroup();
    return Ok(data);
  }

  [HttpPost]
  [Route("UpdateVendorPartNo")]
  public async Task<IActionResult> UpdateVendorPartNo(VendorPart model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.UpdateVendorPartNo(model);
    return Ok(data);
  }

  [HttpPost]
  [Route("ToggleVendorStatus")]
  public async Task<IActionResult> ToggleVendorStatus(StockPriceModel model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.ToggleVendorStatus(model);
    return Ok(data);
  }

  [HttpPost]
  [Route("AddVendorStockCost")]
  public async Task<IActionResult> AddVendorToStockCost(StockCosts model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.AddVendorToStockCost(model);
    return Ok(data);
  }

  // POST api/StockPrice/DeleteVendor
  [HttpPost]
  [Route("DeleteVendor")]
  public async Task<IActionResult> DeleteVendor([FromBody] StockPriceModel model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.DeleteVendor(model);
    return Ok(data);
  }

  // POST api/StockPrice/UpdateStockCost
  [HttpPost]
  [Route("UpdateStockCost")]
  public async Task<IActionResult> UpdateStockCost(StockCosts model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.UpdateStockCost(model);
    return Ok(data);
  }

  // POST api/StockPrice/InsertFutureStockCosts
  [HttpPost]
  [Route("InsertFutureStockCosts")]
  public async Task<IActionResult> InsertFutureStockCosts(StockCostsList model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.InsertFutureStockCosts(model);
    return Ok(data);
  }

  [HttpPost]
  [Route("UpdateStockPrice")]
  public async Task<IActionResult> UpdateStockPrice(PriceModel model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.UpdateStockPrice(model);
    return Ok(data);
  }

  [HttpPost]
  [Route("InsertFuturePrice")]
  public async Task<IActionResult> InsertFuturePrice(PriceModel model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.InsertFuturePrice(model);
    return Ok(data);
  }

  [HttpPost]
  [Route("AddRebate")]
  public async Task<IActionResult> AddRebate(StockRebates model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.AddRebate(model);
    return Ok(data);
  }

  // POST api/StockPrice/DeleteRebate
  [HttpPost]
  [Route("DeleteRebate")]
  public async Task<IActionResult> DeleteRebate([FromBody] StockRebates model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.DeleteRebate(model);
    return Ok(data);
  }

  [HttpPost]
  [Route("AddSpecialPrice")]
  public async Task<IActionResult> AddSpecialPrice(StockSpecialPrice model, bool isUseARInvoice, bool? isSetVendor)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.AddSpecialPrice(model, isUseARInvoice, isSetVendor);
    return Ok(data);
  }

  [HttpPost]
  [Route("UpdateSpecialPriceForUseARInvoicePolicy")]
  public async Task<IActionResult> UpdateSpecialPriceForUseARInvoicePolicy(StockSpecialPrice model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.UpdateSpecialPriceForUseARInvoicePolicy(model);
    return Ok(data);
  }

  // POST api/StockPrice/DeleteSpecialPrice
  [HttpPost]
  [Route("DeleteSpecialPrice")]
  public async Task<IActionResult> DeleteSpecialPrice([FromBody] StockSpecialPrice model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.DeleteSpecialPrice(model, model.UseArInvoice);
    return Ok(data);
  }

  [HttpPost]
  [Route("VendorsNotInStockCost")]
  public async Task<IActionResult> GetVendorsNotInStockCost(StockPriceModel model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest();
    }

    var data = await _stockPriceService.GetVendorsNotInStockCost(model);
    return Ok(data);
  }

  [HttpGet]
  [Route("GetVendorPart")]
  public async Task<IActionResult> GetVendorPart(string? stockCode, string? vendorId)
  {
    VendorPart? vendorPart = await _stockPriceService.GetVendorPart(stockCode, vendorId);
    return Ok(vendorPart);
  }
}
