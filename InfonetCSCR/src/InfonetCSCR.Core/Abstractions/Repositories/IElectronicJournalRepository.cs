namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IElectronicJournalRepository
{
  Task<IEnumerable<ElectronicJournalReportData>> GetElectronicJournalReportDataAsync(string saleNo, bool isFromTills = false);
  Task<IEnumerable<ElectronicJournalSubreport?>> GetEJSubreportDataAsync(string saleNo, bool isFromTills = false);
  Task<IEnumerable<string>> GetElectronicJournalOptionsAsync();
  Task<IEnumerable<EJSearchDescriptionDto>> GetElectronicJournalFiltersAsync(string optionName, bool isWareHouse);
  Task<IEnumerable<string>> GetSearchDropdownValuesAsync(EJSearchDescriptionDto desc);
  Task<IEnumerable<ElectronicJournalReportData>?> GetElectronicJournalGridDataAsync(EJSearchParamDto ejSearchParam, string searchString);
  Task<int> GetElectronicJournalGridDataCountAsync(EJSearchParamDto ejSearchParam, string searchString);
  Task<bool> TruncateSelectedSaleNoAsync(bool? isWareHouse, char? storeRadioOption);
  Task<bool> TruncateSelectedSaleCardTenderAsync(bool? isWareHouse, char? storeRadioOption);
  Task<bool> InsertSelectedSaleNoAsync(bool? isWareHouse, char? storeRadioOption, string searchString);
  Task<bool> InsertSelectedSaleCardTenderAsync(bool? isWareHouse = null, char? storeRadioOption = null, string? saleNo = null);
  Task<IEnumerable<ElectronicJournalSummaryReport>?> GetElectronicJournalSummaryReportDataAsync();
  Task<EJSummarySubreportAData> GetEJSummarySubReportADataAsync();
  Task<IEnumerable<EJSummarySubreportTData>?> GetEJSummarySubReportTDataAsync();
  Task<IEnumerable<EJDetailsReportData>?> GetEJDetailsReportDataAsync(int? pageNumber = null, int? pageSize = null);
  Task<string?> GetEJTenderDetailsReportDataAsync(int saleNo);
}
