namespace InfonetCSCR.Web.Services;

public class PolicyService : IPolicyService
{
  private readonly IApplicationPolicy _policy;
  private readonly IHttpContextAccessor _contextAccessor;
  private readonly IStringLocalizer<PolicyService> _localizer;
  private readonly IHttpService _http;

  public PolicyService(IApplicationPolicy policy, IHttpContextAccessor contextAccessor, IStringLocalizer<PolicyService> localizer, IHttpService http)
  {
    _policy = policy;
    _contextAccessor = contextAccessor;
    _localizer = localizer;
    _http = http;
  }

  public dynamic GetPolicy(string name, string paramValue)
  {
    var compPolicy = _policy.PolicyCompany.FirstOrDefault(x => x.Name == name);
    if (compPolicy == null) return string.Empty;

    dynamic policyValue = GetPolicyValue(compPolicy, string.Empty);
    if (paramValue != null)
    {
      var policyCanbes = _policy.PolicyCanbe.Where(x => x.Name == name && x.Canbe != PolicyName.COMPANY);
      foreach (PolicyCanbeViewModel canbe in policyCanbes)
      {
        var policySettings = _policy.PolicySettings.FirstOrDefault(x => x.Name == name && x.Level == canbe.Canbe && x.Value == paramValue);
        if (policySettings != null)
        {
          policyValue = GetPolicyValue(compPolicy, paramValue, false, policySettings, canbe);
          break;
        }
      }
    }

    return policyValue;
  }

  public dynamic GetPolicy(string name, object? obj = null)
  {
    var compPolicy = _policy?.PolicyCompany?.FirstOrDefault(x => x.Name == name);
    if (compPolicy == null) return string.Empty;

    dynamic policyValue = GetPolicyValue(compPolicy, string.Empty);
    var policyCanbes = _policy?.PolicyCanbe?.Where(x => x.Name == name && x.Canbe != PolicyName.COMPANY);
    bool useDepartmentPolicy = _policy?.PolicyCompany?.FirstOrDefault(x => x.Name == PolicyName.USE_DEPT)?.Settings?.ToUpper() == "YES";
    bool useCustomerPolicy = _policy?.PolicyCompany?.FirstOrDefault(x => x.Name == PolicyName.USE_CUST)?.Settings?.ToUpper() == "YES";

    if (policyCanbes != null)
    {
      foreach (PolicyCanbeViewModel canbe in policyCanbes)
      {
        var paramValue = GetParamValue(canbe, obj, useDepartmentPolicy, useCustomerPolicy);
        if (paramValue != null)
        {
          var policySettings = _policy.PolicySettings.FirstOrDefault(x => x.Name == name && x.Level == canbe.Canbe && x.Value == paramValue);
          if (policySettings != null)
          {
            policyValue = GetPolicyValue(compPolicy, paramValue, false, policySettings, canbe);
            break;
          }
        }
      }
    }

    return policyValue;
  }

  public dynamic GetNetworkUpdateSettings(string level, string value)
  {
    dynamic policyValue = false;
    var policySettings = _policy.PolicySettings.FirstOrDefault(x => x.Level == level && x.Value == value);
    if (policySettings != null)
    {
      policyValue = policySettings.Settings?.ToUpper() == "YES";
    }

    return policyValue;
  }

  #region Policy Module

  public async Task<bool> CheckIfPolicySettingExistsAsync(PolicyCompanyViewModel policySetting)
  {
    const string url = "Policy/CheckIfPolicySettingExists";
    return await _http.PostAndParseAsync<bool>(url, policySetting);
  }

  public async Task<bool> AddPolicySettingAsync(PolicyCompanyViewModel policySetting)
  {
    const string url = "Policy/AddPolicySetting";
    return await _http.PostAndParseAsync<bool>(url, policySetting);
  }

  public async Task<bool> UpdatePolicySettingAsync(PolicyCompanyViewModel policySetting)
  {
    const string url = "Policy/UpdatePolicySetting";
    return await _http.PostAndParseAsync<bool>(url, policySetting);
  }

