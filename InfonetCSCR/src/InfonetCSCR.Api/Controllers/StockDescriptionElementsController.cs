namespace InfonetCSCR.Api.Controllers;

[Route("api/StockDescriptionElements")]
[ApiController]
[Authorize]

public class StockDescriptionElementsController : ControllerBase
{
  private readonly IStockDescriptionElementsService _stockItemFormDataService;
  private readonly IProductTaxExemptService _productTaxExemptService;

  public StockDescriptionElementsController(
            IStockDescriptionElementsService stockItemFormDataService,
            IProductTaxExemptService productTaxExemptService)
  {
    _stockItemFormDataService = stockItemFormDataService;
    _productTaxExemptService = productTaxExemptService;
  }

  // GET api/StockDescriptionElements/Warehouse
  [HttpGet]
  [Route("warehouse")]
  public async Task<IActionResult> GetWarehouse()
  {
    IEnumerable<Warehouse> warehouse = await _stockItemFormDataService.GetWarehouse();
    return Ok(warehouse);
  }

  // GET api/StockDescriptionElements/Grade
  [HttpGet]
  [Route("grade")]
  public async Task<IActionResult> GetGrade(string stockCode)
  {
    IEnumerable<Grade> grade = await _stockItemFormDataService.GetGrade(stockCode);
    return Ok(grade);
  }

  // GET api/StockDescriptionElements/Grade
  [HttpGet]
  [Route("ProductTaxExempt")]
  public async Task<IActionResult> GetProductTaxExempt(string stockCode)
  {
    ProductTaxExempt productTaxExempt = await _productTaxExemptService.GetProductTaxExempt(stockCode);
    return Ok(productTaxExempt);
  }
}
