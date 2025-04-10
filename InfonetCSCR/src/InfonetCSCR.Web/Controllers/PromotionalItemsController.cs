namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class PromotionalItemsController : Controller
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policyService;
  private readonly IPromotionalItemsService _promotionalItemsService;

  public PromotionalItemsController(
        IHttpService http,
        IPolicyService policyService,
        IPromotionalItemsService promotionalItemsService)
  {
    _http = http;
    _policyService = policyService;
    _promotionalItemsService = promotionalItemsService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> GetPromotions()
  {
    string url = "PromotionalItems/GetPromotions";
    var promoHeaders = await _http.GetAndParseAsync<List<PromoHeaderViewModel>>(url);
    return Ok(promoHeaders);
  }

  public async Task<IActionResult> GetPromotionByPromoId(string promoId)
  {
    string url = $"PromotionalItems/GetPromotionByPromoId?promoId={promoId}";
    var promoHeader = await _http.GetAndParseAsync<PromoHeaderViewModel>(url);
    return Ok(promoHeader);
  }

  public async Task<IActionResult> GetPromotionDetailsByPromoId(string promoId)
  {
    string url = $"PromotionalItems/GetPromotionDetailByPromoId?promoId={promoId}";
    var promoDetails = await _http.GetAndParseAsync<List<PromoDetailViewModel>>(url);
    return Ok(promoDetails);
  }

  public async Task<IActionResult> IsPromoIdExist(string promoId)
  {
    bool promoExist = await _promotionalItemsService.IsPromoIdExist(promoId);
    return Ok(promoExist);
  }

  public async Task<IActionResult> GetStockCodeMstForPromotion()
  {
    bool policySellInactive = _policyService.GetPolicy(PolicyName.SELL_INACT);
    string url = $"StockItems/GetStockMstForPromotion?sellInactive={policySellInactive}";
    var stockCodes = await _http.GetAndParseAsync<List<StockCodesForModalViewModel>>(url);
    return Ok(stockCodes);
  }

  [HttpPost]
  public async Task<ActionResult<GenericApiResponse>> SavePromotionalItems([FromBody] PromoHeaderViewModel promoHeader)
  {
    await _promotionalItemsService.SavePromotionalItems(promoHeader);
    return NoContent();
  }

  [HttpPost]
  public async Task<IActionResult> DeletePromo([FromBody] string promoId)
  {
    string promoHeaderUrl = @$"PromotionalItems/DeletePromotionalItemHeader";
    await _http.PostAndParseAsync<bool>(promoHeaderUrl, promoId);

    string promoDetailUrl = @$"PromotionalItems/DeletePromotionalItemDetails";
    await _http.PostAndParseAsync<bool>(promoDetailUrl, promoId);

    return NoContent();
  }

  public async Task<IActionResult> IsStockCodeValid(string stockCode)
  {
    bool policySellInactive = _policyService.GetPolicy(PolicyName.SELL_INACT);
    var url = @$"PromotionalItems/IsStockCodeValid?sellInactive={policySellInactive}&stockCode={stockCode}";
    var stockCodeDescription = await _http.GetAndParseAsync<StockCodesForModalViewModel>(url);
    return Ok(stockCodeDescription);
  }
}
