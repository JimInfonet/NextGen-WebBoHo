namespace InfonetCSCR.Services;

public class CheckInCheckOutService : ICheckInCheckOutService
{
  #region Private Properties
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;
  #endregion

  public CheckInCheckOutService(IApplicationConfig config)
  {
    this._unitOfWork = new UnitOfWork(config, config.CSCTransConnectionString);
    _config = config;
  }

  public async Task<IEnumerable<CheckInCheckOutUserDto>> GetCurrentUsers()
  {
    IEnumerable<CheckInCheckOutUserDto> users = await _unitOfWork.CheckInCheckOut.GetCurrentUsersAsync();
    _unitOfWork.Commit();

    return users;
  }

  public async Task<DetermineInOrOutDto> DetermineInOrOut(bool policyHOUR24STORE, string userCode)
  {
    bool checkedOut = true;
    bool checkOutPreviousDay = false;
    int numRec = 0;
    List<TimesheetDetailDto> details = (await _unitOfWork.CheckInCheckOut.GetTimeSheetDetails(userCode, DateTime.Today)).ToList();

    if (details.Count == 0)
    {
      // No records for today's date, which means 1) running for the 1st time OR 2) new pay period
      numRec = 0;
      if (policyHOUR24STORE)
      {
        // 24 HOUR store - the user could have forgotten to check out, get the latest check-in
        DateTime? lastCheckIn = await _unitOfWork.CheckInCheckOut.GetLastCheckIn(userCode);
        if (lastCheckIn != null)
        {
          // Get the details of the last check-in
          List<DateTime> timeOuts = (await _unitOfWork.CheckInCheckOut.GetTimeOutsByShiftDate(userCode, lastCheckIn ?? DateTime.Today)).ToList();

          if (timeOuts.Count > 0)
          {
            int lastIndex = timeOuts.Count() - 1;
            DateTime lastTimeOut = timeOuts[lastIndex];

            // if last timeout == "00:00", user forgot to check out.
            // Otherwise, the user checked out, so now it is check-in
            checkedOut = lastTimeOut != DateTime.Parse("00:00"); 
          }
          // else - By default if no records then it is check-in
        }
        // else - No records in timesheetdetails, first time running
      }
      else
      {
        // For non 24 hours stores that close at midnight the employees cannot check out after midnight
        // system considers a new day, give them 1 hour to check out
        if (DateTime.Now <= DateTime.Parse("3:00 AM")) // Nicolette, June 04, 2013 changed to 3 after discussing to Mr. Gas
        {
          // Check-Out
          checkedOut = false;
          checkOutPreviousDay = true;
          numRec = 1;
        }
        else checkedOut = true; // Check-In
      }
    }
    else if (details.Count == 1)
    {
      // There is one record that was either created before and we are checking in (so check if forgot to checkout for previous shift)
      // OR we are checking in
      numRec = details.Count;
      if (policyHOUR24STORE)
      {
        // 24 HOUR store - the user could have forgotten to check out, get the latest check-in
        DateTime? lastCheckIn = await _unitOfWork.CheckInCheckOut.GetLastCheckIn(userCode);
        if (lastCheckIn != null)
        {
          // Get details of the last check-in
          List<DateTime> timeOuts = (await _unitOfWork.CheckInCheckOut.GetTimeOutsByShiftDate(userCode, lastCheckIn ?? DateTime.Today)).ToList();

          if (timeOuts.Count > 0)
          {
            int lastIndex = timeOuts.Count - 1;
            DateTime lastTimeOut = timeOuts[lastIndex];

            if (lastTimeOut == DateTime.Parse("00:00")) checkedOut = false; // Check-Out
            else checkedOut = HasCheckedOut(details[0].TimeIn, details[0].TimeOut) ?? checkedOut;
          }
        }
        else checkedOut = HasCheckedOut(details[0].TimeIn, details[0].TimeOut) ?? checkedOut;
      }
      else
      {
        // NOT 24 HOUR STORE
        checkedOut = HasCheckedOut(details[0].TimeIn, details[0].TimeOut) ?? checkedOut;

        // To fix the issue with stores that are closing at midnight and the employees cannot check out
        if (DateTime.Now > DateTime.Parse("12:00 AM") && DateTime.Now <= DateTime.Parse("3:00 AM"))
        {
          List<TimesheetDetailDto> lastDayDetails = (await _unitOfWork.CheckInCheckOut.GetTimeSheetDetailsForLastDay(userCode)).ToList();

          if (lastDayDetails.Count > 0)
          {
            int lastIndex = lastDayDetails.Count - 1;
            TimesheetDetailDto lastDetailOfLastDay = lastDayDetails[lastIndex];

            if (lastDetailOfLastDay.TimeIn != TimeSpan.Parse("00:00")
              && lastDetailOfLastDay.TimeOut == TimeSpan.Parse("00:00"))
            {
              checkedOut = false; // Check-Out
              checkOutPreviousDay = true;
            }
          }
        }
      }
    }
    else if (details.Count > 1)
    {
      // more than one record for this date, that means the first record has been checked-in and out.
      // second record was created when checking-in, so now that means we r checking out

      List<TimesheetDetailDto> allDetails = (await _unitOfWork.CheckInCheckOut.GetTimeSheetDetailsOrderByTimeIn(userCode)).ToList();
      int lastIndex = allDetails.Count - 1;
      TimesheetDetailDto lastDetail = allDetails[lastIndex];

      if (lastDetail.TimeIn != TimeSpan.Parse("00:00") && lastDetail.TimeOut != TimeSpan.Parse("00:00"))
      {
        // new record so it is check-in
        checkedOut = true; // Check-In
      }
      else if (lastDetail.TimeIn != TimeSpan.Parse("00:00") && lastDetail.TimeOut == TimeSpan.Parse("00:00"))
      {
        checkedOut = false; // Check-Out
      }
      numRec = allDetails.Count;
    }

    _unitOfWork.Commit();
    return new DetermineInOrOutDto
    {
      NumRec = numRec,
      CheckedOut = checkedOut,
      CheckOutPreviousDay = checkOutPreviousDay
    };
  }

