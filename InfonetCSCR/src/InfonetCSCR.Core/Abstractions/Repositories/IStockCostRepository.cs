namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IStockCostRepository
{
  Task<bool> AddStockCostAsync(StockCost model);
  Task<StockCostInfo> GetStockCostInfoAsync(GetStockCostInfoDto model);
  Task<IEnumerable<StockCostInfo>> GetStockCostInfoBatchAsync(List<GetStockCostInfoDto> models);
  Task<StockCostInfo?> GetStockCostInfoSingleAsync(GetStockCostInfoDto model);
  Task<int> GetStockCostRecordCountAsync(GetStockCostRecordCountDto model);
  Task<bool> AddStockCostByStockCodeAsync(string stockCode, string baseStockCode);
  Task<bool> DeleteStockCostByStockCodeAsync(string stockCode);
  Task<IEnumerable<StockCostInfo>> GetStockCostInfoBatchForBackOfficeAsync(StockCostInfoBatchParameterBackOfficeDto stockCostInfoParameterBackOffice);
  Task<IEnumerable<StockCostInfo>> GetStockCostInfoBatchForHeadOfficeAsync(StockCostInfoBatchParameterDto stockCostInfoParameter);
  Task<IEnumerable<StockPriceInfoDto>> GetStockPriceInfoBatchForBackOfficeAsync(IEnumerable<StockCostInfo> stockCostInfo);
  Task<IEnumerable<StockCharge>> GetStockChargeInfoBatchForBackOfficeAsync(List<string> stockCodes, string storeID);
  Task<IEnumerable<StockTax>> GetStockTaxInfoBatchForBackOfficeAsync(List<string> stockCodes, string storeID);
  Task<IEnumerable<StockCostInfo>> GetStockCostInfoByMultipleVendorBatchForBackOfficeAsync(GetStockCostInfoByMultipleVendorBatchBackOfficeDto model);
  Task<IEnumerable<StockCostInfo>> GetStockCostInfoByMultipleVendorBatchPBMAsync(GetStockCostInfoByMultipleVendorBatchBackOfficeDto model, string selectedVendor);
  Task<StockCostInfo?> GetStockCostInfoForBackOfficeAsync(StockCostInfoParameterBODto stockCostInfoParameterBackOffice);
  Task<string> GetVendorIDByStockCodeAsync(string stockCode, string activeVendor, string orderNo);
  Task<bool> UpdateCostAsync(double stockCost, string stockCode, string orderNo, string vendorID);
  Task<bool> UpdateOrInsertStockCostAsync(double stockCost, string stockCode, string orderNo, string vendorID);
  Task<IEnumerable<StockCost>> GetStockCodesWithNoNetworkValueAsync();
  Task<IEnumerable<StockCost>> GetStockCodesWithNetworkVendorSettingAsync();
  Task<IEnumerable<StockCost>> GetStockCodesWithNoVendorSettingAsync();

}
