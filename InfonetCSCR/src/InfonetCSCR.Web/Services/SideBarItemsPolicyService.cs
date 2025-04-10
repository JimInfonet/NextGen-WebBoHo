namespace InfonetCSCR.Web.Services;

public class SideBarItemsPolicyService : ISideBarItemsPolicyService
{
  private readonly IApplicationConfig _config;
  private readonly IPolicyService _policy;
  private readonly IDistributedCache _cache;

  public SideBarItemsPolicyService(IApplicationConfig config, IPolicyService policy, IDistributedCache cache)
  {
    _config = config;
    _policy = policy;
    _cache = cache;
  }

  public async Task<SideBarItemsPoliciesViewModel> GetSideBarItemsPoliciesAsync()
  {
    SideBarItemsPoliciesViewModel sideBarPolicies
      = await _cache.GetCacheAsync<SideBarItemsPoliciesViewModel>(CacheKeys.SideBarItemsPolicies);
    if (sideBarPolicies == null)
    {
      await SetSideBarItemsPoliciesAsync();
      sideBarPolicies = await _cache.GetCacheAsync<SideBarItemsPoliciesViewModel>(CacheKeys.SideBarItemsPolicies);
    }

    return sideBarPolicies;
  }

  private async Task<bool> SetSideBarItemsPoliciesAsync()
  {
    var sideBarItemsPolicies = new SideBarItemsPoliciesViewModel
    {
      ShowStockReceiving = _config.IsBackOffice && !(bool) _policy.GetPolicy(PolicyName.PURCHASE),
      ShowStockTakes = _config.IsBackOffice,
      ShowPurchasingAndReceiving = _config.IsBackOffice && (bool)_policy.GetPolicy(PolicyName.PURCHASE),
      ShowCheckInCheckOut = _config.IsBackOffice && (bool)_policy.GetPolicy(PolicyName.TIMESHEETS),
      ShowPriceBookTransfer = !_config.IsBackOffice,
      ShowPriceBookManagement = _config.IsBackOffice && (bool)_policy.GetPolicy(PolicyName.PRICEBOOK),
      IsCustomer = string.Equals(_policy.GetPolicy(PolicyName.CUST_MEM).ToString(), "Customer", StringComparison.InvariantCultureIgnoreCase),
      CustomerDiscount = (bool)_policy.GetPolicy(PolicyName.CUST_DISC),
      ProductDiscount = (bool)_policy.GetPolicy(PolicyName.PROD_DISC),
      ShowFunctionButtons = _config.IsBackOffice && !(bool)_policy.GetPolicy(PolicyName.USE_FUEL),
      ShowFuelPriceChange = !_config.IsBackOffice && (bool)_policy.GetPolicy(PolicyName.FUELPR_HO),
      UserReports = (bool)_policy.GetPolicy(PolicyName.U_REPORT), 
      ShowSendDataToStore = !_config.IsBackOffice,
    };

    await _cache.SetCacheAsync(CacheKeys.SideBarItemsPolicies, sideBarItemsPolicies);
    return true;
  }
}
