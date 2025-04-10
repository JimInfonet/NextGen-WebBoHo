namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class CardDefinitionController : ControllerBase
{
  private readonly ICardDefinitionService _cardDefinitionService;

  public CardDefinitionController(ICardDefinitionService cardDefinitionService)
  {
    _cardDefinitionService = cardDefinitionService;
  }

  // GET api/CardDefinition/GetOptionalProfileIDs
  [HttpGet]
  [Route("GetOptionalProfileIDs")]
  public async Task<IActionResult> GetOptionalProfileIDs()
  {
    List<string> optionalProfileIDs = await _cardDefinitionService.GetOptionalProfileIDs();
    return Ok(optionalProfileIDs);
  }

  // GET api/CardDefinition/GetCards
  [HttpGet]
  [Route("GetCards")]
  public async Task<IActionResult> GetCards()
  {
    IEnumerable<Card> cards = await _cardDefinitionService.GetCards();
    return Ok(cards);
  }

  // GET api/CardDefinition/GetCardDefinition/{cardCode}
  [HttpGet]
  [Route("GetCardDefinition/{cardCode}")]
  public async Task<IActionResult> GetCardDefinition(string cardCode)
  {
    CardDefinitionDto cardDefinition = await _cardDefinitionService.GetCardDefinitionByCardCode(cardCode);
    return Ok(cardDefinition);
  }

  // GET api/CardDefinition/GetCardRangesByCardCode/{cardCode}
  [HttpGet]
  [Route("GetCardRangesByCardCode/{cardCode}")]
  public async Task<IActionResult> GetCardRangesByCardCode(string cardCode)
  {
    IEnumerable<CardRange> cardRanges = await _cardDefinitionService.GetCardRangesByCardCode(cardCode);
    return Ok(cardRanges);
  }

  // POST api/CardDefinition/AddCardForUpdateTender
  [HttpPost]
  [Route("AddCardForUpdateTender")]
  public async Task<IActionResult> AddCardForUpdateTender([FromBody] Card card)
  {
    bool result = await _cardDefinitionService.AddCardForUpdateTender(card);
    return Ok(true);
  }

  // POST api/CardDefinition/AddOrUpdateTenderCardForUpdateTender
  [HttpPost]
  [Route("AddOrUpdateTenderCardForUpdateTender")]
  public async Task<IActionResult> AddOrUpdateTenderCardForUpdateTender([FromBody] TenderCard tenderCard)
  {
    bool result = await _cardDefinitionService.AddOrUpdateTenderCardForUpdateTender(tenderCard);
    return Ok(true);
  }

  // POST api/CardDefinition/AddCardDefinition
  [HttpPost]
  [Route("AddCardDefinition")]
  public async Task<IActionResult> AddCardDefinition([FromBody] CardDefinitionDto cardDefinition)
  {
    bool result = await _cardDefinitionService.AddCardDefinitionForUpdateTender(cardDefinition);
    return Ok(true);
  }

  // POST api/CardDefinition/UpdateCardDefinition
  [HttpPost]
  [Route("UpdateCardDefinition")]
  public async Task<IActionResult> UpdateCardDefinition([FromBody] CardDefinitionDto cardDefinition)
  {
    bool result = await _cardDefinitionService.UpdateCardDefinitionByCardCode(cardDefinition);
    return Ok(true);
  }

  // POST api/CardDefinition/DeleteCardDefinitionByCardCode
  [HttpPost]
  [Route("DeleteCardDefinitionByCardCode")]
  public async Task<IActionResult> DeleteCardDefinitionByCardCode([FromBody] string cardCode)
  {
    bool result = await _cardDefinitionService.DeleteCardDefinitionByCardCode(cardCode);
    return Ok(true);
  }

  // POST api/CardDefinition/DeleteCardRangeByCardCodeData
  [HttpPost]
  [Route("DeleteCardRangesByCardCodeData")]
  public async Task<IActionResult> DeleteCardRangesByCardCodeData([FromBody] CardRange cardRange)
  {
    bool result = await _cardDefinitionService.DeleteCardRangesByCardCodeData(cardRange.CardCode.ToString(), cardRange.LowerLimit, cardRange.UpperLimit);
    return Ok(true);
  }

  // POST api/CardDefinition/AddCardRange
  [HttpPost]
  [Route("AddCardRange")]
  public async Task<IActionResult> AddCardRange(CardRange cardRange)
  {
    bool added = await _cardDefinitionService.AddCardRange(cardRange.CardCode, cardRange.LowerLimit, cardRange.UpperLimit);
    return Ok(added);
  }

  // POST api/CardDefinition/DeleteCardDefinition
  [HttpPost]
  [Route("DeleteCardDefinition")]
  public async Task<IActionResult> DeleteCardDefinition([FromBody] string cardCode)
  {
    TenderCard tenderCard = await _cardDefinitionService.GetTendCardForCardCode(cardCode);

    if (tenderCard != null)
    {
      Card card = await _cardDefinitionService.GetCardForCardCode(cardCode);
      return Ok(card.Description);
    }

    bool result = await _cardDefinitionService.DeleteCardDefinitionByCardCode(cardCode);
    if (!result)
    {
      return Ok("Error");
    }
    return Ok("Success");
  }
}