  public async Task<bool> AddComment(AddCommentDto model)
  {
    bool added = false;
    if (model.CheckOutPreviousDay) added = await _unitOfWork.CheckInCheckOut.AddComment(DateTime.Today.AddDays(-1), model.Comment, model.UserCode);
    else added = await _unitOfWork.CheckInCheckOut.AddComment(DateTime.Today, model.Comment, model.UserCode);

    _unitOfWork.Commit();
    return added;
  }

  public async Task<PerformCheckInOrOutResponseDto> PerformCheckInOrOut(PerformCheckInOrOutDto infos)
  {
    var policies = infos.Policies;
    int numRec = infos.NumRec;
    string userCode = infos.UserCode;
    string commentFieldValue = infos.CommentFieldValue;
    bool isCheckIn = infos.IsCheckIn;
    bool checkOutPreviousDay = infos.CheckOutPreviousDay;
    int interval;

    // For 24 hour stores, we first need to check if the user forgot to check-out and if so, check them out
    if (policies.Hour24Store)
    {
      // 24 HOUR store - the user could have forgotten to check out, get the latest check-in
      DateTime? lastCheckInShiftDate = await _unitOfWork.CheckInCheckOut.GetLastCheckIn(userCode);
      if (lastCheckInShiftDate != null)
      {
        // Get details of the last check-in
        TimesheetDetailDto? lastCheckInDetails = await _unitOfWork.CheckInCheckOut.GetLastCheckInDetails(userCode, lastCheckInShiftDate ?? DateTime.Today);
        if (lastCheckInDetails != null && lastCheckInDetails.TimeOut == TimeSpan.Parse("00:00"))
        {
          await CalculateCommision(userCode, lastCheckInShiftDate ?? DateTime.Today);

          TimeSpan timeOut = DateTime.Now.TimeOfDay;
          double hours = CheckInCheckOutHelper.CalculateHours(lastCheckInDetails.TimeIn, DateTime.Now.TimeOfDay, policies.Roundtime);
          await _unitOfWork.CheckInCheckOut.ForceCheckOut(timeOut, hours, lastCheckInShiftDate ?? DateTime.Today, userCode, lastCheckInDetails.TimeIn, lastCheckInDetails.ReportId);

          _unitOfWork.Commit();
          return new PerformCheckInOrOutResponseDto
          {
            SuccessMessage = $"CheckOutSuccessMessage",
            HeaderText = "Check-out",
            Success = true
          };
        }
      }
    }

    // if we reached here that means the user didn't forget to check out for 24 hour store OR
    // forgot for not 24 hour store but we dont' care, we will let him check in
    if (numRec == 0)
    {
      // record for this date doesn't exist => new pay period
      TimeSheetPayPeriod? firstPayPeriod = await _unitOfWork.CheckInCheckOut.GetFirstTimeSheetPayPeriod();

      if (firstPayPeriod == null)
      {
        _unitOfWork.Commit();
        return new PerformCheckInOrOutResponseDto
        {
          WarningMessage = "EnterPayPeriodInformationMessage",
          HeaderText = "Initial Data Missing",
          Success = false
        };
      }
      else
      {
        if (firstPayPeriod.PayDate == null)
        {
          _unitOfWork.Commit();
          return new PerformCheckInOrOutResponseDto
          {
            WarningMessage = "EnterPayPeriodInformationMessage",
            HeaderText = "Initial Data Missing",
            Success = false
          };
        }
      }

      // Note pay period might be updated but pay period header is not created for this employee since he/she didn't log in yet.
      if (DateTime.Now > firstPayPeriod.PeriodEnding)
      {
        // new pay period starts now. Calculate new pay date and new range
        if (policies.PayPeriod == "Bi-Weekly")
        {
          // Assuming firstPayPeriod.PeriodStarting and firstPayPeriod.PeriodEnding are DateTime properties
          firstPayPeriod.PeriodStarting = firstPayPeriod.PeriodEnding.Value.AddDays(1);
          firstPayPeriod.PeriodEnding = firstPayPeriod.PeriodEnding.Value.AddDays(14);
          firstPayPeriod.PayDate = firstPayPeriod.PayDate.Value.AddDays(14);
        }
        else if (policies.PayPeriod == "Semi-monthly")
        {
          if (firstPayPeriod.PeriodStarting!.Value.Day == 1)
          {
            // pay period 1-15  ->  16-31
            DateTime lastDay = new DateTime(DateTime.Today.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1);

            firstPayPeriod.PeriodStarting = firstPayPeriod.PeriodStarting.Value.AddDays(15);
            firstPayPeriod.PeriodEnding = lastDay;
            firstPayPeriod.PayDate = lastDay;
          }
          else
          {
            int aDay = firstPayPeriod.PeriodEnding.Value.Day;

            if (aDay < 10)
            {
              firstPayPeriod.PeriodStarting = firstPayPeriod.PeriodEnding.Value.AddDays(1);
              firstPayPeriod.PeriodEnding = firstPayPeriod.PeriodStarting.Value.AddDays(14);

              await _unitOfWork.CheckInCheckOut.UpdateFirstPayPeriod(firstPayPeriod);
            }
            else
            {
              DateTime lastDay = firstPayPeriod.PeriodStarting.Value.AddMonths(1);
              firstPayPeriod.PeriodStarting = firstPayPeriod.PeriodEnding.Value.AddDays(1);
              firstPayPeriod.PeriodEnding = lastDay.AddDays(-1);
            }
            firstPayPeriod.PayDate = firstPayPeriod.PeriodEnding;
            await _unitOfWork.CheckInCheckOut.UpdateFirstPayPeriod(firstPayPeriod);
          }
        }
      }

      // We need to increase reportid For each pay period for each employee
      firstPayPeriod.InitialReportId += 1;
      await _unitOfWork.CheckInCheckOut.UpdateFirstPayPeriod(firstPayPeriod);

      // We need to create a new pay period header
      await _unitOfWork.CheckInCheckOut.AddNewTimeSheetHeader(firstPayPeriod, userCode);

      string province = await GetProvince();

      // Create timesheet reports for this pay period,but first check if there are any holidays
      List<TimeSheetHolidays> holidays = await GetTimeSheetHolidaysInRange(firstPayPeriod.PeriodStarting, firstPayPeriod.PeriodEnding, province, policies.UseProvHoliday);

      interval = (firstPayPeriod.PeriodEnding! - firstPayPeriod.PeriodStarting!).Value.Days;
      if (holidays.Count > 0)
      {
        // There is/are holiday in this pay period
        var holidayTasks = new List<Task<List<TimeSheetHolidays>>>();
        var insertTimeSheetDetailTasks = new List<Task<bool>>();

        for (int i = 0; i <= interval; ++i)
        {
          DateTime tempDate = firstPayPeriod.PeriodStarting!.Value.AddDays(i);
          holidayTasks.Add(GetTimeSheetHolidays(tempDate, province, policies.UseProvHoliday));
        }

        List<TimeSheetHolidays>[] holidayTaskResults = await Task.WhenAll(holidayTasks);

        for (int i = 0; i <= interval; ++i)
        {
          DateTime tempDate = firstPayPeriod.PeriodStarting!.Value.AddDays(i);

          insertTimeSheetDetailTasks.Add(_unitOfWork.CheckInCheckOut.InsertTimeSheetDetail(new TimesheetDetailDto
          {
            ReportId = firstPayPeriod.InitialReportId ?? 0,
            UserCode = userCode,
            ShiftDate = tempDate,
            DayOfWeek = tempDate.DayOfWeek.ToString(),
            TimeIn = TimeSpan.Zero,
            TimeOut = TimeSpan.Zero,
            Hours = 0.00,
            Shortages = 0.00,
            Comment = "",
            Stat = holidayTaskResults[i].Count > 0, // if holiday exist, this report is holiday, otherwise not
            Commission = 0.00,
            Training = false
          }));
        }

        await Task.WhenAll(insertTimeSheetDetailTasks);
      }
      else
      {
        // no holidays in this period, so just create timesheet details
        var insertTimeSheetDetailTasks = new List<Task<bool>>();
        for (int i = 0; i <= interval; ++i)
        {
          DateTime tempDate = firstPayPeriod.PeriodStarting!.Value.AddDays(i);

          insertTimeSheetDetailTasks.Add(_unitOfWork.CheckInCheckOut.InsertTimeSheetDetail(new TimesheetDetailDto
          {
            ReportId = firstPayPeriod.InitialReportId ?? 0,
            UserCode = userCode,
            ShiftDate = tempDate,
            DayOfWeek = tempDate.DayOfWeek.ToString(),
            TimeIn = TimeSpan.Zero,
            TimeOut = TimeSpan.Zero,
            Hours = 0.00,
            Shortages = 0.00,
            Comment = "",
            Stat = false,
            Commission = 0.00,
            Training = false
          }));
        }

        await Task.WhenAll(insertTimeSheetDetailTasks);
      }

      // Set the check-in time for this shift
      await _unitOfWork.CheckInCheckOut.UpdateTimeInOfTimeSheetDetail(DateTime.Now,
        firstPayPeriod.InitialReportId ?? 0, DateTime.Today, userCode);

      _unitOfWork.Commit();
      return new PerformCheckInOrOutResponseDto
      {
        SuccessMessage = $"CheckInSuccessMessage",
        HeaderText = "Check-in",
        Success = true
      };
    }
    else if (numRec > 0)
    {
      DateTime date = checkOutPreviousDay ? DateTime.Today.AddDays(-1) : DateTime.Today;
      TimesheetDetailDto lastEntryDetails = await _unitOfWork.CheckInCheckOut.GetLastTimeSheetDetailsForLastEntry(date, userCode);

      if (lastEntryDetails.TimeIn != TimeSpan.Parse("00:00") && lastEntryDetails.TimeOut != TimeSpan.Parse("00:00"))
      {
        // Add split shift to the comment
        lastEntryDetails.Comment = "Split Shift";
        await _unitOfWork.CheckInCheckOut.UpdateTimeSheetDetail(lastEntryDetails);

        // Another shift for this date
        await _unitOfWork.CheckInCheckOut.InsertTimeSheetDetail(new TimesheetDetailDto
        {
          ReportId = lastEntryDetails.ReportId,
          UserCode = userCode,
          ShiftDate = DateTime.Now,
          DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
          TimeIn = DateTime.Now.TimeOfDay, // check-in for sure, otherwise there would be a record already
          TimeOut = TimeSpan.Zero,
          Hours = 0.00,
          Shortages = 0.00,
          Comment = "Split Shift",
          Stat = lastEntryDetails.Stat,
          Commission = 0.00,
          Training = false,
        });

        // split shift so update header
        await _unitOfWork.CheckInCheckOut.UpdateTimeSheetHeader(lastEntryDetails.ReportId, userCode);

        _unitOfWork.Commit();
        return new PerformCheckInOrOutResponseDto
        {
          SuccessMessage = $"CheckInSuccessMessage",
          HeaderText = "Check-in",
          Success = true
        };
      }
      else
      {
        if (isCheckIn)
        {
          // Checking in
          TimeSpan newTimeIn = DateTime.Now.TimeOfDay;
          await _unitOfWork.CheckInCheckOut.UpdateTimeSheetDetail(lastEntryDetails, newTimeIn);

          _unitOfWork.Commit();
          return new PerformCheckInOrOutResponseDto
          {
            SuccessMessage = $"CheckInSuccessMessage",
            HeaderText = "Check-in",
            Success = true
          };
        }
        else
        {
          // Checking out
          if (checkOutPreviousDay)
          {
            // Update the record for the previous day shift so reset the rs recordset to previous day
            lastEntryDetails = await _unitOfWork.CheckInCheckOut.GetLastTimeSheetDetailsForLastEntry(date, userCode);
          }

          await CalculateCommision(userCode, date);

          lastEntryDetails.TimeOut = DateTime.Now.TimeOfDay;
          lastEntryDetails.Hours = CheckInCheckOutHelper.CalculateHours(lastEntryDetails.TimeIn, DateTime.Now.TimeOfDay, policies.Roundtime);

          await _unitOfWork.CheckInCheckOut.UpdateTimeSheetDetail(lastEntryDetails);

          if (lastEntryDetails.Hours > 10 && commentFieldValue == "")
          {
            _unitOfWork.Commit();
            return new PerformCheckInOrOutResponseDto
            {
              SuccessMessage = $"EnterExplanationMessage",
              HeaderText = "Explaination",
              Success = false,
              CommentNeeded = true
            };
          }

          _unitOfWork.Commit();
          return new PerformCheckInOrOutResponseDto
          {
            SuccessMessage = $"CheckOutSuccessMessage",
            HeaderText = "Check-out",
            Success = true,
          };
        }
      }
    }

    _unitOfWork.Commit();
    return new PerformCheckInOrOutResponseDto { Success = true };
  }

