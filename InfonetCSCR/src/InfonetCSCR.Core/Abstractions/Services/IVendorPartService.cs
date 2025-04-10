namespace InfonetCSCR.Core.Abstractions.Services;

public interface IVendorPartService
{
  Task<IEnumerable<VendorPartDto>> GetVendorPartsFromVendorPartStockMstAndStockBr();
  Task<VendorPartDto> GetVendorPartFromVendorPartStockBrByPartNumber(string partNo);
}
