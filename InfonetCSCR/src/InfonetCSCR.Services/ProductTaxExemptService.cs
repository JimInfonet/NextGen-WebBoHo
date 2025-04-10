namespace InfonetCSCR.Services;

public class ProductTaxExemptService : IProductTaxExemptService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _applicationConfig;
  private readonly IFuelService _fuelService;

  public ProductTaxExemptService(IApplicationConfig applicationConfig, IFuelService fuelService)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
    this._applicationConfig = applicationConfig;
    this._fuelService = fuelService;
  }

  public ProductTaxExemptDto GetProductTaxExemptDtoFromProductTaxExemptModel(ProductTaxExempt? productTaxExempt)
  {
    ProductTaxExemptDto productTaxExemptDto = new()
    {
      ProductKey = productTaxExempt.ProductKey,
      UpcCode = productTaxExempt.UpcCode,
      CategoryFK = productTaxExempt.CategoryFK,
      TaxFreePrice = productTaxExempt.TaxFreePrice,
      UnitsPerPkg = productTaxExempt.UnitsPerPkg,
      EmplID = productTaxExempt.EmplID,
      LastUpdateTime = productTaxExempt.LastUpdateTime,
      TaxCode = productTaxExempt.TaxCode,
      ProductCode = productTaxExempt.ProductCode,
      InStock = productTaxExempt.InStock,
      Available = productTaxExempt.Available,
      Hold = productTaxExempt.Hold,
      Waste = productTaxExempt.Waste,
      ReorderQuantity = productTaxExempt.ReorderQuantity,
      LastSaleTime = productTaxExempt.LastSaleTime,
      QuantityPerPkg = productTaxExempt.QuantityPerPkg,
      BaseUnitQty = productTaxExempt.BaseUnitQty,
      MaxQty = productTaxExempt.MaxQty,
      TEVendor = productTaxExempt.TEVendor
    };

    return productTaxExemptDto;
  }

  public async Task<ProductTaxExempt> GetProductTaxExempt(string stockCode)
  {
    ProductTaxExempt productTaxExempt = await _unitOfWork.ProductTaxExempts.GetProductTaxExemptAsync(stockCode);
    _unitOfWork.Commit();
    return productTaxExempt;
  }

  public async Task<ProductTaxExemptDto?> GetProductTaxExemptByProductKey(string productKey)
  {
    ProductTaxExempt? productTaxExempt = await _unitOfWork.ProductTaxExempts.GetProductTaxExemptByProductKeyAsync(productKey);
    ProductTaxExemptDto productTaxExemptDto = GetProductTaxExemptDtoFromProductTaxExemptModel(productTaxExempt);
    StockMaster stockMaster = await _unitOfWork.StockItems.GetStockMasterByStockCodeAsync(productKey);
    if (productTaxExemptDto != null)
    {
      productTaxExemptDto.Description = stockMaster.Description;
    }
    _unitOfWork.Commit();
    return productTaxExemptDto;
  }

  public async Task<bool> AddProductTaxExempt(ProductTaxExempt productTaxExempt)
  {
    await _unitOfWork.ProductTaxExempts.AddProductTaxExemptAsync(productTaxExempt);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> UpdateProductTaxExempt(ProductTaxExempt productTaxExempt)
  {
    await _unitOfWork.ProductTaxExempts.UpdateProductTaxExemptAsync(productTaxExempt);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> UpdateProductTaxExemptMaxQtyAndReorderQty(ProductTaxExempt productTaxExempt)
  {
    await _unitOfWork.ProductTaxExempts.UpdateProductTaxExemptMaxQtyAndReorderQtyAsync(productTaxExempt);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> DeleteProductTaxExemptByStockCode(string productKey)
  {
    await _unitOfWork.ProductTaxExempts.DeleteProductTaxExemptByStockCodeAsync(productKey);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<double> CalculateTaxExemptPrice(
    string stockCode,
    int taxCode,
    int taxExemptCategory,
    int quantityPerPkg,
    bool isUseFuelPolicy)
  {
    double registrationPrice;
    double taxExemptPrice;
    bool isFuel = false;

    if (isUseFuelPolicy == true && _applicationConfig.IsBackOffice)
    {
      isFuel = await _fuelService.CheckStockCodeExistsInGrade(stockCode);
    }

    if (isFuel)
    {
      registrationPrice = await _fuelService.GetFuelPriceByStockCode(stockCode);
    }
    else
    {
      StockMaster stockMaster = await _unitOfWork.StockItems.GetStockMasterByStockCodeAsync(stockCode);
      string vendor = stockMaster.Vendor ?? "";
      registrationPrice = await _unitOfWork.StockPrices.GetStockPriceByStockCodeAndVendorAsync(stockCode, vendor);
    }

    TaxExemptRate? taxExemptRate = await _unitOfWork.TaxExemptRates.GetTaxExemptRateByTaxCodeAsync(taxCode);
    double taxRate = 0;
    string rateType = "";

    if (taxExemptRate != null)
    {
      taxRate = taxExemptRate.TaxRate;
      rateType = taxExemptRate.RateType ?? "";
    }

    if (rateType == "$")
    {
      if (taxExemptCategory == 3)
      {
        taxRate = (taxRate * quantityPerPkg).Round(2);
        taxExemptPrice = registrationPrice - taxRate;
      }
      else
      {
        taxExemptPrice = registrationPrice - taxRate;
      }
    }
    else if (rateType == "%")
    {
      if (taxExemptCategory == 3)
      {
        if (quantityPerPkg == 0)
        {
          quantityPerPkg = 1;
        }

        taxRate = (registrationPrice / quantityPerPkg) * (taxRate / 100);
        taxExemptPrice = registrationPrice - (taxRate * quantityPerPkg);
      }
      else
      {
        taxExemptPrice = registrationPrice * (1 - (taxRate / 100));
      }
    }
    else
    {
      taxExemptPrice = registrationPrice;
    }

    _unitOfWork.Commit();
    return taxExemptPrice.Round(4);
  }

  public async Task<bool> DeleteProductTaxExempt(DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail)
  {
    IEnumerable<StockItem> stockItems = await _unitOfWork.StockItems.GetStockItemsByDepartmentSubDepartmentAndDetailAsync(departmentSubDepartmentAndDetail);
    List<string> productKeys = new List<string>();

    foreach (StockItem stockItem in stockItems)
    {
      productKeys.Add(stockItem.StockCode);
    }

    await _unitOfWork.ProductTaxExempts.DeleteProductTaxExemptByStockCodeBatchAsync(productKeys);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> AddProductTaxExemptForDepartmentSubDepartmentAndDetail(TaxExemptDto taxExemptCategory)
  {
    DepartmentSubDepartmentAndDetailDto departmentSubDepartmentAndDetail = new()
    {
      Department = taxExemptCategory.Department,
      SubDepartment = taxExemptCategory.SubDepartment,
      Detail = taxExemptCategory.Detail
    };

    IEnumerable<StockItem> stockItems = await _unitOfWork.StockItems.GetStockItemsByDepartmentSubDepartmentAndDetailAsync(departmentSubDepartmentAndDetail);
    List<ProductTaxExempt> productTaxExempts = new List<ProductTaxExempt>();
    List<string> productKeys = new List<string>();

    foreach (StockItem stockItem in stockItems)
    {
      productTaxExempts.Add(new ProductTaxExempt()
      {
        ProductKey = stockItem.StockCode,
        UpcCode = stockItem.StockCode.ToString()
      });
    }

    foreach (ProductTaxExempt productTaxExempt in productTaxExempts)
    {
      ProductTaxExempt? tmpProductTaxExempt = await _unitOfWork.ProductTaxExempts.GetProductTaxExemptByProductKeyAsync(productTaxExempt.ProductKey);
      if (tmpProductTaxExempt != null)
      {
        productTaxExempt.ProductKey = tmpProductTaxExempt.ProductKey;
        productTaxExempt.UpcCode = tmpProductTaxExempt.UpcCode;
        productTaxExempt.CategoryFK = tmpProductTaxExempt.CategoryFK;
        productTaxExempt.TaxFreePrice = tmpProductTaxExempt.TaxFreePrice;
        productTaxExempt.UnitsPerPkg = tmpProductTaxExempt.UnitsPerPkg;
        productTaxExempt.EmplID = tmpProductTaxExempt.EmplID;
        productTaxExempt.LastUpdateTime = tmpProductTaxExempt.LastUpdateTime;
        productTaxExempt.TaxCode = tmpProductTaxExempt.TaxCode;
        productTaxExempt.ProductCode = tmpProductTaxExempt.ProductCode;
        productTaxExempt.InStock = tmpProductTaxExempt.InStock;
        productTaxExempt.Available = tmpProductTaxExempt.Available;
        productTaxExempt.Hold = tmpProductTaxExempt.Hold;
        productTaxExempt.Waste = tmpProductTaxExempt.Waste;
        productTaxExempt.ReorderQuantity = tmpProductTaxExempt.ReorderQuantity ?? 0;
        productTaxExempt.LastSaleTime = tmpProductTaxExempt.LastSaleTime;
        productTaxExempt.QuantityPerPkg = tmpProductTaxExempt.QuantityPerPkg;
        productTaxExempt.BaseUnitQty = tmpProductTaxExempt.BaseUnitQty;
        productTaxExempt.MaxQty = tmpProductTaxExempt.MaxQty;
        productTaxExempt.TEVendor = tmpProductTaxExempt.TEVendor;
      }

      productTaxExempt.InStock ??= 0;
      productTaxExempt.Available ??= 0;
      productTaxExempt.Hold ??= 0;
      productTaxExempt.Waste ??= 0;
      productTaxExempt.ReorderQuantity = productTaxExempt.ReorderQuantity ?? 0;

      if ((taxExemptCategory.QuantityPerPkg != null && taxExemptCategory.BaseUnitQty != null) || taxExemptCategory.UnitsPerPkg != 0)
      {
        productTaxExempt.UnitsPerPkg = (int)taxExemptCategory.UnitsPerPkg == 0 ? 1 : (int)taxExemptCategory.UnitsPerPkg;
      }

      if (taxExemptCategory.QuantityPerPkg != null)
      {
        productTaxExempt.QuantityPerPkg = (taxExemptCategory.QuantityPerPkg == 0) ? 1 : (int)taxExemptCategory.QuantityPerPkg;
      }

      if (taxExemptCategory.BaseUnitQty != null)
      {
        productTaxExempt.BaseUnitQty = (taxExemptCategory.BaseUnitQty == 0) ? 1 : (int)taxExemptCategory.BaseUnitQty;
      }

      if (taxExemptCategory.ProductCode != "" && taxExemptCategory.ProductCode != null)
      {
        productTaxExempt.ProductCode = taxExemptCategory.ProductCode;
      }
      else if (productTaxExempt.ProductCode == "" || productTaxExempt.ProductCode == null)
      {
        productTaxExempt.ProductCode = null;
      }

      if (taxExemptCategory.TaxType != 0 && taxExemptCategory.TaxType != null)
      {
        productTaxExempt.TaxCode = taxExemptCategory.TaxType;
      }

      if (taxExemptCategory.TaxFreePrice != 0 && taxExemptCategory.TaxFreePrice != null)
      {
        productTaxExempt.TaxFreePrice = taxExemptCategory.TaxFreePrice;
      }

      if (taxExemptCategory.Category != null)
      {
        productTaxExempt.CategoryFK = taxExemptCategory.Category.ConvertToIntOrZero();
      }

      productTaxExempt.EmplID = taxExemptCategory.EmployeeId;
      productTaxExempt.LastUpdateTime = DateTime.Now.ToString();

      productKeys.Add(productTaxExempt.ProductKey);
    }

    await _unitOfWork.ProductTaxExempts.DeleteProductTaxExemptByStockCodeBatchAsync(productKeys);
    await _unitOfWork.ProductTaxExempts.AddProductTaxExemptBatchAsync(productTaxExempts);
    bool result = _unitOfWork.Commit();
    return result;
  }
}
