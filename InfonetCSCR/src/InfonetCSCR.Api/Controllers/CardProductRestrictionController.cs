namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CardProductRestrictionController : ControllerBase
{
  private readonly ICardProductRestrictionService _productRestrictionService;

  public CardProductRestrictionController(ICardProductRestrictionService categoryService)
  {
    _productRestrictionService = categoryService;
  }

  // GET api/CardProductRestriction/GetProductRestrictions
  [HttpGet]
  [Route("GetProductRestrictions")]
  public async Task<IActionResult> GetProductRestrictions(string? bankCardId, string? profileId)
  {
    IEnumerable<ProductRestriction> productRestrictions = await _productRestrictionService.GetProductRestrictions(bankCardId, profileId);
    return Ok(productRestrictions);
  }

  // GET api/CardProductRestriction/GetStockCodesForProductRestrictions
  [HttpGet]
  [Route("GetStockCodesForProductRestrictions")]
  public async Task<IActionResult> GetStockCodesForProductRestrictions(bool sellInactivePolicy, bool? isOrderByStockCode = false)
  {
    IEnumerable<StockCodesForModalDto> stockCodes = await _productRestrictionService.GetStockCodesForProductRestrictions(sellInactivePolicy, isOrderByStockCode);
    return Ok(stockCodes);
  }

  // POST api/CardProductRestriction/AddProductRestriction
  [HttpPost]
  [Route("AddProductRestriction")]
  public async Task<IActionResult> AddProductRestriction(ProductRestriction productRestriction)
  {
    bool isAdded = await _productRestrictionService.AddProductRestriction(productRestriction);
    return Ok(isAdded);
  }

  // POST api/CardProductRestriction/UpdateProductRestriction
  [HttpPost]
  [Route("UpdateProductRestriction")]
  public async Task<IActionResult> UpdateProductRestriction(ProductRestriction productRestriction)
  {
    bool isUpdated = await _productRestrictionService.UpdateProductRestriction(productRestriction);
    return Ok(isUpdated);
  }

  // POST api/CardProductRestriction/DeleteProductRestriction
  [HttpPost]
  [Route("DeleteProductRestriction")]
  public async Task<IActionResult> DeleteProductRestriction([FromBody] ProductRestriction productRestriction)
  {
    bool isDeleted = await _productRestrictionService.DeleteProductRestriction(productRestriction);
    return Ok(isDeleted);
  }

  // POST api/CardProductRestriction/IsProductRestrictionExist
  [HttpPost]
  [Route("IsProductRestrictionExist")]
  public async Task<IActionResult> IsProductRestrictionExist(ProductRestriction productRestriction)
  {
    bool isExist = await _productRestrictionService.IsProductRestrictionExist(productRestriction);
    return Ok(isExist);
  }
}
