namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITaxExemptVendorsRepository
{
  Task<IEnumerable<TaxExemptVendor>> GetAllTaxExemptVendorsAsync(string stockCode);
}
