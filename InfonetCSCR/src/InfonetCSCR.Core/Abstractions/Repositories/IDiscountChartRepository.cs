namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IDiscountChartRepository
{
  Task<IEnumerable<DiscountChart>> GetDiscountChartDataAsync(string? groupID, bool isUseARInvoice);
  Task<bool> DeleteDiscountChartDataAsync(string groupID);
  Task<bool> SaveDiscountChartDataAsync(DiscountChart discountChart);
}
