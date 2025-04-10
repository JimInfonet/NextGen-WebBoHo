namespace InfonetCSCR.Web.Services;

public class AssociatedChargesService : IAssociatedChargesService
{
  private readonly IHttpService _http;

  public AssociatedChargesService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IEnumerable<AssocViewModel>?> GetAssociations()
  {
    string url = $"AssociatedCharges/GetAssociations";
    var assocCharges = await _http.GetAndParseAsync<IEnumerable<AssocViewModel>?>(url);
    return assocCharges;
  }

  public async Task<bool> AddAssociation(AssocViewModel assocViewModel)
  {
    string url = $"AssociatedCharges/AddAssociation";
    var isAdded = await _http.PostAndParseAsync<bool>(url, assocViewModel);
    return isAdded;
  }

  public async Task<bool> UpdateAssociation(AssocViewModel assocViewModel)
  {
    string url = $"AssociatedCharges/UpdateAssociation";
    var isUpdated = await _http.PostAndParseAsync<bool>(url, assocViewModel);
    return isUpdated;
  }

  public async Task<bool> DeleteAssociation(string code)
  {
    string url = $"AssociatedCharges/DeleteAssociation";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, code);
    return isDeleted;
  }

  public async Task<bool> IsAssociationExists(string code)
  {
    string url = $"AssociatedCharges/IsAssociationExists";
    var isExists = await _http.PostAndParseAsync<bool>(url, code);
    return isExists;
  }

  public async Task<IEnumerable<TaxViewModel>?> GetTaxes(string code)
  {
    string url = $"AssociatedCharges/GetTaxes";
    var taxes = await _http.PostAndParseAsync<IEnumerable<TaxViewModel>?>(url, code);
    return taxes;
  }

  public async Task<IEnumerable<TaxViewModel>?> GetAvailableTaxes(string code)
  {
    string url = $"AssociatedCharges/GetAvailableTaxes";
    var taxes = await _http.PostAndParseAsync<IEnumerable<TaxViewModel>?>(url, code);
    return taxes;
  }

  public async Task<bool> AddTax(AssocTaxViewModel assocTax){
    string url = $"AssociatedCharges/AddTax";
    var isAdded = await _http.PostAndParseAsync<bool>(url, assocTax);
    return isAdded;
  }

  public async Task<bool> DeleteTax(AssocTaxViewModel assocTax){
    string url = $"AssociatedCharges/DeleteTax";
    var isDeleted = await _http.PostAndParseAsync<bool>(url, assocTax);
    return isDeleted;
  }

  public async Task<bool> IsChargeInUse(string code)
  {
    string url = $"AssociatedCharges/IsChargeInUse";
    var isChargeInUse = await _http.PostAndParseAsync<bool>(url, code);
    return isChargeInUse;
  }
}
