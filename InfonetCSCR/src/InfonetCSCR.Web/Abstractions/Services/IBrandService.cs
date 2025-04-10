namespace InfonetCSCR.Web.Abstractions.Services;

public interface IBrandService
{
  Task<IList<BrandViewModel>?> GetAllBrands();
  Task<bool> AddBrand(BrandViewModel brand);
  Task<bool> UpdateBrand(BrandViewModel brand);
  Task<bool> DeleteBrand(string brandId);
}
