namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStockItemDetailsService
{
  Task<IEnumerable<Assoc>> GetAssocNotInStockCodeAc(StockItemDetailsCommonDto model);
  Task<IEnumerable<ChargesAndDeposits>> GetChargesAndDeposits(string stockCode, string storeID);
  Task<StockItemDetailsDto> GetStockItemDetails(string stockCode, string storeID);
  Task<IEnumerable<Assoc>> DeleteCharge(ChargesDto model);
  Task<IEnumerable<Assoc>> InsertCharge(ChargesDto model);
  Task<IEnumerable<TaxNameAndCode>> GetTaxesAndCode(StockItemDetailsCommonDto model);
  Task<IEnumerable<Tax>> InsertTaxes(Tax model);
  Task<IEnumerable<Tax>> DeleteTaxes(Tax model);
  Task<List<StockAvailabilityByDay>> InsertStockAvailabilityByDay(StockCodeAndAvailabilityByDay stockCodeAndAvailabilityByDay);
}
