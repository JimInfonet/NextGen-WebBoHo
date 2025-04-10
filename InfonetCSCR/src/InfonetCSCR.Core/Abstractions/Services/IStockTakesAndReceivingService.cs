namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStockTakesAndReceivingService
{
  Task<bool> AddAllStocksToRecline(StockTakesAndRecievingDto stockTakesAndRecievingDto);
  Task<bool> AddAllStocksToRecLineForStockReceiving(StockTakesAndRecievingDto stockTakesAndRecievingDto);
  Task<bool> AddRecline(ReclineDto reclineDto);
  Task<bool> DeleteAllRecLine(int purpose, bool deleteFromImport = false);
  Task<bool> DeleteAllRecLineAndRecHead(int purpose);
  Task<bool> DeleteRecLine(StockTakesAndReceiveDto receiveDto);
  Task<decimal> GetCost(string stockCode);
  Task<StockTakesAndReceivingResult> GetStockTakesAndReceiveData(StockTakesAndReceiveRequestDto stockTakesAndReceiveRequest);
  Task<int> GetRecLineSusCount(int purpose);
  Task<IEnumerable<StockReceivingExportDto>?> GetStockReceivingExportData();
  Task<IEnumerable<StockTakesAndReceivingReportDto>> GetStockTakesAndReceivingReportData(int purpose);
  Task<IEnumerable<StockDto>?> GetStocks(StockTakesAndRecievingDto stockTakesAndRecievingDto);
  Task<bool> Process(StockProcessParametersDto stockProcessParameters);
  Task<List<string>> ImportFromFile(string text, string exceptedTax, int costDec, int purpose);
  Task<bool> Suspend(int purpose);
  Task<bool> Unsuspend(int purpose);
  Task<bool> GetCountOfInventoryDataForMonth(int month);
  Task<bool> AddAllStocksToRecLineForStockTakesAsync(StockTakesAndRecievingDto stockTakesAndRecievingDto);
  Task<bool> UpdateQuantityForStockTakes(string stockCode, decimal quantity, string costType);
  Task<bool> UpdateTotal(ReclineDto recLine);
  Task<bool> CalculateAllSales(int purpose, bool isUpdateInitial, bool enableUpdate = false);
  Task<int> GetNotPrintedRecLinesCount(int purpose);
  Task<StockTakesExportDto> GetStockTakesExportData();
  Task<bool> UpdateInvoiceNumber(ReclineDto reclineDto);
  Task<bool> UpdatePriceRecords(List<(double cost, string stockCode, string vendorId)> listOfCostStockCodeAndVendorId, bool isPriceChange, bool isCostMarkupPolicy, bool updateNetworkPolicy, bool setVendPolicy, int numPricePolicy);
}
