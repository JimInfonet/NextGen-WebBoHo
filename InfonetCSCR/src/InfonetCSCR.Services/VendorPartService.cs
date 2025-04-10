namespace InfonetCSCR.Services;

public class VendorPartService : IVendorPartService
{
  private readonly IUnitOfWork _unitOfWork;

  public VendorPartService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<VendorPartDto>> GetVendorPartsFromVendorPartStockMstAndStockBr()
  {
    IEnumerable<VendorPartDto> vendorParts = await _unitOfWork.VendorParts.GetVendorPartsFromVendorPartStockMstAndStockBrAsync();
    _unitOfWork.Commit();
    return vendorParts;
  }

  public async Task<VendorPartDto> GetVendorPartFromVendorPartStockBrByPartNumber(string partNo)
  {
    VendorPartDto vendorPart = await _unitOfWork.VendorParts.GetVendorPartFromVendorPartStockBrByPartNumberAsync(partNo);
    _unitOfWork.Commit();
    return vendorPart;
  }
}
