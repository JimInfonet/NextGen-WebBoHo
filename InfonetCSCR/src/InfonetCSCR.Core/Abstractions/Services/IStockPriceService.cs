namespace InfonetCSCR.Core.Abstractions.Services;

public interface IStockPriceService
{
  Task<StockPriceFormDto?> GetStockPrice(StockPriceDto model, bool isUseARInvoicePolicy = false);
  Task<StockPriceInfo> GetStockPriceInfo(GetStockPriceInfoDto model);
  Task<IEnumerable<StockPriceInfo>> GetStockPriceInfoBatch(List<GetStockPriceInfoDto> models);
  Task<IEnumerable<StockPriceVendors>?> ToggleVendorStatus(StockPriceModel model);
  Task<IEnumerable<StockPriceVendors>?> UpdateVendorPartNo(VendorPart model);
  Task<IEnumerable<StockCosts>?> UpdateStockCost(StockCosts model);
  Task<IEnumerable<StockCosts>?> InsertFutureStockCosts(StockCostsList model);
  Task<IEnumerable<StockPrice>?> UpdateStockPrice(PriceModel model);
  Task<IEnumerable<StockPrice>?> InsertFuturePrice(PriceModel model);
  Task<IEnumerable<StockRebates>?> AddRebate(StockRebates model);
  Task<IEnumerable<StockRebates>?> DeleteRebate(StockRebates model);
  Task<IEnumerable<StockSpecialPrice>?> AddSpecialPrice(StockSpecialPrice model, bool isUseARInvoice, bool? isSetVendor);
  Task<IEnumerable<StockSpecialPrice>?> UpdateSpecialPriceForUseARInvoicePolicy(StockSpecialPrice model);
  Task<IEnumerable<StockSpecialPrice>?> DeleteSpecialPrice(StockSpecialPrice model, bool isUseARInvoice);
  Task<IEnumerable<StockPriceVendors>?> GetVendorsNotInStockCost(StockPriceModel model);
  Task<IEnumerable<StockPriceVendors>?> AddVendorToStockCost(StockCosts model);
  Task<IEnumerable<StockPriceVendors>?> DeleteVendor(StockPriceModel model);
  Task<IEnumerable<PriceGroup>?> GetPriceGroup();
  Task<VendorPart?> GetVendorPart(string? stockCode, string? vendorId);
}
