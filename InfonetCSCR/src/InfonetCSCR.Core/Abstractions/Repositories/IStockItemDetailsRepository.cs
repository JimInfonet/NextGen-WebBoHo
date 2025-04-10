namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IStockItemDetailsRepository
{
  Task<int> GetTotalActiveCharges();
  Task<int> GetTotalActiveTaxes();
  Task<IEnumerable<Assoc>> GetAssocNotInStockCodeAc(string? stockCode, string storeIDs);
  Task<IEnumerable<ChargesAndDeposits>> GetChargesAndDeposits(string? stockCode, string? storeID);
  Task<int> DeleteCharge(string? stockCode, string? chargeCode, string? storeIDs);
  Task<IEnumerable<StockAc>> GetCharges(string? stockCode, string? chargeCode, string? storeIDs);
  Task<int> InsertCharges(string? stockCode, string? chargeCode, string? selectionValue, List<string>? storeIDs);
  Task<IEnumerable<Tax>> GetTaxes(string? stockCode, string? storeID);
  Task<IEnumerable<TaxNameAndCode>> GetTaxesAndCode(string? stockCode, string storeIDs);
  Task<int> DeleteTaxes(string? stockCode, string? taxName, string? taxCode, string? storeIDs, bool deleteForInsert = true);
  Task<int> InsertTaxes(string? stockCode, string? taxName, string? taxCode, List<string> storeIDs);
  Task<IEnumerable<AvailabilityByDay>> GetAvailabilityByDay(string stockCode);
  Task<int> DeleteStockAvailabilityByDay(string stockCode);
  Task<int> InsertStockAvailabilityByDay(List<AvailabilityByDay> days);
}
