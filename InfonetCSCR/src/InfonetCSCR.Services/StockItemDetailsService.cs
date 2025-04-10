namespace InfonetCSCR.Services;

public class StockItemDetailsService : IStockItemDetailsService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;
  public StockItemDetailsService(IApplicationConfig config)
  {
    this._unitOfWork = new UnitOfWork(config, config.CSCMasterConnectionString);
    _config = config;
  }

  public async Task<StockItemDetailsDto> GetStockItemDetails(string? stockCode, string? storeID)
  {
    int activeCharges = await _unitOfWork.StockItemDetails.GetTotalActiveCharges();
    int activeTaxes = await _unitOfWork.StockItemDetails.GetTotalActiveTaxes();
    var item = await _unitOfWork.StockItems.GetStockItemByCode(stockCode);
    var charges = await _unitOfWork.StockItemDetails.GetChargesAndDeposits(stockCode, storeID);
    var taxes = await _unitOfWork.StockItemDetails.GetTaxes(stockCode, storeID);
    var stockAvailability = await GetStockAvailabilityByDay(stockCode);
    _unitOfWork.Commit();

    var data = new StockItemDetailsDto
    {
      TotalActiveCharges = activeCharges,
      TotalActiveTaxes = activeTaxes,
      Charges = charges,
      Taxes = taxes,
      AvailabilityByDays = stockAvailability,
      StockItemName = item.Description
    };
    return data;
  }

  private async Task<List<StockAvailabilityByDay>> GetStockAvailabilityByDay(string stockCode)
  {
    var stockAvailability = await _unitOfWork.StockItemDetails.GetAvailabilityByDay(stockCode);

    var dayOfWeek = new List<StockAvailabilityByDay>();
    for(int i = 1; i <= 7; i++)
    {
      dayOfWeek.Add(new StockAvailabilityByDay
      {
        StockCode = stockCode,
        DayOfWeek = i,
        IsAvailable = stockAvailability.Where(x => x.DayOfWeek == i).Any(),
      });
    }
    _unitOfWork.Commit();
    return dayOfWeek;
  }

  public async Task<IEnumerable<Assoc>> GetAssocNotInStockCodeAc(StockItemDetailsCommonDto model)
  {
    string stores = string.Empty;
    List<string> ids = new List<string>();
    if(!_config.IsBackOffice)
    {
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      stores = ids.Count > 0 ? String.Join(", ", ids) : "";
    }

    IEnumerable<Assoc> assocs = await _unitOfWork.StockItemDetails.GetAssocNotInStockCodeAc(model.StockCode, stores);
    _unitOfWork.Commit();
    return assocs;
  }

  public async Task<IEnumerable<TaxNameAndCode>> GetTaxesAndCode(StockItemDetailsCommonDto model)
  {
    string stores = string.Empty;
    List<string> ids = new List<string>();
    if (!_config.IsBackOffice)
    {
      if (model.StoreIDs != null || model.StoreIDs?.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      stores = ids.Count > 0 ? String.Join(", ", ids) : "";
    }

    IEnumerable<TaxNameAndCode> taxNameAndCodes = await _unitOfWork.StockItemDetails.GetTaxesAndCode(model.StockCode, stores);
    _unitOfWork.Commit();
    return taxNameAndCodes;
  }

  public async Task<IEnumerable<ChargesAndDeposits>> GetChargesAndDeposits(string stockCode, string storeID)
  {
    IEnumerable<ChargesAndDeposits> chargesAndDeposits = await _unitOfWork.StockItemDetails.GetChargesAndDeposits(stockCode, storeID);
    _unitOfWork.Commit();
    return chargesAndDeposits;
  }

  public async Task<IEnumerable<Assoc>> DeleteCharge(ChargesDto model)
  {
    List<string> ids = new List<string>();
    string stores = string.Empty;
    if (!_config.IsBackOffice)
    {
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      stores = ids.Count > 0 ? String.Join(", ", ids) : "";
    }

    int numOfRows = await _unitOfWork.StockItemDetails.DeleteCharge(model.StockCode, model.ChargeCode, stores);
    if(numOfRows == 0)
    {
      throw new Exception("Failed to delete charges");
    }

    IEnumerable<Assoc> chargesAndDeposits = await _unitOfWork.StockItemDetails.GetAssocNotInStockCodeAc(model.StockCode, stores);
    _unitOfWork.Commit();
    return chargesAndDeposits;
  }

  public async Task<IEnumerable<Assoc>> InsertCharge(ChargesDto model)
  {
    List<string> ids = new List<string>();
    string stores = string.Empty;
    // filter store ids to insert
    List<string> storeIdsToInsert = new List<string>();
    if (!_config.IsBackOffice)
    {
      if (model.StoreIDs != null && model.StoreIDs.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }

        stores = String.Join(",", ids);
        var charges = await _unitOfWork.StockItemDetails.GetCharges(model.StockCode, model.ChargeCode, stores);
        foreach (var id in model.StoreIDs)
        {
          var charge = charges.FirstOrDefault(x => x.StoreID == id);
          if (charge == null)
          {
            storeIdsToInsert.Add(id);
          }
        }
      }
    }

    int numOfRows = await _unitOfWork.StockItemDetails.InsertCharges(model.StockCode, model.ChargeCode, model.SelectionValue, storeIdsToInsert);
    if(numOfRows == 0)
    {
      throw new Exception("Failed to insert charges");
    }

    IEnumerable<Assoc> assocs = await _unitOfWork.StockItemDetails.GetAssocNotInStockCodeAc(model.StockCode, stores);
    _unitOfWork.Commit();
    return assocs;
  }

  public async Task<IEnumerable<Tax>> InsertTaxes(Tax model)
  {
    int numOfRows = 0;
    List<string> ids = new List<string>();
    string stores = string.Empty;
    if (!_config.IsBackOffice)
    {
      if (model.StoreIDs != null || model.StoreIDs?.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      stores = String.Join(",", ids);
      numOfRows += await _unitOfWork.StockItemDetails.DeleteTaxes(model.StockCode, model.TaxName, model.TaxCode, stores);
    }

    numOfRows += await _unitOfWork.StockItemDetails.InsertTaxes(model.StockCode, model.TaxName, model.TaxCode, ids);
    if (numOfRows == 0)
    {
      throw new Exception("Failed to insert Taxes");
    }
    IEnumerable<Tax> taxes = await _unitOfWork.StockItemDetails.GetTaxes(model.StockCode, "ALL");
    _unitOfWork.Commit();
    return taxes;
  }

  public async Task<IEnumerable<Tax>> DeleteTaxes(Tax model)
  {
    int numOfRows = 0;
    List<string> ids = new List<string>();
    string stores = string.Empty;
    if (!_config.IsBackOffice)
    {
      if (model.StoreIDs != null || model.StoreIDs?.Count > 0)
      {
        foreach (var id in model.StoreIDs)
        {
          ids.Add($@"'{id}'");
        }
      }

      stores = String.Join(",", ids);
    }

    numOfRows += await _unitOfWork.StockItemDetails.DeleteTaxes(model.StockCode, model.TaxName, model.TaxCode, stores, false);
    if (numOfRows == 0)
    {
      throw new Exception("Failed to insert Taxes");
    }
    IEnumerable<Tax> taxes = await _unitOfWork.StockItemDetails.GetTaxes(model.StockCode, "ALL");
    _unitOfWork.Commit();
    return taxes;
  }

  public async Task<List<StockAvailabilityByDay>> InsertStockAvailabilityByDay(StockCodeAndAvailabilityByDay stockCodeAndAvailabilityByDay)
  {
    int numOfRows = await _unitOfWork.StockItemDetails.DeleteStockAvailabilityByDay(stockCodeAndAvailabilityByDay.StockCode);
    numOfRows += await _unitOfWork.StockItemDetails.InsertStockAvailabilityByDay(stockCodeAndAvailabilityByDay.AvailabilityByDays);

    List<StockAvailabilityByDay> stockAvailabilityByDays = await GetStockAvailabilityByDay(stockCodeAndAvailabilityByDay.StockCode);
    _unitOfWork.Commit();
    return stockAvailabilityByDays;

  }
}
