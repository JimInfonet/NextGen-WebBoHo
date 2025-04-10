namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class CardProductRestrictionController : Controller
{
  private readonly ICardProductRestrictionService _cardProductRestrictionService;
  private readonly IPolicyService _policyService;

  public IEnumerable<StockCodesForModalViewModel>? StockCodes { get; set; }

  public CardProductRestrictionController(ICardProductRestrictionService cardProductRestrictionService, IPolicyService policyService)
  {
    _cardProductRestrictionService = cardProductRestrictionService;
    _policyService = policyService;
  }
  public IActionResult Index(string? bankCardId, string? cardDescription, string cardCode, string tendCodeData, string fromGrid, string? profileId)
  {
    ViewData["ProfileId"] = profileId;
    ViewData["BankCardId"] = bankCardId;
    ViewData["CardDescription"] = cardDescription;

    // Card Definition parameters for back button functionality
    ViewData["CardCode"] = cardCode;
    ViewData["TendCodeData"] = tendCodeData;
    ViewData["FromGrid"] = fromGrid;
    return View();
  }

  public async Task<IActionResult> ProductRestrictionForm(ProductRestrictionViewModel? productRestriction, string mode, string cardCode, string tendCodeData, string fromGrid, string? profileId)
  {
    ViewData["ProfileId"] = profileId;
    ViewData["CardCode"] = cardCode;
    ViewData["TendCodeData"] = tendCodeData;
    ViewData["FromGrid"] = fromGrid;
    if (productRestriction == null)
    {
      productRestriction = new ProductRestrictionViewModel();
    }

    if (mode == "Add")
    {
      ViewData["Mode"] = "Add";
    }
    else
    {
      ViewData["Mode"] = "Edit";
    }

    productRestriction.PreAmount = productRestriction.Amount;
    productRestriction.PreBankCardId = productRestriction.BankCardId;
    productRestriction.PreCardDescription = productRestriction.CardDescription;
    productRestriction.PreCardNumber = productRestriction.CardNumber;
    productRestriction.PreCardProductCode = productRestriction.CardProductCode;
    productRestriction.PreCardProductDescription = productRestriction.CardProductDescription;
    productRestriction.PreDescription = productRestriction.Description;
    productRestriction.PreDept = productRestriction.Dept;
    productRestriction.PreSubDept = productRestriction.SubDept;
    productRestriction.PreSubDetail = productRestriction.SubDetail;
    productRestriction.PreProfileId = productRestriction.ProfileId;
    productRestriction.PreStockCode = productRestriction.StockCode;

    return View(productRestriction);
  }

  [HttpGet]
  public async Task<IActionResult> GetProductRestrictions(string? bankCardId, string? profileId)
  {
    return Json(await _cardProductRestrictionService.GetProductRestrictions(bankCardId, profileId));
  }

  [HttpPost]
  public async Task<IActionResult> AddProductRestriction(ProductRestrictionViewModel productRestriction, string cardCode, string tendCodeData, string fromGrid)
  {
    if (productRestriction.Dept == null && productRestriction.StockCode == null)
    {
      ViewData["Mode"] = "Add";
      ViewData["isDepartmentAndStockCodeNull"] = "True";

      ViewData["CardCode"] = cardCode;
      ViewData["TendCodeData"] = tendCodeData;
      ViewData["FromGrid"] = fromGrid;
      return View(nameof(ProductRestrictionForm), productRestriction);
    }

    bool isProductRestrictionExist = await _cardProductRestrictionService.IsProductRestrictionExist(productRestriction);
    if (isProductRestrictionExist)
    {
      ViewData["Mode"] = "Add";
      ViewData["isProductRestrictionExist"] = isProductRestrictionExist.ToString();

      ViewData["CardCode"] = cardCode;
      ViewData["TendCodeData"] = tendCodeData;
      ViewData["FromGrid"] = fromGrid;
      return View(nameof(ProductRestrictionForm), productRestriction);
    }

    bool responseData = await _cardProductRestrictionService.AddProductRestriction(productRestriction);
    if (responseData)
    {
      if (productRestriction.ProfileId != null)
      {
        return RedirectToAction("Index", new { profileId = productRestriction.ProfileId, profileName = productRestriction.CardDescription, ShowToaster = "Add" });
      }

      return RedirectToAction("Index", new { bankCardId = productRestriction.BankCardId, cardDescription = productRestriction.CardDescription, cardCode, tendCodeData, fromGrid, ShowToaster = "Add" });
    }
    else
    {
      ViewData["Mode"] = "Add";
      ViewData["isProductRestrictionAdded"] = responseData.ToString();

      ViewData["CardCode"] = cardCode;
      ViewData["TendCodeData"] = tendCodeData;
      ViewData["FromGrid"] = fromGrid;
      return View(nameof(ProductRestrictionForm), productRestriction);
    }
  }

  [HttpPost]
  public async Task<IActionResult> UpdateProductRestriction(ProductRestrictionViewModel productRestriction, string cardCode, string tendCodeData, string fromGrid)
  {
    if (productRestriction.Dept == null && productRestriction.StockCode == null)
    {
      ViewData["Mode"] = "Edit";
      ViewData["isDepartmentAndStockCodeNull"] = "True";

      ViewData["CardCode"] = cardCode;
      ViewData["TendCodeData"] = tendCodeData;
      ViewData["FromGrid"] = fromGrid;
      return View(nameof(ProductRestrictionForm), productRestriction);
    }

    bool responseData = await _cardProductRestrictionService.UpdateProductRestriction(productRestriction);
    if (responseData)
    {
      if (productRestriction.ProfileId != null)
      {
        return RedirectToAction("Index", new { profileId = productRestriction.ProfileId, profileName = productRestriction.CardDescription, ShowToaster = "Edit" });
      }

      return RedirectToAction("Index", new { bankCardId = productRestriction.BankCardId, cardDescription = productRestriction.CardDescription, cardCode, tendCodeData, fromGrid, ShowToaster = "Edit" });
    }
    else
    {
      ViewData["Mode"] = "Edit";
      ViewData["isProductRestrictionUpdated"] = responseData.ToString();

      ViewData["CardCode"] = cardCode;
      ViewData["TendCodeData"] = tendCodeData;
      ViewData["FromGrid"] = fromGrid;
      return View(nameof(ProductRestrictionForm), productRestriction);
    }
  }

  [HttpPost]
  public async Task<IActionResult> DeleteProductRestriction(ProductRestrictionViewModel productRestriction, string cardCode, string tendCodeData, string fromGrid)
  {
    bool responseData = await _cardProductRestrictionService.DeleteProductRestriction(productRestriction);
    if (!responseData)
    {
      ViewData["isProductRestrictionDeleted"] = responseData.ToString();
    }

    if (productRestriction.ProfileId != null)
    {
      return RedirectToAction("Index", new { profileId = productRestriction.ProfileId, profileName = productRestriction.CardDescription, ShowToaster = "Delete" });
    }

    return RedirectToAction("Index", new { bankCardId = productRestriction.BankCardId, cardDescription = productRestriction.CardDescription, cardCode, tendCodeData, fromGrid, ShowToaster = "Delete" });
  }

  public async Task<bool> IsProductRestrictionExist(ProductRestrictionViewModel productRestriction)
  {
    return await _cardProductRestrictionService.IsProductRestrictionExist(productRestriction);
  }

  public async Task<IActionResult> GetStockCodesForProductRestriction()
  {
    bool sellInactivePolicy = _policyService.GetPolicy(PolicyName.SELL_INACT);
    StockCodes = await _cardProductRestrictionService.GetStockCodesForProductRestriction(sellInactivePolicy);
    return Ok(StockCodes);
  }
}
