namespace InfonetCSCR.Core.Abstractions.Services;

public interface ITaxExemptRateService
{
  Task<IEnumerable<TaxExemptRate>> GetAllTaxExemptRate();
}
