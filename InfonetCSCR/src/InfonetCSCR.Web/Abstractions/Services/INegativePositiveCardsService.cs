namespace InfonetCSCR.Web.Abstractions.Services;

public interface INegativePositiveCardsService
{
  Task<IList<PositiveCardViewModel>?> GetAllPositiveCardsData();
  Task<bool> InsertPositiveCard(PositiveCardViewModel positiveCard);
  Task<bool> DeletePositiveCard(PositiveCardViewModel positiveCard);
  Task<IList<NegativeCardViewModel>?> GetAllNegativeCardsData();
  Task<bool> InsertNegativeCard(NegativeCardViewModel negativeCard);
  Task<bool> DeleteNegativeCard(NegativeCardViewModel negativeCard);
  Task<IList<CardMessageViewModel>?> GetAllCardMessagesData();
  Task<bool> InsertCardMessage(CardMessageViewModel cardMessage);
  Task<bool> DeleteCardMessage(CardMessageViewModel cardMessage);
  Task<bool> InsertNegativeCardsBatch(IList<NegativeCardViewModel> negativeCards);
  Task<bool> DeleteNegativeCardsBatch(IList<NegativeCardViewModel> negativeCards);
  Task<bool> DeleteAllNegativeCards();
  Task<bool> InsertPositiveCardsBatch(IList<PositiveCardViewModel> positiveCards);
  Task<bool> DeletePositiveCardsBatch(IList<PositiveCardViewModel> positiveCards);
  Task<ReadNegativeCardLogViewModel> ReadNegativeCard();
  Task<ReadPositiveCardLogViewModel> ReadPositiveCard();
  Task<bool> DeleteAllPositiveCards();
}
