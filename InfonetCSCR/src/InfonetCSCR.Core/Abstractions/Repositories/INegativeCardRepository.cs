namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface INegativeCardRepository
{
  Task<IList<NegativeCard>> GetAllNegativeCardsDataAsync();
  Task<bool> InsertNegativeCardAsync(NegativeCard negativeCard);
  Task<bool> DeleteNegativeCardAsync(NegativeCard negativeCard);
  Task<bool> InsertNegativeCardsBatchAsync(IList<NegativeCard> negativeCards);
  Task<bool> DeleteNegativeCardsBatchAsync(IList<NegativeCard> negativeCards);
  Task<bool> DeleteAllNegativeCardsDataAsync();
}
