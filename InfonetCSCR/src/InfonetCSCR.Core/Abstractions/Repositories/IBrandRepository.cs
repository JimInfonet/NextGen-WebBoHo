namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IBrandRepository
{
  Task<IList<Brand>> GetAllBrandsAsync();
  Task<bool> AddBrandAsync(Brand brand);
  Task<bool> UpdateBrandAsync(Brand brand);
  Task<bool> DeleteBrandAsync(string brandId);
}
