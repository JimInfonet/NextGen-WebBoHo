namespace InfonetCSCR.Core.Abstractions.Services;

public interface IProductTaxExemptService
{
  Task<ProductTaxExemptDto?> GetProductTaxExemptByProductKey(string productKey);
  Task<bool> AddProductTaxExempt(ProductTaxExempt productTaxExempt);
  Task<bool> UpdateProductTaxExempt(ProductTaxExempt productTaxExempt);
  Task<bool> DeleteProductTaxExemptByStockCode(string productKey);
  Task<ProductTaxExempt> GetProductTaxExempt(string stockCode);
  Task<bool> UpdateProductTaxExemptMaxQtyAndReorderQty(ProductTaxExempt taxExemptfromProduct);
  Task<double> CalculateTaxExemptPrice(string stockCode, int taxCode, int taxExemptCategory, int quantityPerPkg, bool isUseFuelPolicy);
  Task<bool> AddProductTaxExemptForDepartmentSubDepartmentAndDetail(TaxExemptDto taxExemptCategory);
  Task<bool> DeleteProductTaxExempt(DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail);
}
