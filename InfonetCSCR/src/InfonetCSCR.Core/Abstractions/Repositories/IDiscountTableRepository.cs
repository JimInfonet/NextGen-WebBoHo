namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IDiscountTableRepository
{
  Task<IList<DiscountTable>> GetAllDiscountTableDataAsync(DiscountTablePolicyRecord discountTablePolicyRecord);
  Task<bool> InsertDataOnDiscountTableAsync(DiscountTable discountTable);
  Task<bool> UpdateDiscountPercentageAsync(DiscountTable discountTable);
  Task<bool> DeleteDiscountAsync(DiscountTable discountTable);
}
