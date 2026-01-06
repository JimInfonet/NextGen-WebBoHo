namespace InfonetCSCR.Services;

public class CustomerGroupService : ICustomerGroupService
{
  private readonly IUnitOfWork _unitOfWork;

  public CustomerGroupService(IApplicationConfig _applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IEnumerable<CustomerGroup>> GetAllCustomerGroupsByCashBonus(bool cashBonus)
  {
    IEnumerable<CustomerGroup> customerGroups = await _unitOfWork.CustomerGroup.GetAllCustomerGroupsByCashBonusAsync(cashBonus);
    _unitOfWork.Commit();
    return customerGroups;
  }

  public async Task<bool> AddCustomerGroup(CustomerGroup customerGroup)
  {
    bool isCustomerGroupAdded = await _unitOfWork.CustomerGroup.AddCustomerGroupAsync(customerGroup);
    _unitOfWork.Commit();

    return isCustomerGroupAdded;
  }

  public async Task<bool> UpdateCustomerGroup(CustomerGroup customerGroup)
  {
    bool isCustomerGroupAdded = await _unitOfWork.CustomerGroup.UpdateCustomerGroupAsync(customerGroup);
    _unitOfWork.Commit();

    return isCustomerGroupAdded;
  }

  public async Task<bool> DeleteCustomerGroup(string groupID)
  {
    await DeleteExistingCashBonusRecords(groupID);
    await DeleteDiscountChartData(groupID);

    bool isCustomerGroupDeleted = await _unitOfWork.CustomerGroup.DeleteCustomerGroupAsync(groupID);
    _unitOfWork.Commit();

    return isCustomerGroupDeleted;
  }

  public async Task<CustomerGroup> GetCustomerGroup(string groupID)
  {
    var customerGroup = await _unitOfWork.CustomerGroup.GetCustomerGroupAsync(groupID);
    _unitOfWork.Commit();

    return customerGroup;
  }

  public async Task<IEnumerable<CashBonusChart>> GetCashBonusData(string groupID)
  {
    var cashBonusData = await _unitOfWork.CashBonus.GetCashBonusDataAsync(groupID);
    _unitOfWork.Commit();

    return cashBonusData;
  }

  public async Task<bool> UpdateCashBonusData(IEnumerable<CashBonusChart> cashBonusCharts)
  {
    if (cashBonusCharts.Any())
    {
      var updateCashBonusDataTasks = new List<Task<bool>>();
      await _unitOfWork.CashBonus.DeleteExistingCashBonusRecordsAsync(cashBonusCharts.First().GroupID);
      foreach (var cashBonusChart in cashBonusCharts)
      {
        updateCashBonusDataTasks.Add(_unitOfWork.CashBonus.AddCashBonusDataAsync(cashBonusChart));
      }
      bool[] results = await Task.WhenAll(updateCashBonusDataTasks);
      _unitOfWork.Commit();
      var isUpdated = results.Aggregate(true, (current, next) => current && next);

      return isUpdated;
    }

    return false;
  }

  private async Task DeleteExistingCashBonusRecords(string groupID)
  {
    if (!string.IsNullOrEmpty(groupID))
    {
      await _unitOfWork.CashBonus.DeleteExistingCashBonusRecordsAsync(groupID);
    }
  }

  public async Task DeleteCashBonusData(string groupID)
  {
    await DeleteExistingCashBonusRecords(groupID);
    _unitOfWork.Commit();
  }

  public async Task<IEnumerable<DiscountChart>> GetDiscountChartData(string? groupID, bool isUseARInvoice)
  {
    var discountCharts = await _unitOfWork.DiscountChart.GetDiscountChartDataAsync(groupID, isUseARInvoice);
    _unitOfWork.Commit();

    return discountCharts;
  }

  private async Task<bool> DeleteDiscountChartData(string groupID)
  {
    if (!string.IsNullOrEmpty(groupID))
    {
      var isDeleted = await _unitOfWork.DiscountChart.DeleteDiscountChartDataAsync(groupID);
      return isDeleted;
    }
    return false;
  }

  public async Task<bool> DeleteDiscountChart(string groupID)
  {
    var isDeleted = await DeleteDiscountChartData(groupID);
    _unitOfWork.Commit();

    return isDeleted;
  }

  public async Task SaveDiscountChartData(IEnumerable<DiscountChart> discountCharts)
  {
    var saveDiscountChartDataTasks = new List<Task<bool>>();

    foreach (var discountChart in discountCharts)
    {
      saveDiscountChartDataTasks.Add(_unitOfWork.DiscountChart.SaveDiscountChartDataAsync(discountChart));
    }

    await Task.WhenAll(saveDiscountChartDataTasks);
    _unitOfWork.Commit();
  }
}
