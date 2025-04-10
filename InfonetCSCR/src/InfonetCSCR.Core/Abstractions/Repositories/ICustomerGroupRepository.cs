namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICustomerGroupRepository
{
  Task<IEnumerable<CustomerGroup>> GetAllCustomerGroupsByCashBonusAsync(bool cashBonus);
  Task<bool> AddCustomerGroupAsync(CustomerGroup customerGroup);
  Task<bool> UpdateCustomerGroupAsync(CustomerGroup customerGroup);
  Task<bool> DeleteCustomerGroupAsync(string groupID);
  Task<CustomerGroup> GetCustomerGroupAsync(string groupID);
}
