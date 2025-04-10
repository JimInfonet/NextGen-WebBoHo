namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICustomerRepository
{
  Task<IEnumerable<CustomersDto>> GetAllCustomersAsync(bool orderByCode);
  Task<bool> DeleteClientCardByCodeAsync(string code);
  Task<bool> DeleteClientAsync(string code);
  Task<Customer> GetCustomerAsync(string code);
  Task<Customer> GetCustomerByLoyaltyNumAsync(string loyaltyNum);
  Task<bool> AddCustomerAsync(Customer customer);
  Task<bool> UpdateCustomerAsync(Customer customer);
  Task<IEnumerable<CustomerGroup>> GetAllCustomerGroupsAsync();
  Task<bool> AddClientChangeAsync(ClientChanges clientChange);
  Task<bool> DeleteClientChangeAsync(ClientChanges clientChange);
  Task<bool> AddArCustomerAsync(ArCustomer arCustomer);
  Task<ArCustomer> GetArCustomerAsync(string code);
  Task<bool> UpdateArCustomerAsync(ArCustomer arCustomer);
  Task<bool> DeleteArCustomerAsync(string code);
  Task<string> GetSecurityARAsync();
}
