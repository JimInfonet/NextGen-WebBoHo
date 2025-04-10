namespace InfonetCSCR.Core.Abstractions.Services;

public interface IAssociationService
{
  Task<IEnumerable<Assoc>> GetAssociations();
  Task<IEnumerable<Assoc>> GetStockAssociationsByStockCodeAndStore(string stockCode, string storeId);
  Task<bool> AddAssociation(Assoc assoc);
  Task<bool> UpdateAssociation(Assoc assoc);
  Task<bool> DeleteAssociation(string code);
  Task<bool> IsAssociationExists(string code);
  Task<IEnumerable<Tax>?> GetTaxes(string code);
  Task<IEnumerable<Tax>?> GetAvailableTaxes(string code);
  Task<bool> AddTax(AssocTax assocTax);
  Task<bool> DeleteTax(AssocTax assocTax);
  Task<bool> IsChargeInUse(string code);
}
