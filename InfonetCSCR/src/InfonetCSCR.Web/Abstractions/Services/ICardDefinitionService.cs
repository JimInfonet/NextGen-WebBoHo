namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICardDefinitionService
{
  Task<List<string>> GetOptionalProfileIDs();
  Task<bool?> AddCardForUpdateTender(CardViewModel cardViewModel);
  Task<CardDefinitionViewModel?> GetCardDefinitionByCardCode(string cardCode);
  Task<string> DeleteCardDefinition(string cardCode);
}