  public async Task CalculateCommision(string userCode, DateTime shiftDate)
  {
    var _unitOfWorkTrans = new UnitOfWork(_config, _config.CSCTransConnectionString);
    var _unitOfWorkMaster = new UnitOfWork(_config, _config.CSCMasterConnectionString);
    int empCommissionRecordCount = await _unitOfWorkMaster.CheckInCheckOut.GetEmpCommissionRecordCount();
    _unitOfWorkMaster.Commit();

    if (empCommissionRecordCount == 0) return;

    // Clear the timesheetdetails commission first.
    await _unitOfWorkTrans.CheckInCheckOut.UpdateTimesheetDetailsCommissions(0.00, shiftDate, userCode);

    // Get all the products for which we need to calculate commissions
    List<CommissionProduct> products = (await _unitOfWorkTrans.CheckInCheckOut.GetAllProducts(shiftDate, userCode)).ToList();

    var updateEmployeeCommissionTasks = new List<Task>();
    foreach (var product in products)
    {
      updateEmployeeCommissionTasks.Add(UpdateEmployeeCommissionForAllCases(userCode, shiftDate, product));
    }

    await Task.WhenAll(updateEmployeeCommissionTasks);
    _unitOfWorkTrans.Commit();
  }

  #region Private Methods
  private async Task<List<TimeSheetHolidays>> GetTimeSheetHolidays(DateTime shiftDate, string? province, bool policyUSE_PROV_HOLIDAY)
  {
    var holidays = new List<TimeSheetHolidays>();
    var _unitOfWorkMaster = new UnitOfWork(_config, _config.CSCMasterConnectionString);

    if (policyUSE_PROV_HOLIDAY)
    {
      holidays = (await _unitOfWorkMaster.CheckInCheckOut.GetTimeSheetHolidays(shiftDate, province)).ToList();
    }
    else holidays = (await _unitOfWorkMaster.CheckInCheckOut.GetTimeSheetHolidays(shiftDate)).ToList();

    _unitOfWorkMaster.Commit();
    return holidays;
  }

