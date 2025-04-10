namespace InfonetCSCR.Core.Abstractions.Services;

public interface IBrandService
{
  Task<IList<Brand>> GetAllBrands();
  Task<bool> AddBrand(Brand brand);
  Task<bool> UpdateBrand(Brand brand);
  Task<bool> DeleteBrand(string brandId);
}
