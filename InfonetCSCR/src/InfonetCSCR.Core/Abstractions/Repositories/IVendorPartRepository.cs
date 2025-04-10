namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IVendorPartRepository
{
  Task<bool> DeleteVendorPartByStockCodeAsync(string stockCode);
  Task<IEnumerable<VendorPart>> GetVendorPartsFromVendorPartsAndStockMstAsync(string sortByColumn, string vendorId);
  Task<IEnumerable<ProductAndVendorPartDataOfKitItemDto>> GetProductAndVendorPartDataOfKitItemsAsync(
      IList<(string VendorId, string StockCode)> vendorIDStockCodePairList);
  Task<IEnumerable<VendorPart>> GetVendorPartsByPartNosAsync(IList<string> partNos);
  Task<IEnumerable<VendorPartDto>> GetVendorPartsFromVendorPartStockMstAndStockBrAsync();
  Task<VendorPartDto> GetVendorPartFromVendorPartStockBrByPartNumberAsync(string partNo);
}
