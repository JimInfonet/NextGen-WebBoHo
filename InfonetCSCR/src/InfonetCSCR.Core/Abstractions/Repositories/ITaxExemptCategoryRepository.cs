namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ITaxExemptCategoryRepository
{
  Task<IEnumerable<TaxExemptCategory>> GetAllTaxExemptCategoryAsync();
  Task<IEnumerable<Reason>> GetTaxExemptReasonsListByTypeAsync(string reasonType);
}
