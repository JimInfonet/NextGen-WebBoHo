namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class CardDefinitionGridController : Controller
{
  private readonly ICardDefinitionService _cardDefinitionService;
  public CardDefinitionGridController(ICardDefinitionService cardDefinitionService)
  {
    _cardDefinitionService = cardDefinitionService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> DeleteCardDefinition(string cardCode)
  {
    string result = await _cardDefinitionService.DeleteCardDefinition(cardCode);
    return Json(result);
  }
}
