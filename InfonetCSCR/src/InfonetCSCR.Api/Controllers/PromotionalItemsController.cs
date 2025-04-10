namespace InfonetCSCR.Api.Controllers;

[Route("api/PromotionalItems")]
[ApiController]
[Authorize]

public class PromotionalItemsController : Controller
{
  private readonly IPromotionalItemsService _promotionalItemsService;

  public PromotionalItemsController(
      IPromotionalItemsService promotionalItemsService)
  {
    _promotionalItemsService = promotionalItemsService;
  }

  // GET api/PromotionalItems/GetPromotions
  [HttpGet]
  [Route("GetPromotions")]
  public async Task<IActionResult> GetPromotions()
  {
    IEnumerable<PromoHeader> promotions = await _promotionalItemsService.GetPromoHeaders();
    return Ok(promotions);
  }

  // GET api/PromotionalItems/GetPromotionByPromoId
  [HttpGet]
  [Route("GetPromotionByPromoId")]
  public async Task<IActionResult> GetPromotion(string promoId)
  {
    PromoHeader promotions = await _promotionalItemsService.GetPromoHeader(promoId);
    return Ok(promotions);
  }

  // GET api/PromotionalItems/GetPromotionDetailByPromoId
  [HttpGet]
  [Route("GetPromotionDetailByPromoId")]
  public async Task<IActionResult> GetPromoDetail(string promoId)
  {
    IEnumerable<PromoDetail>? promoDetail = await _promotionalItemsService.GetPromoDetail(promoId);
    return Ok(promoDetail);
  }

  // GET api/PromotionalItems/IsPromoIdAlreadyExists
  [HttpGet]
  [Route("IsPromoIdAlreadyExists")]
  public async Task<IActionResult> IsPromoIdExist(string promoId)
  {
    var isExist = await _promotionalItemsService.IsPromoIdExist(promoId);
    return Ok(isExist);
  }

  // POST api/PromotionalItems/AddPromotionalItemHeader
  [HttpPost]
  [Route("AddPromotionalItemHeader")]
  public async Task<IActionResult> AddPromotionalItemHeader([FromBody] PromoHeader promoHeader)
  {
    var success = await _promotionalItemsService.AddPromotionalItemHeader(promoHeader);
    return Ok(success);
  }

  // POST api/PromotionalItems/UpdatePromotionalItemHeader
  [HttpPost]
  [Route("UpdatePromotionalItemHeader")]
  public async Task<IActionResult> UpdatePromotionalItemHeader([FromBody] PromoHeader promoHeader)
  {
    var success = await _promotionalItemsService.UpdatePromotionalItemHeader(promoHeader);
    return Ok(success);
  }

  // POST api/PromotionalItems/AddPromotionalItemDetails
  [HttpPost]
  [Route("AddPromotionalItemDetails")]
  public async Task<IActionResult> AddPromotionalItemDetails([FromBody] IEnumerable<PromoDetail> promoDetails)
  {
    var success = await _promotionalItemsService.AddPromotionalItemDetials(promoDetails);
    return Ok(success);
  }

  // POST api/PromotionalItems/DeletePromotionalItemDetails
  [HttpPost]
  [Route("DeletePromotionalItemDetails")]
  public async Task<IActionResult> DeletePromotionalItemDetails([FromBody] string promoId)
  {
    var success = await _promotionalItemsService.DeletePromotionalItemDetails(promoId);
    return Ok(success);
  }

  // POST api/PromotionalItems/DeletePromotionalItemHeader
  [HttpPost]
  [Route("DeletePromotionalItemHeader")]
  public async Task<IActionResult> DeletePromotionalItemHeader([FromBody] string promoId)
  {
    var success = await _promotionalItemsService.DeletePromotionalItemHeader(promoId);
    return Ok(success);
  }

  // GET api/PromotionalItems/IsStockCodeValid
  [HttpGet]
  [Route("IsStockCodeValid")]
  public async Task<IActionResult> IsStockCodeValid(bool sellInactive, string stockCode)
  {
    var isExist = await _promotionalItemsService.IsStockCodeValid(sellInactive, stockCode);
    return Ok(isExist);
  }
}
