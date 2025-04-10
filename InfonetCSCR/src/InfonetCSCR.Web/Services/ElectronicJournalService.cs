namespace InfonetCSCR.Web.Services;

public class ElectronicJournalService : IElectronicJournalService
{
  private readonly IHttpService _http;
  
  public ElectronicJournalService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IEnumerable<string>?> GetElectronicJournalOptions()
  {
    string url = "ElectronicJournal/GetElectronicJournalOptions";
    return await _http.GetAndParseAsync<IEnumerable<string>?>(url);
  }

  public async Task<IEnumerable<EJSearchDescriptionViewModel>?> GetElectronicJournalFilters(string optionName, bool isWareHouse)
  {
    string url = $"ElectronicJournal/GetElectronicJournalFilters?optionName={optionName}&isWareHouse={isWareHouse}";
    return await _http.GetAndParseAsync<IEnumerable<EJSearchDescriptionViewModel>?>(url);
  }

  public async Task<IEnumerable<string>?> GetSearchDropdownValues(EJSearchDescriptionViewModel desc)
  {
    string url = $"ElectronicJournal/GetSearchDropdownValues";
    return await _http.PostAndParseAsync<IEnumerable<string>?>(url, desc);
  }

  public async Task<string> GetSetupCStore()
  {
    string url = $"CompanySetup/GetCompanyStoreCode";
    return await _http.GetAndParseAsync<string>(url) ?? "";
  }
  public async Task<IEnumerable<StoreOptionViewModel>?> GetStoreDropdownValues()
  {
    string url = $"ElectronicJournal/GetStoreDropdownValues";
    return await _http.GetAndParseAsync<IEnumerable<StoreOptionViewModel>?>(url);
  }
  
  public async Task<EJGridDataFilterViewModel> GetElectronicJournalGridData(EJSearchParamViewModel ejSearchParam)
  {
    string url = $"ElectronicJournal/GetElectronicJournalGridData";
    return await _http.PostAndParseAsync<EJGridDataFilterViewModel>(url, ejSearchParam);
  }

  public async Task<IEnumerable<EJDetailsReportDataViewModel>> GetEJDetailReportData(int? pageNumber = null, int? pageSize = null)
  {
    string url = $"ElectronicJournal/GetEJDetailsReportData?pageNumber={pageNumber}&pageSize={pageSize}";
    var ejDetails = await _http.GetAndParseAsync< IEnumerable<EJDetailsReportDataViewModel>>(url);

    foreach(var detail in ejDetails)
    {
      detail.FormattedQty = FormatDecimal(detail.Qty, "N2");
      detail.FormattedPrice = FormatDecimal(detail.Price, "C2");
      detail.FormattedDisc = FormatDecimal(detail.Disc, "C2");
      detail.FormattedAmount = FormatDecimal(detail.Amount - detail.Disc, "C2");
      detail.FormattedInvoiceDiscount = detail.InvoiceDiscount.HasValue ? FormatDecimal(detail.InvoiceDiscount.Value, "C2") : "";
      detail.FormattedTaxAddedAmount = detail.TaxAddedAmount.HasValue ? FormatDecimal(detail.TaxAddedAmount.Value, "C2") : "";
      detail.FormattedTaxIncludedAmount = detail.TaxIncludedAmount.HasValue ? FormatDecimal(detail.TaxIncludedAmount.Value, "C2") : "";
      detail.FormattedSaleAmt = detail.SaleAmt.HasValue ? FormatDecimal(detail.SaleAmt.Value, "C2") : "";
      detail.FormattedTendAmt = detail.TendAmt.HasValue ? FormatDecimal(detail.TendAmt.Value, "C2") : "";
      detail.FormattedChangeAmt = detail.ChangeAmt.HasValue ? FormatDecimal(detail.ChangeAmt.Value, "C2") : "";
      detail.FormattedOverPaymentAmt = detail.OverPaymentAmt.HasValue ? FormatDecimal(detail.OverPaymentAmt.Value, "C2") : "";
      detail.FormattedAmtUsed = detail.AmtUsed.HasValue ? FormatDecimal(detail.AmtUsed.Value, "C2") : "";
      detail.FormattedSaleTime = FormtDateTimeValue(detail.SaleTime, "h:mm tt");
      detail.FormattedSaleDate = FormtDateTimeValue(detail.SaleDate, "MM/dd/yyyy");
      detail.FormattedShiftDate = FormtDateTimeValue(detail.ShiftDate, "MM/dd/yyyy");
    }

    return ejDetails;
  }

  private string FormatDecimal(double value, string format)
  {
    string formattedValue = Math.Abs(value).ToString(format);

    if (value < 0)
    {
      formattedValue = $"({formattedValue})";
    }

    return formattedValue;
  }

  private string FormtDateTimeValue(string value, string format)
  {
    if (DateTime.TryParse(value, out DateTime dateTime))
    {
      return dateTime.ToString(format);
    }

    return string.Empty;
  }

}
