namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITaxRateRepository
{
  Task<IEnumerable<TaxRate>> GetTaxRatesByTaxNameAsync(string taxName);
  Task<bool> AddTaxRateAsync(TaxRate taxRate);
  Task<bool> UpdateTaxRateAsync(TaxRate taxRate);
  Task<bool> TaxCodeAlreadyExistsAsync(TaxRate taxRate);
}
