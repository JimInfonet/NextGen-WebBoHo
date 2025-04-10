namespace InfonetCSCR.Api.Controllers;

[Route("api/PricebookTransfer")]
[ApiController]
[Authorize]

public class PricebookTransferController : Controller
{
  private readonly IPricebookTransferService _pricebookTransferService;
  private readonly IStockTransferService _stockTransferService;

  public PricebookTransferController(
    IStockTransferService stockTransferService,
    IPricebookTransferService pricebookTransferService)
  {
    _stockTransferService = stockTransferService;
    _pricebookTransferService = pricebookTransferService;
  }

  // GET api/PricebookTransfer/GetPendingTransferStores
  [HttpGet]
  [Route("GetPendingTransferStores")]
  public async Task<IActionResult> GetPendingTransferStores(string dataTransferType)
  {
    IEnumerable<Store> stores = await _pricebookTransferService.GetPendingTransferStores(dataTransferType);
    return Ok(stores);
  }

  // POST api/PricebookTransfer/PendingTransfers
  [HttpPost]
  [Route("PendingTransfers")]
  public async Task<IActionResult> PendingTransfers([FromBody] PendingTransferDto model)
  {
    if (model.IsStockChangeSelected == true)
    {
      await _pricebookTransferService.StockPendingTransfers(model);
    }
    else
    {
      await _pricebookTransferService.PendingTransfers(model);
    }
    return Ok();
  }

  // POST api/PricebookTransfer/PendingCommodityTransfers
  [HttpPost]
  [Route("PendingCommodityTransfers")]
  public async Task<IActionResult> PendingCommodityTransfers([FromBody] PendingCommodityTransferDto model, string? userCode)
  {
    await _pricebookTransferService.PendingCommodityTransfers(model, userCode);
    return Ok();
  }

  // POST api/PricebookTransfer/PendingPromotionTransfers
  [HttpPost]
  [Route("PendingPromotionTransfers")]
  public async Task<IActionResult> PendingPromotionTransfers([FromBody] PendingPromotionTransferDto model, string? userCode)
  {
    await _pricebookTransferService.PendingPromotionTransfers(model, userCode);
    return Ok();
  }

  [HttpPost]
  [Route("PendingHotButtonsTransfers")]
  public async Task<IActionResult> PendingHotButtonsTransfers([FromBody] PendingHotButtonsTransferDto model, string? userCode)
  {
    await _pricebookTransferService.PendingHotButtonsTransfers(model, userCode);
    return Ok();
  }

  // POST api/PricebookTransfer/DeleteFromTransferList
  [HttpPost]
  [Route("DeleteFromTransferList")]
  public async Task<IActionResult> DeleteFromTransferList([FromBody] DeleteFromTransferListDto model)
  {
    await _pricebookTransferService.DeleteFromTransferList(model);
    return Ok();
  }

  // POST api/PricebookTransfer/PerformTransfer
  [HttpPost]
  [Route("PerformTransfer")]
  public async Task<IActionResult> PerformTransfer(PricebookTransferPoliciesDto policies, [FromQuery] string dataTransferType, [FromQuery] string userCode)
  {
    if (dataTransferType == "stocks")
    {
      await _pricebookTransferService.StockTransfer();
    }
    else
    {
      await _pricebookTransferService.PerformTransfer(dataTransferType, policies, userCode);
    }
    return Ok();
  }

  // GET api/PricebookTransfer/GetPriceChangeTransfers
  [HttpGet]
  [Route("GetPriceChangeTransfers")]
  public async Task<IActionResult> GetPriceChangeTransfers([FromQuery] string? storeId)
  {
    var priceChangeTransfers = await _pricebookTransferService.GetPriceChangeTransfers(storeId);
    return Ok(priceChangeTransfers);
  }

  // GET api/PricebookTransfer/GetCommodityTransfers
  [HttpGet]
  [Route("GetCommodityTransfers")]
  public async Task<IActionResult> GetCommodityTransfers([FromQuery] string? storeId)
  {
    var data = await _pricebookTransferService.GetCommodityTransfers(storeId);
    return Ok(data);
  }

  [HttpGet]
  [Route("GetHotButtonsTransfers")]
  public async Task<IActionResult> GetHotButtonsTransfers([FromQuery] string? storeId)
  {
    var data = await _pricebookTransferService.GetHotButtonsTransfers(storeId);
    return Ok(data);
  }

  // GET api/PricebookTransfer/GetPromotionTransfers
  [HttpGet]
  [Route("GetPromotionTransfers")]
  public async Task<IActionResult> GetPromotionTransfers([FromQuery] string? storeId)
  {
    var data = await _pricebookTransferService.GetPromotionTransfers(storeId);
    return Ok(data);
  }

  [HttpPost]
  [Route("GetStockTransfersByStoreId")]
  public async Task<IActionResult> GetStockTransfersByStoreId(ProductSearchParameterDto productSearchParameterDto)
  {
    StockTransferFilterDto stockTransferFilterDto = await _stockTransferService.GetStockTransfersByStoreId(productSearchParameterDto);
    return Ok(stockTransferFilterDto);
  }

  // GET api/PricebookTransfer/GetCostChangeTransfers
  [HttpGet]
  [Route("GetCostChangeTransfers")]
  public async Task<IActionResult> GetCostChangeTransfers(string? storeId)
  {
    var data = await _pricebookTransferService.GetCostChangeTransfers(storeId);
    return Ok(data);
  }

  // GET api/PricebookTransfer/GetLabelPrints
  [HttpGet]
  [Route("GetLabelPrints")]
  public async Task<IActionResult> GetLabelPrints()
  {
    IEnumerable<StockItemMinimal>? labelPrints = await _pricebookTransferService.GetLabelPrints();
    return Ok(labelPrints);
  }

  // POST api/PricebookTransfer/DeleteLabelPrint
  [HttpPost]
  [Route("DeleteLabelPrint")]
  public async Task<IActionResult> DeleteLabelPrint([FromBody] string? stockCode)
  {
    await _pricebookTransferService.DeleteLabelPrint(stockCode);
    return Ok();
  }
}
