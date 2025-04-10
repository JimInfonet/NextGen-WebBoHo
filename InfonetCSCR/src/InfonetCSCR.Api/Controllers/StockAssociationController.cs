namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class StockAssociationController : ControllerBase
{
  private readonly IStockAssociationService _stockAssociationService;

  public StockAssociationController(IStockAssociationService stockAssociationService)
  {
    _stockAssociationService = stockAssociationService;
  }

  // POST api/StockAssociation/InsertStockChargesByStockChargeData
  [HttpPost]
  [Route("InsertStockChargesByStockChargeData")]
  public async Task<IActionResult> InsertStockChargesByStockChargeData(IEnumerable<StockAc> stockCharges)
  {
    bool isAdded = await _stockAssociationService.InsertStockChargesByStockChargeData(stockCharges);
    return Ok(isAdded);
  }

  // POST api/StockAssociation/DeleteStockChargesByStockChargeData
  [HttpPost]
  [Route("DeleteStockChargesByStockChargeData")]
  public async Task<IActionResult> DeleteStockChargesByStockChargeData(IEnumerable<StockAc> stockCharges)
  {
    bool isDeleted = await _stockAssociationService.DeleteStockChargesByStockChargeData(stockCharges);
    return Ok(isDeleted);
  }
}
