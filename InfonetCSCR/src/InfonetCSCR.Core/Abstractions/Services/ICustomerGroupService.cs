namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICustomerGroupService
{
  Task<IEnumerable<CustomerGroup>> GetAllCustomerGroupsByCashBonus(bool cashBonus);
  Task<bool> AddCustomerGroup(CustomerGroup customerGroup);
  Task<bool> UpdateCustomerGroup(CustomerGroup customerGroup);
  Task<bool> DeleteCustomerGroup(string groupID);
  Task<CustomerGroup> GetCustomerGroup(string groupID);
  Task<IEnumerable<CashBonusChart>> GetCashBonusData(string groupID);
  Task<bool> UpdateCashBonusData(IEnumerable<CashBonusChart> cashBonusCharts);
  Task DeleteCashBonusData(string groupID);
  Task<IEnumerable<DiscountChart>> GetDiscountChartData(string? groupID, bool isUseARInvoice);
  Task<bool> DeleteDiscountChart(string groupID);
  Task SaveDiscountChartData(IEnumerable<DiscountChart> discountCharts);
}
