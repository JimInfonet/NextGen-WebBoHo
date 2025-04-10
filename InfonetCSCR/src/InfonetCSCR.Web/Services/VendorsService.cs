namespace InfonetCSCR.Web.Services;

public class VendorsService : IVendorsService
{
  private readonly IPolicyService _policyService;
  private readonly IHttpService _http;
  private readonly IStringLocalizer<VendorsService> _localizer;

  public VendorsService(IPolicyService policyService, IHttpService http, IStringLocalizer<VendorsService> localizer)
  {
    _policyService = policyService;
    _http = http;
    _localizer = localizer;
  }

  public async Task<IEnumerable<VendorViewModel>> GetVendors()
  {
    string url = "Vendors/all";
    var vendors = await _http.GetAndParseAsync<IEnumerable<VendorViewModel>>(url)?? new List<VendorViewModel>();
    return vendors;
  }

  public async Task<VendorAddRecordViewModel> AddVendorForm()
  {
    string url = "Vendors/GetVendorSortSetup";
    var sortItems = await _http.GetAndParseAsync<IEnumerable<VendorSortSetupViewModel>>(url) ?? new List<VendorSortSetupViewModel>();
    return new VendorAddRecordViewModel()
    {
      Vendor = new VendorViewModel(),
      SortItems = sortItems.ToList(),
      AddedSortItems = "[]",
      RemovedSortItems = "[]"
    };
  }

  public async Task<IEnumerable<VendorSortSetupViewModel>> GetVendorSortSetup()
  {
    string url = "Vendors/GetVendorSortSetup";
    return await _http.GetAndParseAsync<IEnumerable<VendorSortSetupViewModel>>(url) ?? new List<VendorSortSetupViewModel>();
  }

  public async Task<VendorAddRecordViewModel> EditVendorForm(string vendorId)
  {
    var sortItems = await GetVendorSortSetup();

    string url = $"Vendors/GetVendorSort?vendorId={vendorId}";
    var addedSortItems = await _http.GetAndParseAsync<IEnumerable<VendorSortViewModel>>(url) ?? new List<VendorSortViewModel>();

    url = $"Vendors/GetVendorByVendorCode/{vendorId}";
    var vendor = await _http.GetAndParseAsync<VendorViewModel>(url) ?? new VendorViewModel();

    return new VendorAddRecordViewModel()
    {
      Vendor = vendor,
      SortItems = sortItems.ToList(),
      AddedSortItems = JsonSerializer.Serialize(addedSortItems),
      RemovedSortItems = "[]"
    };
  }

  public VendorsPolicyRecordViewModel GetVendorsPolicies()
  {
    return new VendorsPolicyRecordViewModel()
    {
      IsTaxExempted = _policyService.GetPolicy(PolicyName.TAX_EXEMPT),
      IsPurchase = _policyService.GetPolicy(PolicyName.PURCHASE)
    };
  }

  public async Task<bool> SaveVendor(VendorAddRecordViewModel vendorModel, bool isAdd)
  {
    string url = isAdd ? $"Vendors/AddVendor" : $"Vendors/UpdateVendor";
    return await _http.PostAndParseAsync<bool>(url, vendorModel);
  }

  public async Task<string> CheckAndMakeCodeExistsMessage(string code)
  {
    string url = $"Vendors/GetVendorByVendorCode/{code}";
    var vendor = await _http.GetAndParseAsync<VendorViewModel>(url);
    if (vendor != null)
    {
      string msg = _localizer.GetString("codeAlreadyExist");
      return msg.Replace("{}", code);
    }
    return "";
  }

  public async Task<bool> DeleteVendor(string vendorId)
  {
    string url = $"Vendors/DeleteVendor";
    return await _http.PostAndParseAsync<bool>(url, vendorId);
  }

  public async Task<VendorDeleteValidationRecordViewModel> CheckDeleteValidation(string vendorId)
  {
    string url = $"Vendors/CheckDeleteValidation/{vendorId}";
    var result = await _http.GetAndParseAsync<VendorDeletionCaseRecordViewModel>(url);
    string msg;
    if(result.IsConfirmationAlert)
    {
      msg = _localizer.GetString("vendorDeletionConfirmationAlert");
    }
    else
    {
      msg = _localizer.GetString("vendorDeletionInfoAlert");
    }

    var index = msg.IndexOf("{}");
    msg = $"{msg.Substring(0, index)}{vendorId}{msg.Substring(index + 2)}";

    index = msg.IndexOf("{}");
    msg = $"{msg.Substring(0, index)}{result.Count}{msg.Substring(index + 2)}";

    return new VendorDeleteValidationRecordViewModel()
    {
      Message = msg,
      IsConfirmationAlert = result.IsConfirmationAlert
    };
  }

  public async Task<IEnumerable<VendorViewModel>> GetAllVendorName()
  {
    string url = $"Vendors/GetAllVendorsName";
    return await _http.GetAndParseAsync<IEnumerable<VendorViewModel>>(url);
  }

  public async Task<IEnumerable<VendorLinkViewModel>> GetVendorLink(string vendorId)
  {
    string url = $"Vendors/GetVendorLink/{vendorId}";
    return await _http.GetAndParseAsync<IEnumerable<VendorLinkViewModel>>(url);
  }

  public async Task<StoreRegionRecordViewModel> GetAllStoresAndRegions()
  {
    string url = $"Vendors/GetAllStoresAndRegions";
    return await _http.GetAndParseAsync<StoreRegionRecordViewModel>(url) ?? new StoreRegionRecordViewModel();
  }

  public async Task<bool> DeleteVendorLink(VendorLinkViewModel vendorLink)
  {
    string url = $"Vendors/DeleteVendorLink";
    return await _http.PostAndParseAsync<bool>(url, vendorLink);
  }

  public async Task<bool> AddVendorLink(VendorLinkViewModel vendorLink)
  {
    string url = $"Vendors/AddVendorLink";
    return await _http.PostAndParseAsync<bool>(url, vendorLink);
  }
}
