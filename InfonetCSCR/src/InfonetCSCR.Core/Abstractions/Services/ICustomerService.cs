namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICustomerService
{
  Task<IEnumerable<CustomersDto>> GetAllCustomers(bool orderByCode);
  Task<bool> DeleteClientCard(string code);
  Task<bool> DeleteClient(CustomerFormDto customerForm);
  Task<CustomerFormDto> GetCustomerByCode(string code, string? loyaltyNum);
  Task<bool> AddCustomer(CustomerFormDto customerForm);
  Task<bool> UpdateCustomer(CustomerFormDto customerForm);
  Task<IEnumerable<CustomerGroup>> GetAllCustomerGroups();
  Task<bool> GetSecurityAR();
}
