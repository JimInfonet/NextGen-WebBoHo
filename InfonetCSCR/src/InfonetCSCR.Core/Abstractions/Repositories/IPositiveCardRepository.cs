namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPositiveCardRepository
{
  Task<IList<PositiveCard>> GetAllPositiveCardsDataAsync();
  Task<bool> InsertPositiveCardAsync(PositiveCard positiveCard);
  Task<bool> DeletePositiveCardAsync(PositiveCard positiveCard);
  Task<bool> InsertPositiveCardsBatchAsync(IList<PositiveCard> positiveCards);
  Task<bool> DeletePositiveCardsBatchAsync(IList<PositiveCard> positiveCards);
  Task<bool> DeleteAllPositiveCardsAsync();
}