  public async Task<LogViewModel> GetSetVendPolicyLogAsync(PolicyCompanyViewModel policySetting)
  {
    var sb = new StringBuilder();
    var log = new LogViewModel()
    {
      Success = true,
    };

    if (policySetting.Settings == "Yes")
    {
      const string getStockCodesWithNetworkVendorSettingUrl = "StockItems/GetStockCodesWithNetworkVendorSetting";
      var stockCodesWithNetworkVendorSetting = await _http.GetAndParseAsync<IEnumerable<StockCostViewModel>>(getStockCodesWithNetworkVendorSettingUrl);

      if (stockCodesWithNetworkVendorSetting.Any())
      {
        sb.AppendLine("Next stock codes have network vendor settings and they will be deleted from StockCosts table");
        sb.AppendLine("=========================================================================================");

        foreach (var stockCode in stockCodesWithNetworkVendorSetting)
        {
          sb.AppendLine($"{stockCode.StockCode} {stockCode.Description} in store {stockCode.StoreId ?? "ALL"}");
        }
      }

      const string getStockCodesWithNoVendorSettingUrl = "StockItems/GetStockCodesWithNoVendorSetting";
      var stockCodesWithNoVendorSetting = await _http.GetAndParseAsync<IEnumerable<StockCostViewModel>>(getStockCodesWithNoVendorSettingUrl);

      if (stockCodesWithNoVendorSetting.Any())
      {
        sb.AppendLine($"{Environment.NewLine}Next stock codes have no vendor setting");
        sb.AppendLine("=======================================");

        foreach (var stockCode in stockCodesWithNoVendorSetting)
        {
          sb.AppendLine($"{stockCode.StockCode} {stockCode.Description}");
        }

        log.Success = false;
      }
    }
    else
    {
      const string getStockCodesWithNoNetworkValueUrl = "StockItems/GetStockCodesWithNoNetworkValue";
      var stockCodesWithNoNetworkValue = await _http.GetAndParseAsync<IEnumerable<StockCostViewModel>>(getStockCodesWithNoNetworkValueUrl);

      if (stockCodesWithNoNetworkValue.Any())
      {
        sb.AppendLine("Next stock codes have no cost set for network value");
        sb.AppendLine("===================================================");

        foreach (var stockCode in stockCodesWithNoNetworkValue)
        {
          sb.AppendLine($"{stockCode.StockCode} {stockCode.Description}");
        }
      }
    }

    log.LogText = sb.ToString();
    return log;
  }

  public async Task<bool> DeletePolicySettingAsync(PolicyCompanyViewModel policySetting)
  {
    const string url = "Policy/DeletePolicySetting";
    return await _http.PostAndParseAsync<bool>(url, policySetting);
  }

  public async Task<IEnumerable<SubDepartmentViewModel>> GetSubDepartmentsListAsync()
  {

    const string url = "SubDepartment/GetSubDepartmentsList";
    var subDepartmentsList = await _http.GetAndParseAsync<IEnumerable<SubDepartmentViewModel>>(url);

    foreach (var subDepartment in subDepartmentsList)
    {
      subDepartment.CombinedId = $"{subDepartment.DeptId}ÿ{subDepartment.SubDeptId}";
    }

    return subDepartmentsList;
  }

  public async Task<IEnumerable<SubDetailViewModel>> GetSubDetailsListAsync()
  {

    const string url = "SubDetail/GetSubDetailsList";
    var subDetailsList = await _http.GetAndParseAsync<IEnumerable<SubDetailViewModel>>(url);

    foreach (var subDetail in subDetailsList)
    {
      subDetail.CombinedId = $"{subDetail.DeptId}ÿ{subDetail.SubDeptId}ÿ{subDetail.SubDetailId}";
    }

    return subDetailsList;
  }

  public async Task<IEnumerable<PriceGroupViewModel>> GetPriceGroupsListAsync()
  {
    const string url = "Policy/GetPriceGroupsList";
    return await _http.GetAndParseAsync<IEnumerable<PriceGroupViewModel>>(url);
  }

  public async Task<IEnumerable<ClCategoryViewModel>> GetClCategoryListAsync()
  {
    const string url = "Categories/GetCalCategoryList";
    return await _http.GetAndParseAsync<IEnumerable<ClCategoryViewModel>>(url);
  }

  public async Task<IEnumerable<DiscCodeViewModel>> GetDiscCodesListAsync()
  {
    const string url = "Categories/GetDiscCodesList";
    return await _http.GetAndParseAsync<IEnumerable<DiscCodeViewModel>>(url);
  }

