namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class NegativePositiveCardsController : Controller
{
  private readonly INegativePositiveCardsService _negativePositiveCardsService;

  public NegativePositiveCardsController(INegativePositiveCardsService negativePositiveCardsService)
  {
    _negativePositiveCardsService = negativePositiveCardsService;
  }

  public IActionResult Index()
  {
    return View();
  }

  public async Task<IActionResult> GetAllPositiveCardsData()
  {
    IList<PositiveCardViewModel>? positiveCards = await _negativePositiveCardsService.GetAllPositiveCardsData();
    return Ok(positiveCards);
  }

  public async Task<IActionResult> InsertPositiveCard([FromBody] PositiveCardViewModel positiveCard)
  {
    bool isAdded = await _negativePositiveCardsService.InsertPositiveCard(positiveCard);
    return Ok(isAdded);
  }

  public async Task<IActionResult> DeletePositiveCard([FromBody] PositiveCardViewModel positiveCard)
  {
    bool isDeleted = await _negativePositiveCardsService.DeletePositiveCard(positiveCard);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetAllNegativeCardsData()
  {
    IList<NegativeCardViewModel>? negativeCards = await _negativePositiveCardsService.GetAllNegativeCardsData();
    return Ok(negativeCards);
  }

  public async Task<IActionResult> InsertNegativeCard([FromBody] NegativeCardViewModel negativeCard)
  {
    bool isAdded = await _negativePositiveCardsService.InsertNegativeCard(negativeCard);
    return Ok(isAdded);
  }

  public async Task<IActionResult> DeleteNegativeCard([FromBody] NegativeCardViewModel negativeCard)
  {
    bool isDeleted = await _negativePositiveCardsService.DeleteNegativeCard(negativeCard);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> GetAllCardMessagesData()
  {
    IList<CardMessageViewModel>? cardMessages = await _negativePositiveCardsService.GetAllCardMessagesData();
    return Ok(cardMessages);
  }

  public async Task<IActionResult> InsertCardMessage([FromBody] CardMessageViewModel cardMessage)
  {
    bool isAdded = await _negativePositiveCardsService.InsertCardMessage(cardMessage);
    return Ok(isAdded);
  }

  public async Task<IActionResult> DeleteCardMessage([FromBody] CardMessageViewModel cardMessage)
  {
    bool isDeleted = await _negativePositiveCardsService.DeleteCardMessage(cardMessage);
    return Ok(isDeleted);
  }

  public async Task<IActionResult> ReadNegativeCard()
  {
    ReadNegativeCardLogViewModel? readNegativeCardLog = await _negativePositiveCardsService.ReadNegativeCard();
    return Ok(readNegativeCardLog);
  }

  public async Task<IActionResult> ReadPositiveCard()
  {
    ReadPositiveCardLogViewModel? readPositiveCardLog = await _negativePositiveCardsService.ReadPositiveCard();
    return Ok(readPositiveCardLog);
  }
}
