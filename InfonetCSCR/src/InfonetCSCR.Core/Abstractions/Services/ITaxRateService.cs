namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITaxRateService
{
  Task<IEnumerable<TaxRate>> GetTaxRatesByTaxName(string taxName);
  Task<bool> AddTaxRate(TaxRate taxRate);
  Task<bool> UpdateTaxRate(TaxRate taxRate);
  Task<bool> DeleteTaxRate(string taxName, string taxCode);
  Task<bool> TaxCodeAlreadyExists(TaxRate taxRate);
}
