namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class NegativePositiveCardsController : ControllerBase
{
  private readonly INegativePositiveCardsService _negativePositiveCardsService;

  public NegativePositiveCardsController(INegativePositiveCardsService negativePositiveCardsService)
  {
    _negativePositiveCardsService = negativePositiveCardsService;
  }

  // GET api/NegativePositiveCards/GetAllPositiveCardsData
  [HttpGet]
  [Route("GetAllPositiveCardsData")]
  public async Task<IActionResult> GetAllPositiveCardsData()
  {
    IList<PositiveCard> positiveCards = await _negativePositiveCardsService.GetAllPositiveCardsData();
    return Ok(positiveCards);
  }

  // POST api/NegativePositiveCards/InsertPositiveCard
  [HttpPost]
  [Route("InsertPositiveCard")]
  public async Task<IActionResult> InsertPositiveCard(PositiveCard positiveCard)
  {
    bool isAdded = await _negativePositiveCardsService.InsertPositiveCard(positiveCard);
    return Ok(isAdded);
  }

  // POST api/NegativePositiveCards/DeletePositiveCard
  [HttpPost]
  [Route("DeletePositiveCard")]
  public async Task<IActionResult> DeletePositiveCard(PositiveCard positiveCard)
  {
    bool isDeleted = await _negativePositiveCardsService.DeletePositiveCard(positiveCard);
    return Ok(isDeleted);
  }

  // GET api/NegativePositiveCards/GetAllNegativeCardsData
  [HttpGet]
  [Route("GetAllNegativeCardsData")]
  public async Task<IActionResult> GetAllNegativeCardsData()
  {
    IList<NegativeCard> negativeCards = await _negativePositiveCardsService.GetAllNegativeCardsData();
    return Ok(negativeCards);
  }

  // POST api/NegativePositiveCards/InsertNegativeCard
  [HttpPost]
  [Route("InsertNegativeCard")]
  public async Task<IActionResult> InsertNegativeCard(NegativeCard negativeCard)
  {
    bool isAdded = await _negativePositiveCardsService.InsertNegativeCard(negativeCard);
    return Ok(isAdded);
  }

  // POST api/NegativePositiveCards/DeleteNegativeCard
  [HttpPost]
  [Route("DeleteNegativeCard")]
  public async Task<IActionResult> DeleteNegativeCard(NegativeCard negativeCard)
  {
    bool isDeleted = await _negativePositiveCardsService.DeleteNegativeCard(negativeCard);
    return Ok(isDeleted);
  }

  // GET api/NegativePositiveCards/GetAllCardMessagesData
  [HttpGet]
  [Route("GetAllCardMessagesData")]
  public async Task<IActionResult> GetAllCardMessagesData()
  {
    IList<CardMessage> cardMessages = await _negativePositiveCardsService.GetAllCardMessagesData();
    return Ok(cardMessages);
  }

  // POST api/NegativePositiveCards/InsertCardMessage
  [HttpPost]
  [Route("InsertCardMessage")]
  public async Task<IActionResult> InsertCardMessage(CardMessage cardMessage)
  {
    bool isAdded = await _negativePositiveCardsService.InsertCardMessage(cardMessage);
    return Ok(isAdded);
  }

  // POST api/NegativePositiveCards/DeleteCardMessage
  [HttpPost]
  [Route("DeleteCardMessage")]
  public async Task<IActionResult> DeleteCardMessage(CardMessage cardMessage)
  {
    bool isDeleted = await _negativePositiveCardsService.DeleteCardMessage(cardMessage);
    return Ok(isDeleted);
  }

  // POST api/NegativePositiveCards/InsertNegativeCardsBatch
  [HttpPost]
  [Route("InsertNegativeCardsBatch")]
  public async Task<IActionResult> InsertNegativeCardsBatch(IList<NegativeCard> negativeCards)
  {
    bool isAdded = await _negativePositiveCardsService.InsertNegativeCardsBatch(negativeCards);
    return Ok(isAdded);
  }

  // POST api/NegativePositiveCards/DeleteNegativeCardsBatch
  [HttpPost]
  [Route("DeleteNegativeCardsBatch")]
  public async Task<IActionResult> DeleteNegativeCardsBatch(IList<NegativeCard> negativeCards)
  {
    bool isDeleted = await _negativePositiveCardsService.DeleteNegativeCardsBatch(negativeCards);
    return Ok(isDeleted);
  }

  // POST api/NegativePositiveCards/DeleteAllNegativeCards
  [HttpPost]
  [Route("DeleteAllNegativeCards")]
  public async Task<IActionResult> DeleteAllNegativeCards()
  {
    bool isDeleted = await _negativePositiveCardsService.DeleteAllNegativeCards();
    return Ok(isDeleted);
  }

  // POST api/NegativePositiveCards/InsertPositiveCardsBatch
  [HttpPost]
  [Route("InsertPositiveCardsBatch")]
  public async Task<IActionResult> InsertPositiveCardsBatch(IList<PositiveCard> positiveCards)
  {
    bool isAdded = await _negativePositiveCardsService.InsertPositiveCardsBatch(positiveCards);
    return Ok(isAdded);
  }

  // POST api/NegativePositiveCards/DeletePositiveCardsBatch
  [HttpPost]
  [Route("DeletePositiveCardsBatch")]
  public async Task<IActionResult> DeletePositiveCardsBatch(IList<PositiveCard> positiveCards)
  {
    bool isDeleted = await _negativePositiveCardsService.DeletePositiveCardsBatch(positiveCards);
    return Ok(isDeleted);
  }

  // POST api/NegativePositiveCards/DeleteAllPositiveCards
  [HttpPost]
  [Route("DeleteAllPositiveCards")]
  public async Task<IActionResult> DeleteAllPositiveCards()
  {
    bool isDeleted = await _negativePositiveCardsService.DeleteAllPositiveCards();
    return Ok(isDeleted);
  }
}