  public async Task<IEnumerable<BranchViewModel>> GetBranchesListAsync()
  {
    const string url = "Categories/GetBranchesList";
    return await _http.GetAndParseAsync<IEnumerable<BranchViewModel>>(url);
  }

  public async Task<IEnumerable<UnitOfMeasureViewModel>> GetUomListAsync()
  {
    const string url = "Categories/GetUomList";
    return await _http.GetAndParseAsync<IEnumerable<UnitOfMeasureViewModel>>(url);
  }

  public PolicyChoiceViewModel ParsePolicyChoices(string choices)
  {
    var choicesModel = new PolicyChoiceViewModel();

    if (string.IsNullOrEmpty(choices))
    {
      return choicesModel;
    }

    if (choices.StartsWith("RG"))
    {
      choices = choices.Substring(3, choices.Length - 4);
      var rangeList = choices.Split(',').Select(range => range.Trim()).ToList();
      choicesModel.Type = "Range";
      choicesModel.StartRange = decimal.Parse(rangeList[0]);
      choicesModel.EndRange = decimal.Parse(rangeList[1]);
      choicesModel.Description = $"{_localizer.GetString("valuesBetween")} {rangeList[0]} {_localizer.GetString("and")} {rangeList[1]}";
      return choicesModel;
    }

    if (choices.StartsWith("TM"))
    {
      choices = choices.Substring(3, choices.Length - 4);
      var timeList = choices.Split(',').Select(time => time.Trim()).ToList();
      choicesModel.Type = "Time";
      choicesModel.StartTime = DateTime.ParseExact(timeList[0], "hh:mm:ss tt", CultureInfo.InvariantCulture);
      choicesModel.EndTime = DateTime.ParseExact(timeList[1], "hh:mm:ss tt", CultureInfo.InvariantCulture);
      choicesModel.Description = $"{_localizer.GetString("valuesBetween")} {timeList[0]} {_localizer.GetString("and")} {timeList[1]}";
      return choicesModel;
    }

    if (choices.StartsWith("STAB"))
    {
      choices = choices.Substring(5, choices.Length - 6);
      var tableList = choices.Split(',').Select(table => table.Trim()).ToList();
      choicesModel.Type = "Table";
      choicesModel.Table = tableList[0];
      choicesModel.Field = tableList[1];
      choicesModel.ChoicesList = tableList[0].Split(',').Select(listItem => listItem.Trim()).ToList();
      choicesModel.Description = $"{_localizer.GetString("inTable")} {choicesModel.Table} {_localizer.GetString("field")} {choicesModel.Field}";
      return choicesModel;
    }

    choices = choices.Trim('{', '}');
    choicesModel.Type = "List";

    if (choices.Contains(','))
    {
      choicesModel.ChoicesList = choices.Split(',').Select(listItem => listItem.Trim()).ToList();
      choicesModel.Description = string.Join(Environment.NewLine, choicesModel.ChoicesList);
    }
    else
    {
      choicesModel.Description = choices;
    }


    return choicesModel;
  }

  #endregion

  #region Private Methods

  private dynamic GetPolicyValue(PolicyCompanyViewModel compPolicy, string? paramValue, bool calledByOnlyName = true, PolicySettingsViewModel? policySettings = null, PolicyCanbeViewModel? canbe = null)
  {
    if (paramValue == null)
    {
      calledByOnlyName = true;
    }
    dynamic policyValue;

    switch (compPolicy.VarType)
    {
      case "L":  // "L" represents boolean type
        if (calledByOnlyName)
        {
          policyValue = compPolicy.Settings?.ToUpper() == "YES" && compPolicy.Implemented;
        }
        else if (!calledByOnlyName && canbe?.Canbe == PolicyName.USER && paramValue?.ToUpper() == PolicyName.SYSADMIN)
        {
          policyValue = compPolicy.Implemented;
        }
        else
        {
          policyValue = policySettings?.Settings?.ToUpper() == "YES";
        }
        break;

      case "N":  // "N" represents numeric type
        if (calledByOnlyName)
        {
          policyValue = Convert.ToInt32(compPolicy.Settings);
        }
        else
        {
          policyValue = Convert.ToInt32(policySettings?.Settings);
        }
        break;

      case "C":  // "C" represents string type
        if (calledByOnlyName)
        {
          policyValue = compPolicy.Settings ?? "";
        }
        else
        {
          policyValue = policySettings?.Settings ?? "";
        }
        break;

      case "T":  // "T" represents DateTime type
        if (calledByOnlyName)
        {
          policyValue = Convert.ToDateTime(compPolicy.Settings);
        }
        else
        {
          policyValue = Convert.ToDateTime(policySettings?.Settings);
        }
        break;

      default:
        policyValue = string.Empty;
        break;
    }

    return policyValue;
  }

