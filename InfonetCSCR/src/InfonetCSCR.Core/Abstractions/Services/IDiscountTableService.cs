namespace InfonetCSCR.Core.Abstractions.Services;

public interface IDiscountTableService
{
  Task<IList<DiscountTable>> GetAllDiscountTableData(DiscountTablePolicyRecord discountTablePolicyRecord);
  Task<bool> InsertDataOnDiscountTable(DiscountTable discountTable);
  Task<bool> UpdateDiscountPercentage(DiscountTable discountTable);
  Task<bool> DeleteDiscount(DiscountTable discountTable);
}
