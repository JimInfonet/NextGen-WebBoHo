namespace InfonetCSCR.Services;

public class NegativePositiveCardsService : INegativePositiveCardsService
{
  private readonly IUnitOfWork _unitOfWork;

  public NegativePositiveCardsService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<PositiveCard>> GetAllPositiveCardsData()
  {
    IList<PositiveCard> positiveCards = await _unitOfWork.PositiveCards.GetAllPositiveCardsDataAsync();
    _unitOfWork.Commit();

    return positiveCards;
  }

  public async Task<bool> InsertPositiveCard(PositiveCard positiveCard)
  {
    bool isAdded = await _unitOfWork.PositiveCards.InsertPositiveCardAsync(positiveCard);
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> DeletePositiveCard(PositiveCard positiveCard)
  {
    bool isDeleted = await _unitOfWork.PositiveCards.DeletePositiveCardAsync(positiveCard);
    _unitOfWork.Commit();

    return isDeleted;
  }

  public async Task<IList<NegativeCard>> GetAllNegativeCardsData()
  {
    IList<NegativeCard> negativeCards = await _unitOfWork.NegativeCards.GetAllNegativeCardsDataAsync();
    _unitOfWork.Commit();

    return negativeCards;
  }

  public async Task<bool> InsertNegativeCard(NegativeCard negativeCard)
  {
    bool isAdded = await _unitOfWork.NegativeCards.InsertNegativeCardAsync(negativeCard);
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> DeleteNegativeCard(NegativeCard negativeCard)
  {
    bool isDeleted = await _unitOfWork.NegativeCards.DeleteNegativeCardAsync(negativeCard);
    _unitOfWork.Commit();

    return isDeleted;
  }

  public async Task<IList<CardMessage>> GetAllCardMessagesData()
  {
    IList<CardMessage> cardMessages = await _unitOfWork.CardMessages.GetAllCardMessagesDataAsync();
    _unitOfWork.Commit();

    return cardMessages;
  }

  public async Task<bool> InsertCardMessage(CardMessage cardMessage)
  {
    bool isAdded = await _unitOfWork.CardMessages.InsertCardMessageAsync(cardMessage);
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> DeleteCardMessage(CardMessage cardMessage)
  {
    bool isDeleted = await _unitOfWork.CardMessages.DeleteCardMessageAsync(cardMessage);
    _unitOfWork.Commit();

    return isDeleted;
  }

  public async Task<bool> InsertNegativeCardsBatch(IList<NegativeCard> negativeCards)
  {
    bool isAdded = await _unitOfWork.NegativeCards.InsertNegativeCardsBatchAsync(negativeCards);
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> DeleteNegativeCardsBatch(IList<NegativeCard> negativeCards)
  {
    bool isDeleted = await _unitOfWork.NegativeCards.DeleteNegativeCardsBatchAsync(negativeCards);
    _unitOfWork.Commit();

    return isDeleted;
  }

  public async Task<bool> DeleteAllNegativeCards()
  {
    bool isDeleted = await _unitOfWork.NegativeCards.DeleteAllNegativeCardsDataAsync();
    _unitOfWork.Commit();

    return isDeleted;
  }

  public async Task<bool> InsertPositiveCardsBatch(IList<PositiveCard> positiveCards)
  {
    bool isAdded = await _unitOfWork.PositiveCards.InsertPositiveCardsBatchAsync(positiveCards);
    _unitOfWork.Commit();

    return isAdded;
  }

  public async Task<bool> DeletePositiveCardsBatch(IList<PositiveCard> positiveCards)
  {
    bool isDeleted = await _unitOfWork.PositiveCards.DeletePositiveCardsBatchAsync(positiveCards);
    _unitOfWork.Commit();

    return isDeleted;
  }

  public async Task<bool> DeleteAllPositiveCards()
  {
    bool isDeleted = await _unitOfWork.PositiveCards.DeleteAllPositiveCardsAsync();
    _unitOfWork.Commit();

    return isDeleted;
  }
}
