namespace InfonetCSCR.Web.Services;

public class TaxService : ITaxService
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policyService;

  public TaxService(IHttpService http, IPolicyService policyService)
  {
    _http = http;
    _policyService = policyService;
  }

  public TaxPoliciesViewModel GetTaxPolicies()
  {
    var policies = new TaxPoliciesViewModel
    {
      TaxRebate = _policyService.GetPolicy(PolicyName.TAX_REBATE)
    };

    return policies;
  }

  public async Task<IEnumerable<TaxMasterViewModel>?> GetTaxes()
  {
    string url = $"TaxMaster/GetTaxes";
    var taxes = await _http.GetAndParseAsync<IEnumerable<TaxMasterViewModel>>(url);
    return taxes;
  }

  public async Task<bool> AddTax(TaxMasterViewModel tax)
  {
    string url = $"TaxMaster/AddTax";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, tax);
    return isAdded;
  }

  public async Task<bool> UpdateTax(TaxMasterViewModel tax)
  {
    string url = $"TaxMaster/UpdateTax";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, tax);
    return isUpdated;
  }

  public async Task<bool> DeleteTax(string taxName)
  {
    string url = $"TaxMaster/DeleteTax";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url, taxName);
    return isDeleted;
  }

  public async Task<bool> TaxNameAlreadyExists(string taxName)
  {
    string url = $"TaxMaster/TaxNameAlreadyExists";
    bool alreadyExists = await _http.PostAndParseAsync<bool>(url, taxName);
    return alreadyExists;
  }

  public async Task<int> GetTaxUsageCount(string taxName, string? taxCode = null)
  {
    string url = $"TaxMaster/GetTaxUsageCount?taxCode={taxCode}";
    var usageCount = await _http.PostAndParseAsync<int>(url, taxName);
    return usageCount;
  }

  public async Task<IEnumerable<TaxRateViewModel>?> GetTaxRatesByTaxName(string taxName)
  {
    string url = $"TaxRate/GetTaxRatesByTaxName";
    var taxRates = await _http.PostAndParseAsync<IEnumerable<TaxRateViewModel>>(url, taxName);
    return taxRates;
  }

  public async Task<bool> TaxCodeAlreadyExists(string taxName, string taxCode)
  {
    TaxRateViewModel taxRate = new()
    {
      Name = taxName,
      Code = taxCode
    };

    string url = $"TaxRate/TaxCodeAlreadyExists";
    bool alreadyExists = await _http.PostAndParseAsync<bool>(url, taxRate);
    return alreadyExists;
  }

  public async Task<bool> AddTaxRate(TaxRateViewModel taxRate)
  {
    string url = $"TaxRate/AddTaxRate";
    bool isAdded = await _http.PostAndParseAsync<bool>(url, taxRate);
    return isAdded;
  }

  public async Task<bool> UpdateTaxRate(TaxRateViewModel taxRate)
  {
    string url = $"TaxRate/UpdateTaxRate";
    bool isUpdated = await _http.PostAndParseAsync<bool>(url, taxRate);
    return isUpdated;
  }

  public async Task<bool> DeleteTaxRate(string taxName, string taxCode)
  {
    string url = $"TaxRate/DeleteTaxRate?taxName={taxName}&taxCode={taxCode}";
    bool isDeleted = await _http.PostAndParseAsync<bool>(url);
    return isDeleted;
  }

  public async Task<IEnumerable<ReasonViewModel>> GetTaxExemptReasonsListByTypeAsync(string reasonType)
  {

    string url = $"TaxExempt/GetTaxExemptReasonsListByType?reasonType={reasonType}";
    return await _http.GetAndParseAsync<IEnumerable<ReasonViewModel>>(url);
  }

  public async Task<IEnumerable<TaxMasterViewModel>> GetTaxesOrderByTaxNameAsync()
  {
    const string url = "TaxMaster/GetTaxesOrderByTaxName";
    return await _http.GetAndParseAsync<IEnumerable<TaxMasterViewModel>>(url);
  }
}
