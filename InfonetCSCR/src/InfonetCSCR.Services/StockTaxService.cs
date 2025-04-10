namespace InfonetCSCR.Services;

public class StockTaxService : IStockTaxService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IPolicyService _policy;
  private readonly IApplicationConfig _config;

  public StockTaxService(IApplicationConfig applicationConfig, IPolicyService policy)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
    _policy = policy;
    _config = applicationConfig;
  }

  public async Task<bool> InsertStockTaxByStockTaxData(IEnumerable<StockTax> stockTaxes)
  {
    await _unitOfWork.StockTaxes.DeleteStockTaxByStockTaxDataAsync(stockTaxes);
    await _unitOfWork.StockTaxes.InsertStockTaxByStockTaxDataAsync(stockTaxes);

    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> DeleteStockTaxByStockTaxData(IEnumerable<StockTax> stockTaxes)
  {
    await _unitOfWork.StockTaxes.DeleteStockTaxByStockTaxDataAsync(stockTaxes);
    bool result = _unitOfWork.Commit();
    return result;
  }

  public async Task<double> CalculateTax(double? amount, string? stockCode, string? exptTaxPolicy, int? costDecPolicy)
  {
    double itemsGstRate = await GetItemsGstRate(stockCode, exptTaxPolicy);
    double itemsHstRate = await GetItemsHstRate(stockCode, exptTaxPolicy, costDecPolicy);

    double itemsTaxRate = itemsGstRate + itemsHstRate;

    double? calculatedTax = amount - amount / (1 + itemsTaxRate);

    return (double)calculatedTax;
  }

  public async Task<double> CalculateGST(string exptTaxPolicy, double? extCost, string? stockCode)
  {
    double calculatedGST = 0;
    IEnumerable<StockTax> stockTaxes = await _unitOfWork.StockTaxes.GetStockTaxesByStockCodeAsync(stockCode);

    foreach (StockTax stockTax in stockTaxes)
    {
      bool isTaxExempted = await _policy.CheckTaxExempted(stockCode, stockTax.TaxName, exptTaxPolicy);
      if (isTaxExempted)
      {
        IEnumerable<double> taxRates =
          await _unitOfWork.TaxMasters.GetProductTaxRateByTaxCodeAndTaxNameAsync(stockTax.TaxCode, stockTax.TaxName);

        foreach (double taxRate in taxRates)
        {
          double tmpTaxRate = taxRate;
          calculatedGST = (double)(calculatedGST + (extCost * tmpTaxRate / 100));
          calculatedGST = calculatedGST.Round(2);
        }
      }
    }

    _unitOfWork.Commit();
    return calculatedGST;
  }

  public async Task<IDictionary<string, IList<double>>> GetTaxRatesByStockCodes(
    string exptTaxPolicy,
    IEnumerable<string> stockCodes,
    bool? taxIncl = null,
    DateTime? purchaseDate = null)
  {
    using (var uow = new UnitOfWork(_config, _config.CSCMasterConnectionString))
    {
      IDictionary<string, IList<double>> getTaxRatesByStockCodesDictionary = new Dictionary<string, IList<double>>();
      double calculatedGST = 0;
      IEnumerable<StockTax> stockTaxes = await uow.StockTaxes.GetStockTaxesByStockCodesAsync(stockCodes);
      IDictionary<string, IList<StockTax>> stockTaxesDictionary = new Dictionary<string, IList<StockTax>>();
      IList<(string stockCode, string taxName)> stockCodeAndTaxNamePairs = new List<(string stockCode, string taxName)>();
      HashSet<(string taxCode, string taxName)> taxCodeAndTaxNamePairs = new HashSet<(string taxCode, string taxName)>();

      bool useGSTFromAdmin = false;
      Admin adminGSTTax = await uow.Admins.GetAdminByName("GSTTAX"); // GSTTAX is hard coded in VB
      string[] taxIdDescription = adminGSTTax.Value.Split('-');
      string taxName = taxIdDescription[0];
      string taxCode = taxIdDescription[1];

      if (purchaseDate != null)
      {
        DateTime currentDate = DateTime.Now;
        DateTime datePOValue = (DateTime)purchaseDate;

        string currentDateFormatted = currentDate.ToString("yyyyMMdd");
        string datePOValueFormatted = datePOValue.ToString("yyyyMMdd");

        Admin adminUseGSTTax = await uow.Admins.GetAdminByName("UseGSTTax"); // UseGSTTax is hard coded in VB
        bool useGSTTax = (adminUseGSTTax.Value == "YES");

        //20100701 is Hard coded in VB code
        useGSTFromAdmin =
          (currentDateFormatted.CompareTo("20100701") >= 0 && datePOValueFormatted.CompareTo("20100701") < 0 && useGSTTax);
      }

      foreach (StockTax stockTax in stockTaxes)
      {
        if (stockTaxesDictionary.TryGetValue(stockTax.StockCode, out IList<StockTax>? value))
        {
          stockTaxesDictionary[stockTax.StockCode].Add(stockTax);
        }
        else
        {
          IList<StockTax> tmpStockTaxes = new List<StockTax>();
          tmpStockTaxes.Add(stockTax);
          stockTaxesDictionary.Add(stockTax.StockCode, tmpStockTaxes);
        }

        stockCodeAndTaxNamePairs.Add((stockTax.StockCode, stockTax.TaxName));

        if (useGSTFromAdmin)
        {
          taxCodeAndTaxNamePairs.Add((taxCode, taxName));
        }
        else
        {
          taxCodeAndTaxNamePairs.Add((stockTax.TaxCode, stockTax.TaxName));
        }
      }

      IDictionary<(string stockCode, string taxName), bool> taxExemptedOrNoByStockCodeAndTaxNametDictionary
        = await _policy.GetTaxExemptedOrNotDictionary(stockCodeAndTaxNamePairs, exptTaxPolicy);

      IEnumerable<TaxMasterDto> taxMasterDtos = await uow.TaxMasters.GetProductTaxeRateDataByTaxCodesAndTaxNamesAsync(
        taxCodeAndTaxNamePairs.ToList(),
        taxIncl
        );

      IDictionary<(string taxCode, string taxName), IList<double>> productTaxeRateByTaxCodeAndTaxNameDictionary
        = new Dictionary<(string taxCode, string taxName), IList<double>>();

      foreach (TaxMasterDto taxMaster in taxMasterDtos)
      {
        if (productTaxeRateByTaxCodeAndTaxNameDictionary.TryGetValue((taxMaster.Code, taxMaster.Name), out IList<double>? tmpRateList))
        {
          productTaxeRateByTaxCodeAndTaxNameDictionary[(taxMaster.Code, taxMaster.Name)].Add((double)taxMaster.Rate);
        }
        else
        {
          IList<double> tmpRates = new List<double>();
          tmpRates.Add((double)taxMaster.Rate);
          productTaxeRateByTaxCodeAndTaxNameDictionary.Add((taxMaster.Code, taxMaster.Name), tmpRates);
        }
      }

      foreach (string stockCode in stockCodes)
      {
        IEnumerable<StockTax> stockTaxesForStockCode = new List<StockTax>();
        if (stockTaxesDictionary.TryGetValue(stockCode,
            out IList<StockTax>? tmpStockTax))
        {
          stockTaxesForStockCode = tmpStockTax;
        }

        foreach (StockTax stockTax in stockTaxesForStockCode)
        {
          bool isTaxExempted = false;

          if (taxExemptedOrNoByStockCodeAndTaxNametDictionary.TryGetValue((stockCode, stockTax.TaxName),
            out bool value))
          {
            isTaxExempted = value;
          }

          if (!isTaxExempted)
          {
            IEnumerable<double> taxRates = new List<double>();

            string taxCodeParameter = stockTax.TaxCode;
            string taxNameParameter = stockTax.TaxName;

            if (useGSTFromAdmin)
            {
              taxCodeParameter = taxCode;
              taxNameParameter = taxName;
            }

            if (productTaxeRateByTaxCodeAndTaxNameDictionary.TryGetValue((taxCodeParameter, taxNameParameter),
              out IList<double> tmpRates))
            {
              taxRates = tmpRates;
            }

            foreach (double taxRate in taxRates)
            {
              if (getTaxRatesByStockCodesDictionary.TryGetValue(stockCode,
                out tmpRates))
              {
                getTaxRatesByStockCodesDictionary[stockCode].Add(taxRate);
              }
              else
              {
                tmpRates = new List<double>();
                tmpRates.Add(taxRate);
                getTaxRatesByStockCodesDictionary.Add(stockCode, tmpRates);
              }
            }
          }
        }
      }

      uow.Commit();
      return getTaxRatesByStockCodesDictionary;
    }
  }

  public async Task<double> GetItemsGstRate(string stockCode, string? exptTaxPolicy)
  {
    double itemsGstRate = 0;
    IEnumerable<StockTax> stockTaxes = await _unitOfWork.StockTaxes.GetStockTaxesByStockCodeAsync(stockCode);

    foreach (StockTax stockTax in stockTaxes)
    {
      bool isTaxExempted = await _policy.CheckTaxExempted(stockCode, stockTax.TaxName, exptTaxPolicy);
      if (isTaxExempted)
      {
        IEnumerable<double> taxRates =
          await _unitOfWork.TaxMasters.GetProductTaxRateByTaxCodeAndTaxNameAsync(stockTax.TaxCode, stockTax.TaxName, false);

        foreach (double taxRate in taxRates)
        {
          double tmpTaxRate = taxRate;
          itemsGstRate = (double)(itemsGstRate + (tmpTaxRate / 100));
        }
      }
    }

    _unitOfWork.Commit();

    itemsGstRate = itemsGstRate.Round(2);
    return itemsGstRate;
  }

  public async Task<double> GetItemsHstRate(string stockCode, string? exptTaxPolicy, int? costDecPolicy)
  {
    double itemsHstRate = 0;
    IEnumerable<StockTax> stockTaxes = await _unitOfWork.StockTaxes.GetStockTaxesByStockCodeAsync(stockCode);

    foreach (StockTax stockTax in stockTaxes)
    {
      bool isTaxExempted = await _policy.CheckTaxExempted(stockCode, stockTax.TaxName, exptTaxPolicy);
      if (isTaxExempted)
      {
        IEnumerable<double> taxRates =
          await _unitOfWork.TaxMasters.GetProductTaxRateByTaxCodeAndTaxNameAsync(stockTax.TaxCode, stockTax.TaxName, true);

        foreach (double taxRate in taxRates)
        {
          double tmpTaxRate = taxRate;
          itemsHstRate = (double)(itemsHstRate + (tmpTaxRate / 100));
        }
      }
    }

    _unitOfWork.Commit();

    itemsHstRate = itemsHstRate.Round((int)costDecPolicy);
    return itemsHstRate;
  }
}
