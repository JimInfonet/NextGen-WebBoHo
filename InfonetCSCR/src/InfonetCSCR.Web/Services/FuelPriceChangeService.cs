namespace InfonetCSCR.Web.Services;

public class FuelPriceChangeService : IFuelPriceChangeService
{
  private readonly IHttpService _http;
  private readonly IHttpContextAccessor _contextAccessor;

  public FuelPriceChangeService(IHttpService http, IHttpContextAccessor contextAccessor)
  {
    _http = http;
    _contextAccessor = contextAccessor;
  }

  public async Task<StoreDataInHoDbRecordViewModel?> CheckStoreDataInHeadOfficeDatabase(FuelPriceChangeStoreCheckRecordViewModel fuelPriceChangeStoreCheckRecordViewModel)
  {
    string url = "FuelPriceChange/CheckStoreDataInHeadOfficeDatabase";
    StoreDataInHoDbRecordViewModel? storeDataInHoDbRecordViewModel = 
      await _http.PostAndParseAsync<StoreDataInHoDbRecordViewModel>(url, fuelPriceChangeStoreCheckRecordViewModel);
    return storeDataInHoDbRecordViewModel;
  }

  public async Task<IList<GradeTierServiceLevelTableDataViewModel>> GetDataFromGradeTierAndServiceLevelTable(GradeGridRecordViewModel gradeGridRecord)
  {
    string url = "FuelPriceChange/GetDataFromGradeTierAndServiceLevelTable";
    IList<GradeTierServiceLevelTableDataViewModel> gradeTierServiceLevelTableData = 
      await _http.PostAndParseAsync<IList<GradeTierServiceLevelTableDataViewModel>>(url, gradeGridRecord);
    return gradeTierServiceLevelTableData;
  }

  public async Task<bool> TransferFuelPriceChange(IList<FuelPriceChangeTransferRecordViewModel> storesData)
  {
    string user = _contextAccessor.HttpContext?.Session.GetString(SessionKeys.UserCode)!;
    foreach (var storeData in storesData)
    {
      storeData.EmployeeId = user.ToLower();
    }

    string url = "FuelPriceChange/TransferFuelPriceChange";
    bool isTransfered = await _http.PostAndParseAsync<bool>(url, storesData);
    return isTransfered;
  }
}
