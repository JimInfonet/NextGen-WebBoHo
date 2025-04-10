namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITaxExemptVendorsService
{
  Task<IEnumerable<TaxExemptVendor>> GetAllTaxExemptVendors(string stockCode);
}
