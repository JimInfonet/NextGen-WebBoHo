namespace InfonetCSCR.Services;

public class BrandService : IBrandService
{
  private readonly IUnitOfWork _unitOfWork;

  public BrandService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<Brand>> GetAllBrands()
  {
    IList<Brand> brands = await _unitOfWork.Brand.GetAllBrandsAsync();
    _unitOfWork.Commit();
    return brands;
  }

  public async Task<bool> AddBrand(Brand brand)
  {
    bool isAdded = await _unitOfWork.Brand.AddBrandAsync(brand);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> UpdateBrand(Brand brand)
  {
    var isUpdated = await _unitOfWork.Brand.UpdateBrandAsync(brand);
    _unitOfWork.Commit();
    return isUpdated;
  }

  public async Task<bool> DeleteBrand(string brandId)
  {
    var isDeleted = await _unitOfWork.Brand.DeleteBrandAsync(brandId);
    _unitOfWork.Commit();
    return isDeleted;
  }
}
