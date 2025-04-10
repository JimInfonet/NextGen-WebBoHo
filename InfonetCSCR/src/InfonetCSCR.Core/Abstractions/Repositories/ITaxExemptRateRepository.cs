namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITaxExemptRateRepository
{
  Task<IEnumerable<TaxExemptRate>> GetAllTaxExemptRateAsync();
  Task<TaxExemptRate?> GetTaxExemptRateByTaxCodeAsync(int taxCode);
}
