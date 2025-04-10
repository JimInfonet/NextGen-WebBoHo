namespace InfonetCSCR.Data.Repositories;

public class CheckInCheckOutRepository : BaseRepository, ICheckInCheckOutRepository
{
  private readonly IApplicationConfig _config;
  public CheckInCheckOutRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
  {
    _config = config;
  }

  public async Task<IEnumerable<CheckInCheckOutUserDto>> GetCurrentUsersAsync()
  {
    string sql = @$"SELECT u.u_name AS UserName
	                       ,td.ShiftDate
                         ,CASE WHEN td.ShiftDate <> '{DateTime.Today}' THEN 0 ELSE 1 END AS CheckedOut
                   FROM TimesheetDetail AS td
                   INNER JOIN [CSCAdmin].[dbo].[User] AS u 
	                     ON td.UserCode = u.[u_code] 
                   WHERE td.TimeIn   <> '00:00'
                       AND td.TimeOut = '00:00' 
                   ORDER BY CheckedOut, u.u_name, td.ShiftDate DESC; ";

    var users = await Connection.QueryAsync<CheckInCheckOutUserDto>(sql, null, Transaction);
    return users;
  }

  public async Task<IEnumerable<TimesheetDetailDto>> GetTimeSheetDetails(string userCode, DateTime shiftDate)
  {
    string sql = @$"SELECT ReportId
                          ,UserCode
                          ,ShiftDate
                          ,DayOfWeek
                          ,TimeIn
                          ,TimeOut
                          ,Hours
                          ,Shortages
                          ,Comment
                          ,Stat
                          ,Commission
                          ,Training
                   FROM TimesheetDetail 
                   WHERE Usercode = '{userCode}' AND ShiftDate = '{shiftDate}' ; ";

    var detailsInString = await Connection.QueryAsync<TimesheetDetail>(sql, null, Transaction);
    IEnumerable<TimesheetDetailDto> details = detailsInString.Select(CheckInCheckOutHelper.ConvertToTimeSheetDetailDto);
    return details;
  }

  public async Task<IEnumerable<TimesheetDetailDto>> GetTimeSheetDetailsByReportId(string userCode, int reportId)
  {
    string sql = @$"SELECT ReportId
                          ,UserCode
                          ,ShiftDate
                          ,DayOfWeek
                          ,TimeIn
                          ,TimeOut
                          ,Hours
                          ,Shortages
                          ,Comment
                          ,Stat
                          ,Commission
                          ,Training
                   FROM TimesheetDetail 
                   WHERE Usercode = '{userCode}' AND ReportId = '{reportId}'
                   ORDER BY ShiftDate, TimeIn  ; ";

    var detailsInString = await Connection.QueryAsync<TimesheetDetail>(sql, null, Transaction);
    IEnumerable<TimesheetDetailDto> details = detailsInString.Select(CheckInCheckOutHelper.ConvertToTimeSheetDetailDto);
    return details;
  }

  public async Task<IEnumerable<TimesheetDetailDto>> GetTimeSheetDetailsForLastDay(string userCode)
  {
    string sql = @$"SELECT ReportId
                          ,UserCode
                          ,ShiftDate
                          ,DayOfWeek
                          ,TimeIn
                          ,TimeOut
                          ,Hours
                          ,Shortages
                          ,Comment
                          ,Stat
                          ,Commission
                          ,Training
                   FROM TimesheetDetail 
                   WHERE Usercode = '{userCode}' AND ShiftDate = '{DateTime.Today.AddDays(-1)}' ; ";

    var detailsInString = await Connection.QueryAsync<TimesheetDetail>(sql, null, Transaction);
    IEnumerable<TimesheetDetailDto> details = detailsInString.Select(CheckInCheckOutHelper.ConvertToTimeSheetDetailDto);
    return details;
  }

  public async Task<IEnumerable<TimesheetDetailDto>> GetTimeSheetDetailsOrderByTimeIn(string userCode)
  {
    string sql = @$"SELECT ReportId
                          ,UserCode
                          ,ShiftDate
                          ,DayOfWeek
                          ,TimeIn
                          ,TimeOut
                          ,Hours
                          ,Shortages
                          ,Comment
                          ,Stat
                          ,Commission
                          ,Training
                   FROM TimesheetDetail 
                   WHERE Usercode = '{userCode}' AND ShiftDate = '{DateTime.Today}'
                   ORDER BY TimeIn ASC ; ";

    var detailsInString = await Connection.QueryAsync<TimesheetDetail>(sql, null, Transaction);
    IEnumerable<TimesheetDetailDto> details = detailsInString.Select(CheckInCheckOutHelper.ConvertToTimeSheetDetailDto);
    return details;
  }

