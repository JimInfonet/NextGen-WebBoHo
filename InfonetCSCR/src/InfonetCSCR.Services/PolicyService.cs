namespace InfonetCSCR.Services;

public class PolicyService : IPolicyService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _applicationConfig;

  public PolicyService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
    _applicationConfig = applicationConfig;
  }

  public async Task<IEnumerable<PolicyCompany>> GetPolicyAllCompany()
  {
    IEnumerable<PolicyCompany> policyCompanies = await _unitOfWork.Policy.GetPolicyAllCompany();
    _unitOfWork.Commit();
    return policyCompanies;
  }

  public async Task<IEnumerable<PolicyCanbe>> GetPolicyAllCanbe()
  {
    IEnumerable<PolicyCanbe> policyCanbes = await _unitOfWork.Policy.GetPolicyAllCanbe();
    _unitOfWork.Commit();
    return policyCanbes;
  }

  public async Task<IEnumerable<PolicyClass>> GetPolicyAllClass()
  {
    IEnumerable<PolicyClass> policyClasses = await _unitOfWork.Policy.GetPolicyAllClass();
    _unitOfWork.Commit();
    return policyClasses;
  }

  public async Task<IEnumerable<PolicySettings>> GetPolicyAllSettings()
  {
    IEnumerable<PolicySettings> policySettings = await _unitOfWork.Policy.GetPolicyAllSettings();
    _unitOfWork.Commit();
    return policySettings;
  }

  public async Task<bool> GetPolicyNetworkUpdateByPriceGroup()
  {
    bool result = await _unitOfWork.Policy.GetPolicyNetworkUpdateByPriceGroup();
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> GetPolicyNetworkUpdateByRegion()
  {
    bool result = await _unitOfWork.Policy.GetPolicyNetworkUpdateByRegion();
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> GetPolicyNetworkUpdate()
  {
    bool result = await _unitOfWork.Policy.GetPolicyNetworkUpdate();
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> CheckTaxExempted(string stockCode, string taxName, string exptTaxPolicy)
  {
    bool exemptedTax = false;
    bool isPSetExists = await _unitOfWork.Policy.CheckPSetExistsAsync("EXPT_TAX", taxName); // EXPT_TAX is hard coded in VB Code

    if (!isPSetExists)
    {
      if (taxName == exptTaxPolicy)
      {
        exemptedTax = true;
      }
    }
    else
    {
      isPSetExists = await _unitOfWork.Policy.CheckPSetExistsAsync("EXPT_TAX", taxName, "STOCK", stockCode); // EXPT_TAX and STOCK is hard coded in VB Code
      if (isPSetExists)
      {
        exemptedTax = true;
      }
      else
      {
        StockMaster stockMaster = await _unitOfWork.StockItems.GetStockMasterByStockCodeAsync(stockCode);
        string pValue = $"{stockMaster.Department}ÿ{stockMaster.SubDepartment}ÿ{stockMaster.SubDetail}"; // ÿ is hard coded in VB code

        isPSetExists = await _unitOfWork.Policy.CheckPSetExistsAsync("EXPT_TAX", taxName, "SUBDETAIL", pValue); // EXPT_TAX and SUBDETAIL is hard coded in VB Code

        if (isPSetExists)
        {
          exemptedTax = true;
        }
        else
        {
          pValue = $"{stockMaster.Department}ÿ{stockMaster.SubDepartment}"; // ÿ is hard coded in VB code
          isPSetExists = await _unitOfWork.Policy.CheckPSetExistsAsync("EXPT_TAX", taxName, "SUBDEPT", pValue); // EXPT_TAX and SUBDEPT is hard coded in VB Code

          if (isPSetExists)
          {
            exemptedTax = true;
          }
          else
          {
            pValue = $"{stockMaster.Department}";
            isPSetExists = await _unitOfWork.Policy.CheckPSetExistsAsync("EXPT_TAX", taxName, "DEPT", pValue); // EXPT_TAX and DEPT is hard coded in VB Code

            if (isPSetExists)
            {
              exemptedTax = true;
            }
            else
            {
              if (taxName == exptTaxPolicy)
              {
                exemptedTax = true;
              }
            }
          }
        }
      }
    }

    _unitOfWork.Commit();
    return exemptedTax;
  }

  public async Task<IDictionary<(string stockCode, string taxName), bool>> GetTaxExemptedOrNotDictionary(
    IList<(string stockCode, string taxName)> stockCodeAndTaxNamePairs, 
    string exptTaxPolicy)
  {
    using (var uow = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString))
    {
      IDictionary<(string stockCode, string taxName), bool> pSetExistsForStockCodeAndTaxNameDictionary =
      new Dictionary<(string stockCode, string taxName), bool>();

      HashSet<string> taxNames = new HashSet<string>();
      HashSet<string> stockCodes = new HashSet<string>();

      foreach (var stockCodeAndTaxNamePair in stockCodeAndTaxNamePairs)
      {
        taxNames.Add(stockCodeAndTaxNamePair.taxName);
        stockCodes.Add(stockCodeAndTaxNamePair.stockCode);
      }

      IEnumerable<PSet> pSetsForExptTaxAndTaxNames = await uow.Policy.GetPSetsForExptTaxAndTaxNameAsync(taxNames.ToList());
      IDictionary<string, bool> pSetsForExptTaxAndTaxNamesDictionary = new Dictionary<string, bool>();

      foreach (PSet pSet in pSetsForExptTaxAndTaxNames)
      {
        if (pSetsForExptTaxAndTaxNamesDictionary.TryGetValue(pSet.PSetData, out bool value))
        {
          pSetsForExptTaxAndTaxNamesDictionary[pSet.PSetData] = true;
        }
        else
        {
          pSetsForExptTaxAndTaxNamesDictionary.Add(pSet.PSetData, true);
        }
      }

      IEnumerable<DepartmentSubDepartmentAndDetailDto> departmentSubDepartmentAndDetails =
        await _unitOfWork.StockItems.GetDeptSubDeptAndDetailDataByStockCodesAsync(stockCodes.ToList());
      IDictionary<string, DepartmentSubDepartmentAndDetailDto> deptSubDeptAndDetailDataDictionary =
        new Dictionary<string, DepartmentSubDepartmentAndDetailDto>();

      foreach (DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail in departmentSubDepartmentAndDetails)
      {
        deptSubDeptAndDetailDataDictionary.Add(departmentSubDepartmentAndDetail.StockCode, departmentSubDepartmentAndDetail);
      }

      IDictionary<(string pName, string pSet, string pLevel, string pValue), bool> pSetsForPNamePSetPLevelPValueDictionary =
        new Dictionary<(string pName, string pSet, string pLevel, string pValue), bool>();

      foreach (PSet pSet in pSetsForExptTaxAndTaxNames)
      {
        pSetsForPNamePSetPLevelPValueDictionary.Add((pSet.PName, pSet.PSetData, pSet.PLevel, pSet.PValue), true);
      }

      foreach (var stockCodeAndTaxNamePair in stockCodeAndTaxNamePairs)
      {
        bool exemptedTax = false;
        string stockCode = stockCodeAndTaxNamePair.stockCode;
        string taxName = stockCodeAndTaxNamePair.taxName;

        bool isPSetExists = false;
        if (pSetsForExptTaxAndTaxNamesDictionary.TryGetValue(stockCodeAndTaxNamePair.taxName, out bool value))
        {
          isPSetExists = value;
        }

        if (!isPSetExists)
        {
          if (taxName == exptTaxPolicy)
          {
            exemptedTax = true;
          }
        }
        else
        {
          isPSetExists = false;
          if (pSetsForPNamePSetPLevelPValueDictionary.TryGetValue(("EXPT_TAX", taxName, "STOCK", stockCode), out value))
          {
            isPSetExists = value;
          }

          if (isPSetExists)
          {
            exemptedTax = true;
          }
          else
          {
            isPSetExists = false;
            DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail = new DepartmentSubDepartmentAndDetailDto();
            if (deptSubDeptAndDetailDataDictionary.TryGetValue(stockCodeAndTaxNamePair.stockCode,
              out DepartmentSubDepartmentAndDetailDto tmpDepartmentSubDepartmentAndDetail))
            {
              departmentSubDepartmentAndDetail = tmpDepartmentSubDepartmentAndDetail;
            }

            string pValue =
              $"{departmentSubDepartmentAndDetail.Department}ÿ{departmentSubDepartmentAndDetail.SubDepartment}ÿ{departmentSubDepartmentAndDetail.Detail}"; // ÿ is hard coded in VB code

            if (pSetsForPNamePSetPLevelPValueDictionary.TryGetValue(("EXPT_TAX", taxName, "SUBDETAIL", pValue), out value)) // EXPT_TAX and SUBDETAIL is hard coded in VB Code
            {
              isPSetExists = value;
            }

            if (isPSetExists)
            {
              exemptedTax = true;
            }
            else
            {
              isPSetExists = false;
              pValue = $"{departmentSubDepartmentAndDetail.Department}ÿ{departmentSubDepartmentAndDetail.SubDepartment}"; // ÿ is hard coded in VB code

              if (pSetsForPNamePSetPLevelPValueDictionary.TryGetValue(("EXPT_TAX", taxName, "SUBDEPT", pValue), out value)) // EXPT_TAX and SUBDEPT is hard coded in VB Code
              {
                isPSetExists = value;
              }

              if (isPSetExists)
              {
                exemptedTax = true;
              }
              else
              {
                isPSetExists = false;
                pValue = $"{departmentSubDepartmentAndDetail.Department}";

                if (pSetsForPNamePSetPLevelPValueDictionary.TryGetValue(("EXPT_TAX", taxName, "DEPT", pValue), out value)) // EXPT_TAX and DEPT is hard coded in VB Code
                {
                  isPSetExists = value;
                }

                if (isPSetExists)
                {
                  exemptedTax = true;
                }
                else
                {
                  if (taxName == exptTaxPolicy)
                  {
                    exemptedTax = true;
                  }
                }
              }
            }
          }
        }

        pSetExistsForStockCodeAndTaxNameDictionary.Add(stockCodeAndTaxNamePair, exemptedTax);
      }

      uow.Commit();
      return pSetExistsForStockCodeAndTaxNameDictionary;
    }
  }

  public async Task<bool> CheckIfPolicySettingExistsAsync(PolicyCompany policySetting)
  {
    var items = await _unitOfWork.Policy.CheckIfPolicySettingExistsAsync(policySetting);
    _unitOfWork.Commit();
    return items;
  }

  public async Task<bool> AddPolicySettingAsync(PolicyCompany policySetting)
  {
    var resutl = await _unitOfWork.Policy.AddPolicySettingAsync(policySetting);
    _unitOfWork.Commit();
    return resutl;
  }

  public async Task<bool> UpdatePolicySettingAsync(PolicyCompany policySetting)
  {
    var result = true;

    if (policySetting.Levels.ToUpper() == "COMPANY")
    {
      result &= await _unitOfWork.Policy.UpdatePolicyCompSettingAsync(policySetting);
    }
    else
    {
      result &= await _unitOfWork.Policy.UpdatePolicySetSettingAsync(policySetting);
    }

    if (policySetting.Name == "INITUSERID")
    {
      result &= await _unitOfWork.Policy.UpdateTimesheetPayPeriodAsync(policySetting.Settings);
    }

    if (!_applicationConfig.IsBackOffice && policySetting.Name == "SET_VEND")
    {
      if (policySetting.Settings == "Yes")
      {
        result &= await _unitOfWork.Policy.UpdateSetVendYesAsync();
      }
      else
      {
        result &= await _unitOfWork.Policy.UpdateSetVendNoAsync();
      }
    }

    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> DeletePolicySettingAsync(PolicyCompany policySetting)
  {
    var resutl = await _unitOfWork.Policy.DeletePolicySettingAsync(policySetting);
    _unitOfWork.Commit();
    return resutl;
  }
}
