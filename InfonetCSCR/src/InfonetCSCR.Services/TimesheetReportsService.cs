namespace InfonetCSCR.Services;

public class TimesheetReportsService : ITimesheetReportsService
{
  #region Private Properties & Constructor
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;
  private readonly ICheckInCheckOutService _checkInCheckOutService;

  public TimesheetReportsService(IApplicationConfig config, ICheckInCheckOutService checkInCheckOutService)
  {
    _config = config;
    _checkInCheckOutService = checkInCheckOutService;
    _unitOfWork = new UnitOfWork(config, config.CSCTransConnectionString);
  }
  #endregion

  public async Task<List<TimeSheetHeader>> GetReports(GetReportsDto model)
  {
    var reports = new List<TimeSheetHeader>();
    string status = (model.IsViewCurrent ? TimesheetDetailStatusType.Working : TimesheetDetailStatusType.Complete).ToString();

    if (model.Policies.TSSignpad == false)
    {
      if (model.Policies.ViewReports)
      {
        reports = (await _unitOfWork.TimesheetReports.GetReports(status)).ToList();
      }
      else
      {
        reports = (await _unitOfWork.TimesheetReports.GetReports(status, model.UserCode)).ToList();
      }
    }
    else
    {
      if (model.Policies.ViewReports)
      {
        reports = (await _unitOfWork.TimesheetReports.GetReportsBySignature(!model.IsViewCurrent)).ToList();
      }
      else
      {
        reports = (await _unitOfWork.TimesheetReports.GetReports(status, model.UserCode)).ToList();
      }
    }

    _unitOfWork.Commit();
    return reports;
  }

  public async Task<TimesheetHeaderWithUserDetailsDto> GetTimesheetHeaderDetails(int reportId)
  {
    var timesheetHeader = await _unitOfWork.TimesheetReports.GetTimesheetHeaderDetails(reportId);
    _unitOfWork.Commit();
    return timesheetHeader;
  }

  public async Task<List<OutstandingStortageDto>> GetOutstandingShortages(string userCode, int reportId, DateTime periodStarting)
  {
    var outstandingShortages = (await _unitOfWork.TimesheetReports.GetOutstandingShortages(userCode, reportId, periodStarting)).ToList();
    _unitOfWork.Commit();
    return outstandingShortages;
  }

  public async Task<bool> UpdateShortages()
  {
    int i = 0;
    List<ShortageDto> shortages = (await _unitOfWork.TimesheetReports.GetShortages()).ToList();

    var timesheetDetailTasks = shortages.Select(async shortage =>
    {
      var timesheetDetails = await _unitOfWork.CheckInCheckOut.GetTimeSheetDetails(shortage.UserCode!, shortage.ShiftDate ?? DateTime.Today);
      return timesheetDetails.ToList();
    });

    List<TimesheetDetailDto>[] timesheetDetails = await Task.WhenAll(timesheetDetailTasks);
    var updateTimesheetDetailTasks = new List<Task<bool>>();
    var updateShortagesTasks = new List<Task<int>>();

    for (int recNum = 0; recNum < shortages.Count; ++recNum)
    {
      if (timesheetDetails[recNum].Count > 1)
      {
        // Split shift - find the right record
        i = 0;
        while (i != shortages[i].Shift)
        {
          // INFO: skipped a if block as it is not doing anything meaningful
          i = i + 1;
        }
      }

      if (timesheetDetails[recNum].Count > 0)
      {
        TimesheetDetailDto timesheetDetail = timesheetDetails[recNum][0];
        timesheetDetail.Shortages += shortages[recNum].Shortage ?? 0;

        updateTimesheetDetailTasks.Add(_unitOfWork.CheckInCheckOut.UpdateTimeSheetDetail(timesheetDetail));
        updateShortagesTasks.Add(_unitOfWork.TimesheetReports.UpdateReportIdInShortages(shortages[i].UserCode!, timesheetDetail.ReportId, shortages[i].ShiftDate ?? DateTime.Today));
      }
    }

    await Task.WhenAll(updateTimesheetDetailTasks);
    await Task.WhenAll(updateShortagesTasks);

    _unitOfWork.Commit();
    return true;
  }

