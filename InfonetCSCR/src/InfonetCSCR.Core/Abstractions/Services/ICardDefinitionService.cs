namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICardDefinitionService
{
  Task<IEnumerable<Card>> GetCards();
  Task<Card> GetCardForCardCode(string cardCode);
  Task<TenderCard> GetTendCardForCardCode(string cardCode);
  Task<bool> AddCardForUpdateTender(Card card);
  Task<bool> AddOrUpdateTenderCardForUpdateTender(TenderCard tenderCard);
  Task<CardDefinitionDto> GetCardDefinitionByCardCode(string cardCode);
  Task<List<string>> GetOptionalProfileIDs();
  Task<bool> AddCardDefinitionForUpdateTender(CardDefinitionDto cardDefinition);
  Task<bool> UpdateCardDefinitionByCardCode(CardDefinitionDto cardDefinition);
  Task<bool> DeleteCardDefinitionByCardCode(string cardCode);
  Task<bool> DeleteCardRangesByCardCodeData(string cardCode, string lowerLimit, string upperLimit);
  Task<IEnumerable<CardRange>> GetCardRangesByCardCode(string cardCode);
  Task<bool> AddCardRange(int cardCode, string lowerLimit, string upperLimit);
}
