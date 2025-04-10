namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStockAssociationService
{
  Task<bool> InsertStockChargesByStockChargeData(IEnumerable<StockAc> stockCharges);
  Task<bool> DeleteStockChargesByStockChargeData(IEnumerable<StockAc> stockCharges);
}
