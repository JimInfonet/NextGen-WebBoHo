namespace InfonetCSCR.Web.Services;

public class TimesheetReportsService : ITimesheetReportsService
{
  private readonly IHttpService _http;
  private readonly IPolicyService _policy;
  private readonly IApplicationConfig _config;
  private readonly ILogger<TimesheetReportsService> _logger;
  private readonly ICheckInCheckOutService _checkInCheckOutService;

  public TimesheetReportsService(IHttpService http,
    IPolicyService policy,
    IApplicationConfig config,
    ILogger<TimesheetReportsService> logger,
    ICheckInCheckOutService checkInCheckOutService)
  {
    _http = http;
    _policy = policy;
    _config = config;
    _logger = logger;
    _checkInCheckOutService = checkInCheckOutService;
  }

  public async Task<(string contents, string storeCode)> GeneratePYFile(GeneratePYFileDtoViewModel model, CheckInCheckOutPoliciesDtoViewModel policies)
  {
    GeneratePYFileReturnDtoViewModel data = await GetDataToGeneratePYFile(model);

    string userId = model.UserId.PadLeftFixed(3, '0');
    bool isHourly = data.IsHourly;
    var storeCode = data.StoreCode.PadLeftFixed(3, '0');
    var timesheetHeaders = data.TimesheetHeaders;
    var shortages = data.Shortages;
    var timesheetDetails = data.TimesheetDetails;
    var cashShortages = data.CashShortages;
    var priorShortages = data.PriorShortages;
    var totalHours = data.TotalHours;
    var overtimes = data.Overtimes;
    var statutoryHours = data.StatutoryHours;
    var totalCommissions = data.TotalCommissions;

    string contents = "";

    for (int headerIndex = 0; headerIndex < data.TimesheetHeaders.Count; ++headerIndex)
    {
      // HEADER
      string reportId = timesheetHeaders[headerIndex].ReportId.ToString().PadLeftFixed(5, '0');
      string payDate = timesheetHeaders[headerIndex].PayDate.ToString("yyyyMMdd");
      string periodStarting = timesheetHeaders[headerIndex].PeriodStarting.ToString("yyyyMMdd");
      string periodEnding = timesheetHeaders[headerIndex].PeriodEnding.ToString("yyyyMMdd");
      contents += $"{storeCode}|H|{reportId}|{userId}|{payDate}|{periodStarting}|{periodEnding}\n";

      // PRIOR
      foreach (var shortage in shortages[headerIndex])
      {
        string shiftDate = shortage.ShiftDate?.ToString("yyyyMMdd") ?? "";
        contents += $"{storeCode}|P|{shiftDate}|{shortage.Shortage}";
      }

      // DETAIL
      foreach (var td in timesheetDetails[headerIndex])
      {
        contents += GetFormattedLine(td, storeCode);
      }

      // TOTAL
      string line = $"{storeCode}|T|";

      // Cash shortages
      double cashShortage = 0;
      if (cashShortages[headerIndex] != 0) cashShortage = cashShortages[headerIndex];

      // Prior shortages
      double priorShortage = 0;
      if (priorShortages[headerIndex] > 0) priorShortage = priorShortages[headerIndex];

      // Cash shortage + prior shortage
      line += $"{SpFormat(cashShortage + priorShortage)}|";

      // Uniform purchases
      line += $"{SpFormat(timesheetHeaders[headerIndex].UniformPurchases)}|";

      // TOTAL CHARGES  total charges = cash shortages + uniform + prior charges
      line += $"{SpFormat(priorShortage + timesheetHeaders[headerIndex].UniformPurchases)}|";

      // Total hours
      double hours = 0;
      if (totalHours[headerIndex] > 0) hours = totalHours[headerIndex];

      double overtime = 0;
      int overtimeValue = isHourly ? policies.OvertimeH : policies.OvertimeS;

      foreach (var ot in overtimes[headerIndex])
      {
        if (ot > overtimeValue) overtime += ot - overtimeValue;
      }

      // Statutory hours calculations
      double stat = 0;
      if (statutoryHours[headerIndex] > 0) stat = statutoryHours[headerIndex];

      double totalHoursPaid = 0;
      if (isHourly)
      {
        totalHoursPaid = (hours - overtime - stat + overtime * 1.5 + stat * 1.5);
      }
      else totalHoursPaid = hours - overtime * 1.5;

      // Commission
      double commission = 0;
      if (totalCommissions[headerIndex] > 0) commission = totalCommissions[headerIndex];

      line += $"{SpFormat(hours - overtime - stat)}|"; // regular
      line += $"{SpFormat(overtime)}|"; // overtime
      line += $"{SpFormat(stat)}|"; // statutory hours
      line += $"{SpFormat(hours)}|"; // total hours
      line += $"{SpFormat(totalHoursPaid)}|"; // total hours paid
      line += $"{SpFormat(commission)}"; // commission

      line += "\n";
      contents += line;
    }

    return (contents, storeCode);
  }

  #region Private Methods
  private async Task<GeneratePYFileReturnDtoViewModel> GetDataToGeneratePYFile(GeneratePYFileDtoViewModel model)
  {
    string url = $"TimesheetReports/GetDataToGeneratePYFile";
    var response = await _http.PostAsync(url, model);
    var data = new GeneratePYFileReturnDtoViewModel { };

    if (response.IsSuccessStatusCode)
    {
      data = await response.Content.ReadFromJsonAsync<GeneratePYFileReturnDtoViewModel>();
    }
    return data;
  }

  private string GetFormattedLine(TimesheetDetailDtoViewModel td, string storeCode)
  {
    string shiftDate = td.ShiftDate.ToString("yyyyMMdd");
    string statStr = td.Stat ? "Y" : "N";
    string timeIn = td.TimeIn.ToString("hh\\:mm");
    string timeOut = td.TimeOut.ToString("hh\\:mm");
    string training = td.Training ? "Y" : "N";

    string line = $"{storeCode}|D|{shiftDate}|{statStr}|{timeIn}|{timeOut}|{SpFormat(td.Hours)}|{SpFormat(td.Shortages)}|{SpFormat(td.Commission ?? 0)}|{training}|{td.Comment}\n";

    return line;
  }

  private string SpFormat(double value)
  {
    string sign = value >= 0 ? "+" : "-";
    return sign + Math.Abs(Math.Round(value, 2) * 100).ToString().PadLeftFixed(8, '0');
  }
  #endregion
}
