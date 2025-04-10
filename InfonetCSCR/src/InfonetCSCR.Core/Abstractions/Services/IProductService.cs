namespace InfonetCSCR.Core.Abstractions.Services;

public interface IProductService
{
  Task<bool> AddProduct(StockMaster stockMaster, StockBranch stockBranch, PLUMast pluMast);
  Task<bool> CopyProduct(StockMaster stockMaster, StockBranch stockBranch, PLUMast pluMast, string baseStockCode);
  Task<bool> UpdateProduct(StockMaster stockMaster, StockBranch stockBranch, ProductTaxExempt taxExemptfromProduct);
  Task<bool> DeleteProduct(string stockCode);
}
