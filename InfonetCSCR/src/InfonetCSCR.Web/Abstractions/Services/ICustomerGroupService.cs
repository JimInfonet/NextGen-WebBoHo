namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICustomerGroupService
{
  CustomerPoliciesViewModel GetPolicies();
  Task<bool> SaveCustomerGroup(CustomerGroupViewModel customerGroup, bool isUpdate);
  Task<bool> DeleteCustomerGroup(string groupID);
  Task<CustomerGroupViewModel?> GetCustomerGroup(string groupID);
  Task<bool> UpdateCashBonusData(IEnumerable<CashBonusChartViewModel> cashBonusChartViewModels);
  Task DeleteCashBonusData(string groupID);
  Task<IEnumerable<DiscountChartViewModel>> GetDiscountChartData(string? groupID, bool isUseARInvoice);
  Task DeleteDiscountChartData(string groupID);
  Task SaveDiscountChartData(IEnumerable<DiscountChartViewModel> discountChartViewModels);
}