  private async Task<List<TimeSheetHolidays>> GetTimeSheetHolidaysInRange(DateTime? periodStarting, DateTime? periodEnding, string? province, bool policyUSE_PROV_HOLIDAY)
  {
    var holidays = new List<TimeSheetHolidays>();
    var _unitOfWorkMaster = new UnitOfWork(_config, _config.CSCMasterConnectionString);

    if (policyUSE_PROV_HOLIDAY)
    {
      holidays = (await _unitOfWorkMaster.CheckInCheckOut.GetTimeSheetHolidaysInRange(periodStarting, periodEnding, province)).ToList();
    }
    else holidays = (await _unitOfWorkMaster.CheckInCheckOut.GetTimeSheetHolidaysInRange(periodStarting, periodEnding)).ToList();

    _unitOfWorkMaster.Commit();
    return holidays;
  }

  private async Task<string> GetProvince()
  {
    var _unitOfWorkAdmin = new UnitOfWork(_config, _config.CSCAdminConnectionString);
    string province = await _unitOfWorkAdmin.CheckInCheckOut.GetProvince();
    _unitOfWorkAdmin.Commit();
    return province;
  }

  private async Task UpdateEmployeeCommissionForAllCases(string userCode, DateTime shiftDate, CommissionProduct product)
  {
    bool toStop = false;

    // Step 1. Get by stock code for employee first
    toStop = await UpdateEmployeeCommission(new EmpCommissionData
    {
      ByStockCode = true,
      EmployeeId = userCode,
      StockCode = product.StockCode,
      Dept = "ALL",
      SubDept = "ALL",
      SubDetail = "ALL",
    }, product, toStop, shiftDate, userCode, true);

    // Step 2. Get dept, subdept, subdetail for employee
    if (toStop == false)
    {
      toStop = await UpdateEmployeeCommission(new EmpCommissionData
      {
        ByStockCode = false,
        EmployeeId = userCode,
        StockCode = product.StockCode,
        Dept = product.Dept == "" ? "ALL" : product.Dept,
        SubDept = product.SubDept == "" ? "ALL" : product.SubDept,
        SubDetail = product.SubDetail == "" ? "ALL" : product.SubDetail,
      }, product, toStop, shiftDate, userCode);
    }

    // Step 3. Get dept, subdept for employee
    if (toStop == false)
    {
      toStop = await UpdateEmployeeCommission(new EmpCommissionData
      {
        EmployeeId = userCode,
        StockCode = product.StockCode,
        Dept = product.Dept == "" ? "ALL" : product.Dept,
        SubDept = product.SubDept == "" ? "ALL" : product.SubDept,
        SubDetail = "ALL",
      }, product, toStop, shiftDate, userCode);
    }

    // Step 4. Get dept for employee
    if (toStop == false)
    {
      toStop = await UpdateEmployeeCommission(new EmpCommissionData
      {
        EmployeeId = userCode,
        StockCode = product.StockCode,
        Dept = product.Dept,
        SubDept = "ALL",
        SubDetail = "ALL",
      }, product, toStop, shiftDate, userCode);
    }

    // Step 5. Get ALL for employee
    if (toStop == false)
    {
      toStop = await UpdateEmployeeCommission(new EmpCommissionData
      {
        EmployeeId = userCode,
        StockCode = product.StockCode,
        Dept = "ALL",
        SubDept = "ALL",
        SubDetail = "ALL",
      }, product, toStop, shiftDate, userCode);
    }

    // Now check for ALL employees
    // Step 1. Get by stock code for ALL employee
    if (toStop == false)
    {
      toStop = await UpdateEmployeeCommission(new EmpCommissionData
      {
        ByStockCode = true,
        EmployeeId = "ALL",
        StockCode = product.StockCode,
        Dept = "ALL",
        SubDept = "ALL",
        SubDetail = "ALL",
      }, product, toStop, shiftDate, userCode, true);
    }

    // Step 2. Get dept,subdept,subdetail for all
    if (toStop == false)
    {
      toStop = await UpdateEmployeeCommission(new EmpCommissionData
      {
        ByStockCode = false,
        EmployeeId = "ALL",
        StockCode = product.StockCode,
        Dept = "ALL",
        SubDept = "ALL",
        SubDetail = "ALL",
      }, product, toStop, shiftDate, userCode);
    }

    // Step 3. Get dept,subdept for employee
    if (toStop == false)
    {
      toStop = await UpdateEmployeeCommission(new EmpCommissionData
      {
        EmployeeId = "ALL",
        StockCode = product.StockCode,
        Dept = product.Dept,
        SubDept = product.SubDept == "" ? "ALL" : product.SubDept,
        SubDetail = "ALL",
      }, product, toStop, shiftDate, userCode);
    }

    // Step 4. Get dept for employee
    if (toStop == false)
    {
      toStop = await UpdateEmployeeCommission(new EmpCommissionData
      {
        EmployeeId = "ALL",
        StockCode = product.StockCode,
        Dept = product.Dept,
        SubDept = "ALL",
        SubDetail = "ALL",
      }, product, toStop, shiftDate, userCode);
    }

    // Step 5. Get ALL for employee
    if (toStop == false)
    {
      toStop = await UpdateEmployeeCommission(new EmpCommissionData
      {
        EmployeeId = "ALL",
        StockCode = product.StockCode,
        Dept = "ALL",
        SubDept = "ALL",
        SubDetail = "ALL",
      }, product, toStop, shiftDate, userCode);
    }
  }