  public async Task<bool> UpdateMissingReportIdInShortages(string userCode, int reportId, DateTime periodStarting)
  {
    var updated = await _unitOfWork.TimesheetReports.UpdateMissingReportIdInShortages(userCode, reportId, periodStarting);
    _unitOfWork.Commit();
    return updated >= 0;
  }

  public async Task<List<TimesheetDetailDto>> GetTimeSheetDetailsByReportId(string userCode, int reportId)
  {
    var timesheetDetails = (await _unitOfWork.CheckInCheckOut.GetTimeSheetDetailsByReportId(userCode, reportId)).ToList();
    _unitOfWork.Commit();
    return timesheetDetails;
  }

  public async Task<bool> UpdateUniformPurchases(string userCode, double uniformPurchases, DateTime periodStarting, DateTime periodEnding)
  {
    var updated = await _unitOfWork.TimesheetReports.UpdateUniformPurchases(userCode, uniformPurchases, periodStarting, periodEnding);
    _unitOfWork.Commit();
    return updated >= 0;
  }

  public async Task<bool> MissingCheckOutsExist(string userCode, DateTime periodEnding)
  {
    bool missingCheckOutsExist = await _unitOfWork.TimesheetReports.MissingCheckOutsExist(userCode, periodEnding);
    _unitOfWork.Commit();
    return missingCheckOutsExist;
  }

  public async Task<bool> MissingCommentExist(string userCode)
  {
    bool missingCommentExist = await _unitOfWork.TimesheetReports.MissingCommentExist(userCode);
    _unitOfWork.Commit();
    return missingCommentExist;
  }

  public async Task<bool> PostPayPeriod(string userCode, DateTime periodStarting)
  {
    bool posted = await _unitOfWork.TimesheetReports.PostPayPeriod(userCode, periodStarting);
    _unitOfWork.Commit();
    return posted;
  }

  public async Task<GeneratePYFileReturnDto> GetDataToGeneratePYFile(GeneratePYFileDto model)
  {
    bool isHourly = (await GetUserType(model.UserCode)).ToUpper() == "HOURLY";
    string storeCode = await GetSetupCStoreCode();
    List<TimeSheetHeader> timesheetHeaders = (await _unitOfWork.TimesheetReports.GetTimesheetHeadersForPosting(model.UserCode, model.PeriodStarting, model.PeriodEnding)).ToList();

    var shortageTasks = new List<Task<IEnumerable<ShortageDto>>>();
    var timesheetDetailTasks = new List<Task<IEnumerable<TimesheetDetailDto>>>();
    var cashShortageTasks = new List<Task<double>>();
    var priorShortageTasks = new List<Task<double>>();
    var totalHourTasks = new List<Task<double>>();
    var overtimeTasks = new List<Task<IEnumerable<double>>>();
    var statutoryHourTasks = new List<Task<double>>();
    var totalCommissionTasks = new List<Task<double>>();

    foreach (var th in timesheetHeaders)
    {
      shortageTasks.Add(_unitOfWork.TimesheetReports.GetShortagesForPosting(model.UserCode, th.PeriodStarting, th.ReportId));
      timesheetDetailTasks.Add(_unitOfWork.TimesheetReports.GetTimesheetDetailsBasedOnShiftType(model.UserCode, th.ReportId, th.SplitShift));
      cashShortageTasks.Add(_unitOfWork.TimesheetReports.GetCashShortages(model.UserCode, th.ReportId));
      priorShortageTasks.Add(_unitOfWork.TimesheetReports.GetPriorShortages(model.UserCode, th.ReportId, th.PeriodStarting));
      totalHourTasks.Add(_unitOfWork.TimesheetReports.GetTotalHours(model.UserCode, th.ReportId));
      overtimeTasks.Add(_unitOfWork.TimesheetReports.GetOvertimes(model.UserCode, th.ReportId));
      statutoryHourTasks.Add(_unitOfWork.TimesheetReports.GetTotalHours(model.UserCode, th.ReportId, true));
      totalCommissionTasks.Add(_unitOfWork.TimesheetReports.GetTotalCommissions(model.UserCode, th.ReportId));
    }

    IEnumerable<ShortageDto>[] shortages = await Task.WhenAll(shortageTasks);
    IEnumerable<TimesheetDetailDto>[] timesheetDetails = await Task.WhenAll(timesheetDetailTasks);
    double[] cashShortages = await Task.WhenAll(cashShortageTasks);
    double[] priorShortages = await Task.WhenAll(priorShortageTasks);
    double[] totalHours = await Task.WhenAll(totalHourTasks);
    IEnumerable<double>[] overtimes = await Task.WhenAll(overtimeTasks);
    double[] statutoryHours = await Task.WhenAll(statutoryHourTasks);
    double[] totalCommissions = await Task.WhenAll(totalCommissionTasks);

    return new GeneratePYFileReturnDto
    {
      IsHourly = isHourly,
      StoreCode = storeCode,
      TimesheetHeaders = timesheetHeaders,
      Shortages = shortages,
      TimesheetDetails = timesheetDetails,
      CashShortages = cashShortages,
      PriorShortages = priorShortages,
      TotalHours = totalHours,
      Overtimes = overtimes,
      StatutoryHours = statutoryHours,
      TotalCommissions = totalCommissions,
    };
  }