  public async Task<TimesheetDetailDto> GetLastTimeSheetDetailsForLastEntry(DateTime shiftDate, string userCode)
  {
    string sql = @$"SELECT TOP 1
                           ReportId
                          ,UserCode
                          ,ShiftDate
                          ,DayOfWeek
                          ,TimeIn
                          ,TimeOut
                          ,Hours
                          ,Shortages
                          ,Comment
                          ,Stat
                          ,Commission
                          ,Training
                   FROM TimesheetDetail 
                   WHERE Usercode = '{userCode}' AND ShiftDate = '{shiftDate}'
                   ORDER BY TimeIn DESC ; ";

    var detailsInString = await Connection.QueryFirstOrDefaultAsync<TimesheetDetail>(sql, null, Transaction);
    var details = CheckInCheckOutHelper.ConvertToTimeSheetDetailDto(detailsInString);
    return details;
  }

  public async Task<DateTime?> GetLastCheckIn(string userCode)
  {
    string sql = @$"SELECT MAX(ShiftDate) AS ShiftDate 
                    FROM TimesheetDetail
                    WHERE TimeIn <> '00:00' AND UserCode = '{userCode}'; ";

    var lastCheckIn = await Connection.QueryFirstOrDefaultAsync<DateTime?>(sql, null, Transaction);
    return lastCheckIn;
  }

  public async Task<TimesheetDetailDto?> GetLastCheckInDetails(string userCode, DateTime shiftDate)
  {
    string sql = @$"SELECT TOP 1 *
                    FROM TimesheetDetail
                    WHERE ShiftDate = '{shiftDate}' AND UserCode = '{userCode}'
                    ORDER BY ReportID DESC, UserCode DESC, ShiftDate DESC, TimeIn DESC ; ";

    var lastCheckInDetailsInString = await Connection.QueryFirstOrDefaultAsync<TimesheetDetail?>(sql, null, Transaction);
    var lastCheckInDetails = CheckInCheckOutHelper.ConvertToTimeSheetDetailDto(lastCheckInDetailsInString);

    return lastCheckInDetails;
  }

  public async Task<IEnumerable<DateTime>> GetTimeOutsByShiftDate(string userCode, DateTime shiftDate)
  {
    string sql = @$"SELECT TimeOut 
                    FROM  TimesheetDetail 
                    WHERE ShiftDate = '{shiftDate}' AND UserCode = '{userCode}' 
                    ORDER BY ReportID, UserCode, ShiftDate, TimeIn ; ";

    var timeOuts = await Connection.QueryAsync<DateTime>(sql, null, Transaction);
    return timeOuts;
  }

  public async Task<bool> AddComment(DateTime shiftDate, string comment, string userCode)
  {
    string sql = @$"UPDATE TOP(1) TimesheetDetail
                    SET Comment = '{comment}'
                    WHERE ShiftDate = '{shiftDate}' AND UserCode = '{userCode}' ";

    var affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows == 1;
  }

  public async Task<int> GetEmpCommissionRecordCount()
  {
    string sql = $@"SELECT COUNT(1) AS RecordCount FROM EmpCommission ";

    var allEmpCommissions = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return allEmpCommissions;
  }

