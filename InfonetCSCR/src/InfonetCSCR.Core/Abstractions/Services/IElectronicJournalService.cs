namespace InfonetCSCR.Core.Abstractions.Services;

public interface IElectronicJournalService
{
  Task<IEnumerable<ElectronicJournalReportData>> GetElectronicJournalReportData(string saleNo, bool isFromTills);
  Task<IEnumerable<ElectronicJournalSubreport?>> GetEJSubreportData(string saleNo, bool isFromTills);
  Task<IEnumerable<string>> GetElectronicJournalOptions();
  Task<IEnumerable<EJSearchDescriptionDto>> GetElectronicJournalFilters(string optionName, bool isWareHouse);
  Task<IEnumerable<string>> GetSearchDropdownValues(EJSearchDescriptionDto desc);
  Task<IEnumerable<StoreOptionDto>> GetStoreDropdownValues();
  Task<EJGridDataFilterDto?> GetElectronicJournalGridData(EJSearchParamDto ejSearchParam);
  Task<IEnumerable<ElectronicJournalSummaryReport>> GetElectronicJournalSummaryReportData();
  Task<EJSummarySubreportAData> GetEJSummarySubReportAData();
  Task<IEnumerable<EJSummarySubreportTData>?> GetEJSummarySubReportTData();
  Task<IEnumerable<EJDetailsReportData>?> GetEJDetailsReportData(int? pageNumber = null, int? pageSize = null);
  Task<string?> GetEJTenderDetailsReportData(int saleNo);
}
