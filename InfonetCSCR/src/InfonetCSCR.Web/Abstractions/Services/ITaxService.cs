namespace InfonetCSCR.Web.Abstractions.Services;

public interface ITaxService
{
  TaxPoliciesViewModel GetTaxPolicies();
  Task<IEnumerable<TaxMasterViewModel>?> GetTaxes();
  Task<bool> AddTax(TaxMasterViewModel tax);
  Task<bool> UpdateTax(TaxMasterViewModel tax);
  Task<bool> DeleteTax(string taxName);
  Task<bool> TaxNameAlreadyExists(string taxName);
  Task<int> GetTaxUsageCount(string taxName, string? taxCode = null);
  Task<IEnumerable<TaxRateViewModel>?> GetTaxRatesByTaxName(string taxName);
  Task<bool> AddTaxRate(TaxRateViewModel taxRate);
  Task<bool> UpdateTaxRate(TaxRateViewModel taxRate);
  Task<bool> DeleteTaxRate(string taxName, string taxCode);
  Task<bool> TaxCodeAlreadyExists(string taxName, string taxCode);
  Task<IEnumerable<ReasonViewModel>> GetTaxExemptReasonsListByTypeAsync(string reasonType);
  Task<IEnumerable<TaxMasterViewModel>> GetTaxesOrderByTaxNameAsync();
}
