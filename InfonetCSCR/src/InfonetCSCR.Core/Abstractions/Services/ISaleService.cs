namespace InfonetCSCR.Core.Abstractions.Services;

public interface ISaleService
{
  Task<bool> SaleExistsForStockCode(string stockCode);
}
