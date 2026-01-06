namespace InfonetCSCR.Services;

public class ElectronicJournalService : IElectronicJournalService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;
  private static string _connectionString = "";
  private static bool? _isWareHouse = false;
  private static char? _storeRadioOption = null;

  public ElectronicJournalService(IApplicationConfig _applicationConfig)
  {
    _unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCTransConnectionString);
    _config = _applicationConfig;
  }

  public async Task<IEnumerable<ElectronicJournalReportData>> GetElectronicJournalReportData(string saleNo, bool isFromTills)
  {
    var uow = new UnitOfWork(_config, _connectionString);
    IEnumerable<ElectronicJournalReportData> electronicJournalReportData = await uow.ElectronicJournal.GetElectronicJournalReportDataAsync(saleNo, isFromTills);
    uow.Commit();
    return electronicJournalReportData;
  }

  public async Task<IEnumerable<ElectronicJournalSubreport?>> GetEJSubreportData(string saleNo, bool isFromTills)
  {
    var uow = new UnitOfWork(_config, _connectionString);
    await uow.ElectronicJournal.InsertSelectedSaleCardTenderAsync(_isWareHouse, _storeRadioOption, saleNo);

    IEnumerable<ElectronicJournalSubreport?> EJSubreportData = await uow.ElectronicJournal.GetEJSubreportDataAsync(saleNo, isFromTills);
    uow.Commit();
    return EJSubreportData;
  }

  public async Task<IEnumerable<string>> GetElectronicJournalOptions()
  {
    IEnumerable<string> electronicJournalOptions = await _unitOfWork.ElectronicJournal.GetElectronicJournalOptionsAsync();
    _unitOfWork.Commit();
    return electronicJournalOptions;
  }

  public async Task<IEnumerable<EJSearchDescriptionDto>> GetElectronicJournalFilters(string optionName, bool isWareHouse)
  {
    IEnumerable<EJSearchDescriptionDto> searchDescriptions = await _unitOfWork.ElectronicJournal.GetElectronicJournalFiltersAsync(optionName, isWareHouse);
    _unitOfWork.Commit();
    return searchDescriptions;
  }

  public async Task<IEnumerable<string>> GetSearchDropdownValues(EJSearchDescriptionDto desc)
  {
    IEnumerable<string> searchDescriptions = await _unitOfWork.ElectronicJournal.GetSearchDropdownValuesAsync(desc);
    _unitOfWork.Commit();
    return searchDescriptions;
  }

  public async Task<IEnumerable<StoreOptionDto>> GetStoreDropdownValues()
  {
    IEnumerable<StoreOptionDto> storeOptions = await _unitOfWork.Store.GetStoreDropdownValuesAsync();
    _unitOfWork.Commit();
    return storeOptions;
  }

  public async Task<EJGridDataFilterDto?> GetElectronicJournalGridData(EJSearchParamDto ejSearchParam)
  {
    var isBackOffice = _config.IsBackOffice;
    _storeRadioOption = ejSearchParam.StoreRadioOption;
    _isWareHouse = ejSearchParam.IsWareHouse;

    bool isBoOrFromTrans = isBackOffice || _storeRadioOption != 'C'; // OptionCurrent.Value
    string searchString = BuildSearch(ejSearchParam, isBoOrFromTrans);

    await SetConnectionString(ejSearchParam);

    var tmpUow = new UnitOfWork(_config, _connectionString);
    await tmpUow.ElectronicJournal.TruncateSelectedSaleNoAsync(_isWareHouse, _storeRadioOption);
    await tmpUow.ElectronicJournal.TruncateSelectedSaleCardTenderAsync(_isWareHouse, _storeRadioOption);
    tmpUow.Commit(); // Used separate transactions for deletion and insertion because using single transaction might result in a deadlock

    var uow = new UnitOfWork(_config, _connectionString);
    await uow.ElectronicJournal.InsertSelectedSaleNoAsync(_isWareHouse, _storeRadioOption, searchString);

    IEnumerable<ElectronicJournalReportData>? ejGridData = await uow.ElectronicJournal.GetElectronicJournalGridDataAsync(ejSearchParam, searchString);

    int dataCount = await uow.ElectronicJournal.GetElectronicJournalGridDataCountAsync(ejSearchParam, searchString);

    uow.Commit();

    return new EJGridDataFilterDto
    {
      TotalItems = dataCount,
      ElectronicJournalGridData = ejGridData!
    };
  }

  public async Task<IEnumerable<ElectronicJournalSummaryReport>> GetElectronicJournalSummaryReportData()
  {
    IEnumerable<ElectronicJournalSummaryReport> ejSummaryReportData = await _unitOfWork.ElectronicJournal.GetElectronicJournalSummaryReportDataAsync();
    _unitOfWork.Commit();
    return ejSummaryReportData;
  }

  public async Task<EJSummarySubreportAData> GetEJSummarySubReportAData()
  {
    EJSummarySubreportAData ejSummarySubreportAData = await _unitOfWork.ElectronicJournal.GetEJSummarySubReportADataAsync();
    _unitOfWork.Commit();
    return ejSummarySubreportAData;
  }

  public async Task<IEnumerable<EJSummarySubreportTData>?> GetEJSummarySubReportTData()
  {
    IEnumerable<EJSummarySubreportTData>? ejSummarySubreportAData = await _unitOfWork.ElectronicJournal.GetEJSummarySubReportTDataAsync();
    _unitOfWork.Commit();
    return ejSummarySubreportAData;
  }

  public async Task<IEnumerable<EJDetailsReportData>?> GetEJDetailsReportData(int? pageNumber = null, int? pageSize = null)
  {
    await _unitOfWork.ElectronicJournal.InsertSelectedSaleCardTenderAsync();
    IEnumerable<EJDetailsReportData>? ejDetailsReportData = await _unitOfWork.ElectronicJournal.GetEJDetailsReportDataAsync(pageNumber, pageSize);
    _unitOfWork.Commit();
    return ejDetailsReportData;
  }

  public async Task<string?> GetEJTenderDetailsReportData(int saleNo)
  {
    var uow = new UnitOfWork(_config, _connectionString);
    string? ejTenderDetailsReportData = await uow.ElectronicJournal.GetEJTenderDetailsReportDataAsync(saleNo);
    uow.Commit();
    return ejTenderDetailsReportData;
  }

  private async Task SetConnectionString(EJSearchParamDto ejSearchParam)
  {
    _connectionString = _config.CSCTransConnectionString;

    if (!_config.IsBackOffice && ejSearchParam.IsLocalStoreSelected == false)
    {
      var tmpUowCscMaster = new UnitOfWork(_config, _config.CSCMasterConnectionString);
      Store store = await tmpUowCscMaster.Store.GetStoreByStoreIdAsync(ejSearchParam.SelectedStoreId);
      tmpUowCscMaster.Commit();

      string dbSource = _storeRadioOption == 'C' ? "CSCTills" : "CSCTrans";
      _connectionString = store.Data_Path.Replace("CSCMaster", dbSource);
    }
  }

  private static string BuildSearch(EJSearchParamDto ejSearchParam, bool isBoOrFromTrans = false)
  {
    IEnumerable<EJSearchDescriptionDto>? searchParams = ejSearchParam.EJSearchDescription;

    StringBuilder strSearch = new();

    if (isBoOrFromTrans)
      strSearch.Append($"A.ShiftDate >= '{ejSearchParam.StartDate!:yyyyMMdd}' AND A.ShiftDate <= '{ejSearchParam.EndDate!:yyyyMMdd}' ");
    else
      strSearch.Append($"1 = 1");

    if (!ejSearchParam.SearchDate.IsNullOrEmpty())
    {
      strSearch.Append($"AND A.ShiftDate {ejSearchParam.SearchOperator} '{ejSearchParam.SearchDate:yyyyMMdd}' ");
    }

    if (searchParams?.Count() == 0)
      return strSearch.ToString();

    foreach (EJSearchDescriptionDto? searchParam in searchParams)
    {
      string? andOrOperator = searchParam?.Relation;
      string? fieldType = searchParam?.FieldType;
      string? type = searchParam?.Type;
      string? value = searchParam?.Value;
      string? logicalOperator = searchParam?.Operator;
      string? tableName = searchParam?.TableName;
      string? fieldName = searchParam?.FieldName;

      if (type == "TEXT" && !string.IsNullOrEmpty(value))
      {
        if (fieldType == "STRING")
        {
          strSearch.Append($" {andOrOperator} {tableName}.{fieldName} {value} '{value.Replace("'", "''")}' ");
        }
        else if (fieldType == "NUMERIC")
        {
          if (double.TryParse(value, out double numericValue))
          {
            strSearch.Append($" {andOrOperator} {tableName}.{fieldName} {logicalOperator} {numericValue} ");
          }
          else
          {
            strSearch.Append($" {andOrOperator} {tableName}.{fieldName} {logicalOperator} {0} ");
          }
        }
      }
      else if (type == "TABLE" && !string.IsNullOrEmpty(value))
      {
        string convertWildcards = value.Replace('*', '%').Replace('?', '_');
        string? selKeyField = searchParam?.SelKeyField?.ToString();
        string? selDBName = searchParam?.SelDBName?.ToString();
        string? selTableName = searchParam?.SelTableName?.ToString();
        string? selFieldName = searchParam?.SelFieldName?.ToString();

        if (value.Contains('*') || value.Contains('?'))
        {
          strSearch.Append($" {andOrOperator} {tableName}.{fieldName} IN (" +
                       $"SELECT {selKeyField} FROM {selDBName}.dbo.{selTableName} " +
                       $"WHERE {selFieldName} LIKE '{convertWildcards}')");
        }
        else
        {
          strSearch.Append($" {andOrOperator} {tableName}.{fieldName} IN (" +
                       $"SELECT {selKeyField} FROM {selDBName}.dbo.{selTableName} " +
                       $"WHERE {selFieldName} = '{value.Replace("'", "''")}')");
        }
      }
      else if (type == "DATE")
      {
        strSearch.Append($" {andOrOperator} {tableName}.{fieldName} {logicalOperator} '{value:yyyyMMdd}'");
      }
      else if (type == "TIME")
      {
        strSearch.Append($" {andOrOperator} CONVERT(CHAR(8), {tableName}.{fieldName}, 8) {logicalOperator} '{value:HH:mm}:00'");
      }
    }

    return strSearch.ToString();
  }
}
