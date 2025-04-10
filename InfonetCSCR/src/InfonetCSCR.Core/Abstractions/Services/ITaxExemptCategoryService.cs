namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITaxExemptCategoryService
{
  Task<IEnumerable<TaxExemptCategory>> GetAllTaxExemptCategory();
  Task<IEnumerable<Reason>> GetTaxExemptReasonsListByTypeAsync(string reasonType);
}
