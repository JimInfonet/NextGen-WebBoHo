namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStockItemService
{
  Task<StockCostInfo> GetStockCostInfo(GetStockCostInfoDto model);
  Task<IEnumerable<StockCostInfo>> GetStockCostInfoBatch(List<GetStockCostInfoDto> models, string selectedVendor);
  Task<string> GetActiveVendor(GetActiveVendorDto model);
  Task<StockItemsFilterData> GetStockItemsByFilter(SearchCriteria searchCriteria);
  Task<StockItemMinimalFilterDto> GetStockItemsByDeptSubDeptAndSubDetail(GetStockItemsByDeptSubDeptAndSubDetailDto searchParams);
  Task<StockItemMinimalFilterDto> GetStockItemsByVendor(GetStockItemsByVendorDto searchParams);
  Task<StockMaster> GetStockMasterByStockCode(string stockCode);
  Task<StockMaster> GetStockItemByCode(string stockCode);
  Task<bool> UpdateStockItemTransfer(string stockCode);
  Task<int> GetStockItemsByFilterCount(SearchCriteria criteria);
  Task<bool> RemoveStockItemKit(string stockCode);
  Task<bool> IsVendorActive(string stockCode, string storeId, string vendorId, short orderNo);
  Task<PackingInfo> GetPackingInfo(string stockCode);
  Task<IEnumerable<PackingInfo>> GetPackingInfoBatch(List<string> stockCodes);
  Task<int> GetCountOfProducts(string departmentId, string? subDepartmentId = null, string? detail = null);
  Task<IEnumerable<StockItem>> GetStockItemsByDepartmentSubDepartmentAndDetail(DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail);
  Task<IEnumerable<StockCodesForModalDto>> GetStockMstForPromotion(bool sellInactive);
  Task<bool> AddProductDiscountCode(string productDiscountCode, DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail);
  Task<bool> DeleteProductDiscountCode(DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail);
  Task<Dictionary<string, int>> GetOrderQuantities(
      IList<string> stockCodes,
      Dictionary<string, bool> kitOrderDictionary,
      bool kitsInPOPolicy);
  Task<bool> IsProductsExist(string departmentId, string? subDepartmentId, string? detailId);
  Task<IEnumerable<StockCodesForModalDto>> GetStockItemsByCriteriaAsync(StockItemsCriteriaDto criteria);
  Task<IEnumerable<StockItem>> GetAllStockItemsListAsync();
  Task<IEnumerable<StockCost>> GetStockCodesWithNoNetworkValueAsync();
  Task<IEnumerable<StockCost>> GetStockCodesWithNetworkVendorSettingAsync();
  Task<IEnumerable<StockCost>> GetStockCodesWithNoVendorSettingAsync();
  Task<IEnumerable<StockMaster>> GetStockMasterInfo(string startDepartment, string endDepartment);
  Task<IEnumerable<StockCostInfo>> GetStockCostInfoBatchForBackOffice(StockCostInfoBatchParameterDto stockCostInfoParameterBackOffice);
  Task<IEnumerable<StockPriceInfoDto>> GetStockPriceInfoBatch(IEnumerable<StockCostInfo> stockCostInfo);
  Task<IEnumerable<StockTax>> GetStockTaxInfoBatchForBackOffice(List<string> stockCodes, string storeID);
  Task<IEnumerable<StockCharge>> GetStockChargeInfoBatchForBackOffice(List<string> stockCodes, string storeID);
}