  public async Task<UpdateShiftFormReturnDto> UpdateShiftForm(UpdateShiftFormDto model)
  {
    double hours = model.Hours;
    double commission = model.Commission ?? 0.00;

    if (model.CellType.ToUpper() == ShiftFormCellType.TimeIn.ToString().ToUpper())
    {
      if (model.TimeOut != TimeSpan.Zero)
      {
        model.Hours = CheckInCheckOutHelper.CalculateHours(model.NewTimeIn ?? TimeSpan.Zero, model.TimeOut, model.PolicyRoundTime);
        hours = model.Hours;
      }
    }
    else if (model.CellType.ToUpper() == ShiftFormCellType.TimeOut.ToString().ToUpper())
    {
      model.Hours = CheckInCheckOutHelper.CalculateHours(model.TimeIn, model.TimeOut, model.PolicyRoundTime);
      hours = model.Hours;
      await _checkInCheckOutService.CalculateCommision(model.UserCode, model.ShiftDate);
      commission = await _unitOfWork.CheckInCheckOut.GetCommissionFromTimeSheet(model.ShiftDate, model.UserCode) ?? 0;
    }

    bool updated = await _unitOfWork.CheckInCheckOut.UpdateTimeSheetDetail(model, model.NewTimeIn);
    
    _unitOfWork.Commit();
    return new UpdateShiftFormReturnDto
    {
      Updated = updated,
      Hours = hours,
      Commission = commission,
    };
  }

  public async Task<TimeSheetReportDto> GetTimeSheetReportData(long reportId, string userCode, DateTime shiftDate)
  {
    TimeSheetReportDto data = new();
    IEnumerable<TimeSheetMainReportDto> timeSheetMainReportData = await _unitOfWork.TimesheetReports.GetTimeSheetReportDataAsync(reportId, userCode);
    IEnumerable<ShortageDto> shortagesData = await _unitOfWork.TimesheetReports.GetShortagesReportData(reportId, userCode, shiftDate);
    IEnumerable<TimesheetDetail> timesheetDetails = await _unitOfWork.TimesheetReports.GetTimesheetDetailReportData(reportId, userCode);
    data.timeSheetMainReportData = timeSheetMainReportData;
    data.shortageData = shortagesData;
    data.timesheetDetailsData = timesheetDetails;
    _unitOfWork.Commit();
    return data;
  }

  #region Private Methods
  private async Task<string> GetUserType(string userCode)
  {
    var _unitOfWorkAdmin = new UnitOfWork(_config, _config.CSCAdminConnectionString);
    string userType = await _unitOfWorkAdmin.Users.GetUserTypeByCodeAsync(userCode);
    _unitOfWorkAdmin.Commit();

    return userType;
  }

  private async Task<string> GetSetupCStoreCode()
  {
    var _unitOfWorkAdmin = new UnitOfWork(_config, _config.CSCAdminConnectionString);
    Company? company = await _unitOfWorkAdmin.CompanySetups.GetCompanyAsync();
    _unitOfWorkAdmin.Commit();

    string storeCode = company?.StoreCode ?? "";
    return storeCode; 
  }
  #endregion
}
