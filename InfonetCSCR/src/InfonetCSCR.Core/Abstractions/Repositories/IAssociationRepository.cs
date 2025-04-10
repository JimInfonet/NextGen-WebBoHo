namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IAssociationRepository
{
  Task<IEnumerable<Assoc>> GetAssociationsAsync();
  Task<IEnumerable<Assoc>> GetStockAssociationsByStockCodeAndStoreAsync(string stockCode, string storeId);
  Task<double> GetStockAssociationsChargeByPartNoAsync(string partNo);
  Task<IEnumerable<(string stockCode, double charge)>> GetStockCodeAndAssociationsChargeByStockCodesAsync(IList<string> stockCodes);
  Task<bool> AddAssociationAsync(Assoc assoc);
  Task<bool> UpdateAssociationAsync(Assoc assoc);
  Task<bool> DeleteAssociationAsync(string code);
  Task<bool> IsAssociationExistsAsync(string code);
  Task<IEnumerable<Tax>?> GetTaxesAsync(string code);
  Task<IEnumerable<Tax>?> GetAvailableTaxesAsync(string code);
  Task<bool> AddTaxAsync(AssocTax assocTax);
  Task<bool> DeleteTaxAsync(AssocTax assocTax);
  Task<bool> IsChargeInUseAsync(string code);
}
