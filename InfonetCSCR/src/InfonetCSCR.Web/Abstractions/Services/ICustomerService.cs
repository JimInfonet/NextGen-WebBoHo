namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICustomerService
{
  CustomerPoliciesViewModel GetPolicies();
  Task<bool> DeleteClient(string code);
  Task<CustomerViewModel?> GetCustomer(string code);
  Task<CustomerViewModel?> GetCustomerByLoyaltyNum(string code);
  Task SaveCustomer(CustomerViewModel customer, bool isUpdate);
  Task<bool> GetSecurityAR();
}
