namespace InfonetCSCR.Web.Abstractions.Services;

public interface IElectronicJournalService
{
  Task<IEnumerable<string>?> GetElectronicJournalOptions();
  Task<IEnumerable<EJSearchDescriptionViewModel>?> GetElectronicJournalFilters(string optionName, bool isWareHouse);
  Task<IEnumerable<string>?> GetSearchDropdownValues(EJSearchDescriptionViewModel desc);
  Task<string> GetSetupCStore();
  Task<IEnumerable<StoreOptionViewModel>?> GetStoreDropdownValues();
  Task<EJGridDataFilterViewModel> GetElectronicJournalGridData(EJSearchParamViewModel ejSearchParam);
  Task<IEnumerable<EJDetailsReportDataViewModel>> GetEJDetailReportData(int? pageNumber = null, int? pageSize = null);
}
