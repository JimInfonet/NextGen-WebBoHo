namespace InfonetCSCR.Core.Abstractions.Services;

public interface INegativePositiveCardsService
{
  Task<IList<PositiveCard>> GetAllPositiveCardsData();
  Task<bool> InsertPositiveCard(PositiveCard positiveCard);
  Task<bool> DeletePositiveCard(PositiveCard positiveCard);
  Task<IList<NegativeCard>> GetAllNegativeCardsData();
  Task<bool> InsertNegativeCard(NegativeCard negativeCard);
  Task<bool> DeleteNegativeCard(NegativeCard negativeCard);
  Task<IList<CardMessage>> GetAllCardMessagesData();
  Task<bool> InsertCardMessage(CardMessage cardMessage);
  Task<bool> DeleteCardMessage(CardMessage cardMessage);
  Task<bool> InsertNegativeCardsBatch(IList<NegativeCard> negativeCards);
  Task<bool> DeleteNegativeCardsBatch(IList<NegativeCard> negativeCards);
  Task<bool> DeleteAllNegativeCards();
  Task<bool> InsertPositiveCardsBatch(IList<PositiveCard> positiveCards);
  Task<bool> DeletePositiveCardsBatch(IList<PositiveCard> positiveCards);
  Task<bool> DeleteAllPositiveCards();
}
