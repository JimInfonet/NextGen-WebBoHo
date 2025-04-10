namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class StockTaxController : ControllerBase
{
  private readonly IStockTaxService _stockTaxService;

  public StockTaxController(
    IStockTaxService stockTaxService)
  {
    _stockTaxService = stockTaxService;
  }

  // POST api/StockTax/InsertStockTaxByStockTaxData
  [HttpPost]
  [Route("InsertStockTaxByStockTaxData")]
  public async Task<IActionResult> InsertStockTaxByStockTaxData(IEnumerable<StockTax> stockTaxes)
  {
    bool isAdded = await _stockTaxService.InsertStockTaxByStockTaxData(stockTaxes);
    return Ok(isAdded);
  }

  // POST api/StockTax/DeleteStockTaxByStockTaxData
  [HttpPost]
  [Route("DeleteStockTaxByStockTaxData")]
  public async Task<IActionResult> DeleteStockTaxByStockTaxData(IEnumerable<StockTax> stockTaxes)
  {
    bool isDeleted = await _stockTaxService.DeleteStockTaxByStockTaxData(stockTaxes);
    return Ok(isDeleted);
  }
}