  public async Task<EmpCommissionData?> GetEmployeeCommissionData(EmpCommissionData model, bool notAll = false)
  {
    var sql = new StringBuilder();
    sql.Append($@"SELECT  * 
                    FROM EmpCommissionData 
                    WHERE  EmployeeId  = '{model.EmployeeId}' 
                       AND StockCode   = '{model.StockCode}' ");

    string sign = notAll ? " <> " : " = ";

    if (model.Dept is not null) sql.Append($"AND Dept {sign} '{model.Dept}' ");
    if (model.SubDept is not null) sql.Append($"AND SubDept {sign} '{model.SubDept}' ");
    if (model.SubDetail is not null) sql.Append($"AND SubDetail {sign} '{model.SubDetail}' ");
    if (model.ByStockCode is not null) sql.Append($"AND ByStockCode = {(model.ByStockCode == true ? "1" : "0" )} ");

    var empCommission = await Connection.QueryFirstOrDefaultAsync<EmpCommissionData?>(sql.ToString(), null, Transaction);
    return empCommission;
  }

  public async Task UpdateTimesheetDetailsCommissions(double commission, DateTime shiftDate, string userCode)
  {
    string sql = $@"UPDATE TimesheetDetail SET Commission = {commission}
                    WHERE  UserCode = '{userCode}' AND ShiftDate ='{shiftDate}'";

    var empCommissions = await Connection.ExecuteAsync(sql, null, Transaction);
  }

  public async Task<IEnumerable<CommissionProduct>> GetAllProducts(DateTime shiftDate, string userCode)
  {
    string sql = $@"SELECT  LTRIM(RTRIM(SaleLine.Stock_Code)) AS StockCode
                           ,SaleHead.ShiftDate
                           ,SaleLine.Quantity
                           ,SaleLine.Amount
                           ,Saleline.Dept
                           ,saleline.Sub_dept AS SubDept
                           ,saleline.Sub_detail AS SubDetail
                    FROM   [CSCTrans].[dbo].[SaleHead] AS SaleHead 
                    INNER JOIN [CSCTrans].[dbo].[SaleLine] AS SaleLine 
                          ON  SaleHead.Till = SaleLine.Till_Num 
                          AND SaleHead.Sale_No = SaleLine.Sale_No  
                    WHERE (SaleHead.[USER] = '{userCode}') 
                          AND (SaleHead.ShiftDate = '{shiftDate}') 
                    
                    UNION ALL

                    SELECT  LTRIM(RTRIM(SaleLine.Stock_Code)) AS StockCode
                           ,SaleHead.ShiftDate
                           ,SaleLine.Quantity
                           ,SaleLine.Amount
                           ,Saleline.Dept
                           ,saleline.Sub_dept AS SubDept
                           ,saleline.Sub_detail AS SubDetail
                    FROM   [CSCTills].[dbo].[SaleHead] AS SaleHead 
                    INNER JOIN [CSCTills].[dbo].[SaleLine] AS SaleLine 
                          ON  SaleHead.Till = SaleLine.Till_Num 
                          AND SaleHead.Sale_No = SaleLine.Sale_No  
                    WHERE (SaleHead.[USER] = '{userCode}') 
                          AND (SaleHead.ShiftDate = '{shiftDate}') ";

    var products = await Connection.QueryAsync<CommissionProduct>(sql, null, Transaction);
    return products;
  }

  public async Task<double?> GetCommissionFromTimeSheet(DateTime shiftDate, string userCode)
  {
    string sql = @$"SELECT Commission FROM TimesheetDetail
                    WHERE  UserCode = '{userCode}' AND ShiftDate = '{shiftDate}' ";

    var commission = await Connection.QueryFirstOrDefaultAsync<double?>(sql, null, Transaction);
    return commission;
  }

  public async Task<int> ForceCheckOut(TimeSpan timeOut, double hours, DateTime shiftDate, string userCode, TimeSpan timeIn, int reportId)
  {
    string sql = $@"
        UPDATE TimesheetDetail
        SET TimeOut = '{timeOut.ToString("hh\\:mm")}', Hours = '{hours}'
        WHERE ShiftDate = '{shiftDate}' 
          AND UserCode = '{userCode}' 
          AND TimeIn = '{timeIn.ToString("hh\\:mm")}' 
          AND ReportId = {reportId} ";

    int affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows;
  }

  public async Task<TimeSheetPayPeriod?> GetFirstTimeSheetPayPeriod()
  {
    string sql = "SELECT TOP 1 * FROM TimeSheetPayPeriod ";
    var firstPayPeriod = await Connection.QueryFirstOrDefaultAsync<TimeSheetPayPeriod>(sql, null, Transaction);
    return firstPayPeriod;
  }

  public async Task<bool> UpdateFirstPayPeriod(TimeSheetPayPeriod model)
  {
    string sql = $@"UPDATE TOP (1) TimeSheetPayPeriod
                    SET PeriodStarting  = '{model.PeriodStarting}'
                       ,PeriodEnding    = '{model.PeriodEnding}'
                       ,PayDate         = '{model.PayDate}' 
                       ,InitialReportId = {model.InitialReportId}
                       ,InitialEmpId    = {model.InitialEmpId} ";

    int affactedRows = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return affactedRows == 1;
  }

  public async Task<bool> AddNewTimeSheetHeader(TimeSheetPayPeriod model, string userCode)
  {
    string sql = $@"Insert INTO TimeSheetHeader (
			                          [ReportID]
			                         ,[UserCode]
			                         ,[PayDate]
			                         ,[PeriodStarting]
			                         ,[PeriodEnding]
			                         ,[UniformPurchases]
			                         ,[Status]
			                         ,[SplitShift]) 
                    VALUES (
			                          {model.InitialReportId}
			                         ,'{userCode}'
			                         ,'{model.PayDate}'
			                         ,'{model.PeriodStarting}'
			                         ,'{model.PeriodEnding}'
			                         ,0.00
			                         ,'Working'
			                         ,0) ";

    int affactedRows = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return affactedRows == 1;
  }

  public async Task<string> GetProvince()
  {
    string sql = "SELECT TOP 1 Province FROM [CSCAdmin].[dbo].[Setup_C] ";
    string province = await Connection.QueryFirstOrDefaultAsync<string>(sql, null, Transaction);
    return province is null ? "" : province;
  }