  private string? GetParamValue(PolicyCanbeViewModel canbe, object? obj, bool useDepartmentPolicy, bool useCustomerPolicy)
  {
    if (canbe == null || canbe?.Canbe == null) return null;
    if (obj == null && (canbe.Canbe != PolicyName.UGROUP && canbe.Canbe != PolicyName.USER)) return null;

    string? paramValue = string.Empty;

    switch (canbe?.Canbe)
    {
      case PolicyName.CLCATEG:
        if (useCustomerPolicy)
        {
          paramValue = obj?.GetType().GetProperty("Category")?.GetValue(obj)?.ToString();
        }
        break;

      case PolicyName.CLIENT:
        if (useCustomerPolicy)
        {
          paramValue = obj?.GetType().GetProperty("Name")?.GetValue(obj)?.ToString();
        }
        break;

      case PolicyName.CREGISTER:
        paramValue = obj?.GetType().GetProperty("RegisterNumber")?.GetValue(obj)?.ToString();
        break;

      case PolicyName.STOCK:
        paramValue = obj?.GetType().GetProperty("StockCode")?.GetValue(obj)?.ToString();
        break;

      case PolicyName.STORE:
        paramValue = obj?.GetType().GetProperty("Code")?.GetValue(obj)?.ToString();
        break;

      case PolicyName.DEPT:
        if (useDepartmentPolicy)
        {
          paramValue = obj?.GetType().GetProperty("Department")?.GetValue(obj)?.ToString();
        }
        break;

      case PolicyName.SUBDEPT:
        if (useDepartmentPolicy)
        {
          var dept = obj?.GetType().GetProperty("Department")?.GetValue(obj)?.ToString();
          var subDept = obj?.GetType().GetProperty("SubDepartment")?.GetValue(obj)?.ToString();
          paramValue = $"{dept}{AppConstants.CATEGORY_SEPARATED_CHARACTER}{subDept}";
        }
        break;

      case PolicyName.SUBDETAIL:
        if (useDepartmentPolicy)
        {
          var dept = obj?.GetType().GetProperty("Department")?.GetValue(obj)?.ToString();
          var subDept = obj?.GetType().GetProperty("SubDepartment")?.GetValue(obj)?.ToString();
          var subDetail = obj?.GetType().GetProperty("SubDetail")?.GetValue(obj)?.ToString();
          paramValue = $"{dept}{AppConstants.CATEGORY_SEPARATED_CHARACTER}{subDept}{AppConstants.CATEGORY_SEPARATED_CHARACTER}{subDetail}";
        }
        break;

      case PolicyName.SUPCAT:
        paramValue = obj?.GetType().GetProperty("Category")?.GetValue(obj)?.ToString();
        break;

      case PolicyName.UGROUP:
        if (obj == null)
        {
          string userJson = _contextAccessor.HttpContext?.Session.GetString("UserDetails")!;
          UserDetailsViewModel user = JsonSerializer.Deserialize<UserDetailsViewModel>(userJson)!;
          paramValue = user.UserGroup?.Code ?? "";
        }
        else
        {
          var userGroup = obj?.GetType().GetProperty("UserGroup")?.GetValue(obj);
          paramValue = userGroup?.GetType().GetProperty("Code")?.GetValue(userGroup)?.ToString();
        }
        break;

      case PolicyName.USER:
        if (obj == null)
        {
          paramValue = _contextAccessor.HttpContext?.Session.GetString("UserCode")!;
        }
        else
        {
          paramValue = obj?.GetType().GetProperty("Code")?.GetValue(obj)?.ToString();
        }
        break;

      default:
        break;
    }

    return paramValue;
  }

  #endregion
}
