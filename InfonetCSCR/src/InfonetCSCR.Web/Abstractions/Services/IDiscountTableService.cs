namespace InfonetCSCR.Web.Abstractions.Services;

public interface IDiscountTableService
{
  Task<IList<DiscountTableViewModel>?> GetAllDiscountTableData(DiscountTablePolicyRecordViewModel discountTablePolicyRecordViewModel);
  Task<bool> InsertDataOnDiscountTable(DiscountTableViewModel discountTableViewModel);
  Task<bool> UpdateDiscountPercentage(DiscountTableViewModel discountTableViewModel);
  Task<bool> DeleteDiscount(DiscountTableViewModel discountTableViewModel);
}
