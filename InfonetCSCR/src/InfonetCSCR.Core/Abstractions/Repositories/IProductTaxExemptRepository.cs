namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IProductTaxExemptRepository
{
  Task<ProductTaxExempt?> GetProductTaxExemptByProductKeyAsync(string productKey);
  Task<bool> AddProductTaxExemptAsync(ProductTaxExempt productTaxExempt);
  Task<bool> UpdateProductTaxExemptAsync(ProductTaxExempt productTaxExempt);
  Task<bool> DeleteProductTaxExemptByStockCodeAsync(string productKey);
  Task<ProductTaxExempt> GetProductTaxExemptAsync(string stockCode);
  Task<bool> UpdateProductTaxExemptMaxQtyAndReorderQtyAsync(ProductTaxExempt taxExemptfromProduct);
  Task<bool> AddProductTaxExemptBatchAsync(IEnumerable<ProductTaxExempt> productTaxExempts);
  Task<bool> DeleteProductTaxExemptByStockCodeBatchAsync(IEnumerable<string> productKeys);
}