  private async Task<bool> UpdateEmployeeCommission(EmpCommissionData getEmpCommData, CommissionProduct product, bool toStop, DateTime shiftDate, string userCode, bool notAll = false)
  {
    EmpCommissionData? empCommissionData = await _unitOfWork.CheckInCheckOut.GetEmployeeCommissionData(getEmpCommData, notAll);

    if (empCommissionData is null) return toStop;
    toStop = true;
    if (empCommissionData.Apply == false) return toStop;

    // 'we have found the record but apply=No so go to next record apply=Yes so commission is active
    double? commission = await _unitOfWork.CheckInCheckOut.GetCommissionFromTimeSheet(shiftDate, userCode);

    if (commission is null) return toStop;

    if (commission > 0)
    {
      // commission by %
      commission = commission + product.Amount * (commission / 100);
    }
    else
    {
      // commission by dollar
      commission = commission + empCommissionData.CommissionByDol * product.Quantity;
    }

    await _unitOfWork.CheckInCheckOut.UpdateTimesheetDetailsCommissions(commission ?? 0.00, shiftDate, userCode);

    return toStop;
  }

  private bool? HasCheckedOut(TimeSpan TimeIn, TimeSpan TimeOut)
  {
    // The user checked out, figure out if we r checking in  or checking-out
    if (TimeIn != TimeSpan.Parse("00:00") && TimeOut != TimeSpan.Parse("00:00"))
    {
      return true; // Check-In
    }
    else if (TimeIn != TimeSpan.Parse("00:00") && TimeOut == TimeSpan.Parse("00:00"))
    {
      return false; // Check-Out
    }
    return null;
  }
  #endregion
}
