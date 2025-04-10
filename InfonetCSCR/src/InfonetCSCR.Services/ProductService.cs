namespace InfonetCSCR.Services;

public class ProductService : IProductService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _applicationConfig;

  public ProductService(IApplicationConfig applicationConfig)
  {
    this._applicationConfig = applicationConfig;
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<bool> AddProduct(StockMaster stockMaster, StockBranch stockBranch, PLUMast pluMast)
  {
    await _unitOfWork.StockItems.AddStockMasterAsync(stockMaster);
    await _unitOfWork.StockBranches.AddStockBranchAsync(stockBranch);
    await _unitOfWork.PLUMasts.AddPLUMastAsync(pluMast);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> CopyProduct(
    StockMaster stockMaster,
    StockBranch stockBranch,
    PLUMast pluMast,
    string baseStockCode)
  {
    string stockCode = stockMaster.StockCode;
    await _unitOfWork.StockItems.AddStockMasterAsync(stockMaster);
    await _unitOfWork.StockBranches.AddStockBranchAsync(stockBranch);
    await _unitOfWork.PLUMasts.AddPLUMastAsync(pluMast);
    await _unitOfWork.StockCosts.AddStockCostByStockCodeAsync(stockCode, baseStockCode);
    await _unitOfWork.StockPrices.AddStockPriceByStockCodeAsync(stockCode, baseStockCode);
    await _unitOfWork.PriceLs.AddPriceLByStockCodeAsync(stockCode, baseStockCode);
    await _unitOfWork.StockRebates.AddStockRebateByStockCodeAsync(stockCode, baseStockCode);
    await _unitOfWork.StockTaxes.AddStockTaxByStockCodeAsync(stockCode, baseStockCode);
    await _unitOfWork.StockAcs.AddStockAcByStockCodeAsync(stockCode, baseStockCode);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> UpdateProduct(StockMaster stockMaster, StockBranch stockBranch, ProductTaxExempt taxExemptfromProduct)
  {
    await _unitOfWork.StockItems.UpdateStockMasterAsync(stockMaster);
    await _unitOfWork.StockBranches.UpdateStockBranchAsync(stockBranch);
    if (_applicationConfig.IsBackOffice)
    {
      await _unitOfWork.ProductTaxExempts.UpdateProductTaxExemptMaxQtyAndReorderQtyAsync(taxExemptfromProduct);
    }
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> DeleteProduct(string stockCode)
  {
    await _unitOfWork.KitMasters.DeleteKitMasterByStockCodeAsync(stockCode);
    await _unitOfWork.KitItems.DeleteKitItemByStockCodeAsync(stockCode);
    await _unitOfWork.KitItems.DeleteKitItemByKitCodeAsync(stockCode);
    await _unitOfWork.StockAcs.DeleteStockAcByStockCodeAsync(stockCode);
    await _unitOfWork.StockBranches.DeleteStockBranchByStockCodeAsync(stockCode);
    await _unitOfWork.StockTaxes.DeleteStockTaxByStockCodeAsync(stockCode);
    await _unitOfWork.StockPrices.DeleteStockPriceByStockCodeAsync(stockCode);
    await _unitOfWork.PriceLs.DeletePriceLByStockCodeAsync(stockCode);
    await _unitOfWork.PLUMasts.DeletePLUMastByPLUPrimAsync(stockCode);
    await _unitOfWork.StockItems.DeleteStockMasterByStockCodeAsync(stockCode);
    await _unitOfWork.StockCosts.DeleteStockCostByStockCodeAsync(stockCode);
    await _unitOfWork.StockRebates.DeleteStockRebateByStockCodeAsync(stockCode);
    await _unitOfWork.VendorParts.DeleteVendorPartByStockCodeAsync(stockCode);

    bool result = _unitOfWork.Commit();
    return result;
  }
}
