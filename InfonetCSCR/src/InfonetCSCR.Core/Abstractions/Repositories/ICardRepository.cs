namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICardRepository
{
  Task<bool> CheckCardCodeExistAsync(string cardCode);
  Task<IEnumerable<Card>> GetCardsAsync();
  Task<Card> GetCardDefinitionByCardCodeAsync(string cardCode);
  Task<bool> AddCardAsync(Card card);
  Task<bool> UpdateCardAsync(Card card);
  Task<bool> DeleteCardByCardCodeAsync(string cardCode);
}