  public async Task<IEnumerable<TimeSheetHolidays>> GetTimeSheetHolidays(DateTime shiftDate, string? province = null)
  {
    string sql = $@"SELECT * 
                    FROM TimeSheetHolidays 
                    WHERE ShiftDate = '{shiftDate}' ";

    if (province is not null) sql += $@"AND Province = '{province}' ";

    var holidays = await Connection.QueryAsync<TimeSheetHolidays>(sql, null, Transaction);
    return holidays;
  }

  public async Task<IEnumerable<TimeSheetHolidays>> GetTimeSheetHolidaysInRange(DateTime? periodStarting, DateTime? periodEnding, string? province = null)
  {
    string sql = $@"SELECT * 
                    FROM TimeSheetHolidays 
                    WHERE ShiftDate >= '{periodStarting}' AND ShiftDate <= '{periodEnding}' ";

    if (province is not null) sql += $@"AND Province = '{province}' ";

    var holidays = await Connection.QueryAsync<TimeSheetHolidays>(sql, null, Transaction);
    return holidays;
  }

  public async Task<bool> InsertTimeSheetDetail(TimesheetDetailDto model)
  {
    string sql = $@"Insert INTO TimeSheetDetail (
			                          [ReportID]
		                           ,[UserCode]
		                           ,[ShiftDate]
		                           ,[DayOfWeek]
		                           ,[TimeIn]
		                           ,[TimeOut]
		                           ,[Hours]
		                           ,[Shortages]
		                           ,[Comment]
		                           ,[Stat]
		                           ,[Commission]
		                           ,[Training]) 
                    VALUES (   {model.ReportId}
		                          ,'{model.UserCode}'
		                          ,'{model.ShiftDate.ToString("MM/dd/yyyy")}'
		                          ,'{model.DayOfWeek}'
		                          ,'{model.TimeIn.ToString("hh\\:mm")}'
		                          ,'{model.TimeOut.ToString("hh\\:mm")}'
		                          ,{model.Hours}
		                          ,{model.Shortages}
		                          ,'{(model.Comment == null ? "" : $"{model.Comment}")}'
		                          ,{(model.Stat ? '1' : '0')}
		                          ,{(model.Commission == null ? 0.00 : $"{model.Commission}")}
		                          ,{(model.Training ? '1' : '0')} )";

    int affactedRows = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return affactedRows == 1;
  }

  public async Task<int> UpdateTimeInOfTimeSheetDetail(DateTime timeIn, int reportId, DateTime shiftDate, string userCode)
  {
    string sql = $@"UPDATE TimeSheetDetail 
                    SET TimeIn = '{timeIn.ToString("HH\\:mm")}'
                    WHERE ReportID    = '{reportId}' 
                        AND ShiftDate = '{shiftDate}'
                        AND UserCode  = '{userCode}' ";

    int affactedRows = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return affactedRows;
  }

  public async Task<bool> UpdateTimeSheetDetail(TimesheetDetailDto model, TimeSpan? newTimeIn = null)
  {
    string sql = $@"UPDATE TimeSheetDetail
                    SET    [ReportID]   = {model.ReportId}
		                      ,[UserCode]   = '{model.UserCode}'
		                      ,[ShiftDate]  = '{model.ShiftDate}'
		                      ,[DayOfWeek]  = '{model.DayOfWeek}'
		                      ,[TimeIn]     = '{(newTimeIn != null ? newTimeIn.Value.ToString("hh\\:mm") : model.TimeIn.ToString("hh\\:mm"))}'
		                      ,[TimeOut]    = '{model.TimeOut.ToString("hh\\:mm")}'
		                      ,[Hours]      = {model.Hours}
		                      ,[Shortages]  = {model.Shortages}
		                      ,[Comment]    = '{(model.Comment == null ? "" : $"{model.Comment}")}'
		                      ,[Stat]       = {(model.Stat ? "1" : "0")}
		                      ,[Commission] = {(model.Commission == null ? 0.00 : $"{model.Commission}")}
		                      ,[Training]   = {(model.Training ? "1" : "0")} 
                    WHERE  ShiftDate = '{model.ShiftDate}' 
                        AND UserCode = '{model.UserCode}' 
                        AND ReportId = '{model.ReportId}' 
                        AND TimeIn   = '{model.TimeIn.ToString("hh\\:mm")}' ";

    int affactedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affactedRows == 1;
  }

  public async Task<int> UpdateTimeSheetHeader(int reportId, string userCode)
  {
    string sql = $@"UPDATE TimeSheetHeader 
                    SET SplitShift = 1 
                    WHERE ReportId = {reportId} AND UserCode = '{userCode}' ";

    int affactedRows = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return affactedRows;
  }
}
