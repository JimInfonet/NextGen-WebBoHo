namespace InfonetCSCR.Data.Repositories;

public class TimesheetReportsRepository : BaseRepository, ITimesheetReportsRepository
{
  public TimesheetReportsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<TimeSheetHeader>> GetReports(string status, string? userCode = null)
  {
    string sql = $@"SELECT B.u_name AS UserCode
                          ,A.PeriodStarting
                          ,A.PeriodEnding
                          ,RTRIM(LTRIM(A.Status)) AS Status
                          ,ReportId
                    FROM TimesheetHeader AS A 
                    INNER JOIN CSCAdmin.[dbo].[User] AS B ON A.UserCode = b.u_code
                    WHERE A.Status = '{status}' {(userCode != null ? $@" AND B.U_Code = '{userCode}' " : null)} 
                    ORDER BY B.U_Name ";

    var reports = await Connection.QueryAsync<TimeSheetHeader>(sql, null, Transaction);
    return reports;
  }

  public async Task<IEnumerable<TimeSheetHeader>> GetReportsBySignature(bool not)
  {
    string sql = $@"SELECT B.u_name AS UserCode
                          ,A.PeriodStarting
                          ,A.PeriodEnding
                          ,A.Status
                          ,ReportId
                    FROM TimesheetHeader AS A 
                    INNER JOIN CSCAdmin.[dbo].[User] AS B ON A.UserCode = b.u_code
                    WHERE {(not ? $@" NOT " : null)} (A.ManagerSign IS NULL OR A.EmployeeSign IS NULL) 
                    ORDER BY B.u_name, Status ";

    var reports = await Connection.QueryAsync<TimeSheetHeader>(sql, null, Transaction);
    return reports;
  }

  public async Task<TimesheetHeaderWithUserDetailsDto> GetTimesheetHeaderDetails(int reportId)
  {
    string sql = $@"SELECT ReportId
                          ,PayDate
                          ,PeriodStarting
                          ,PeriodEnding
                          ,UniformPurchases
                          ,Status
                          ,SplitShift
                          ,TransferFlag
                          ,ManagerSign
                          ,EmployeeSign
                          ,U_CODE AS UserCode
                          ,U_NAME AS UserName
                          ,U_ID AS UserId
                          ,Type
                          ,DOB AS DateOfBirth
                    FROM TimesheetHeader AS th
                    INNER JOIN [CSCAdmin].[dbo].[User] AS u
	                    ON th.UserCode = u.U_Code 
                    WHERE th.ReportId = {reportId} ";

    var timesheetHeader = await Connection.QueryFirstOrDefaultAsync<TimesheetHeaderWithUserDetailsDto>(sql, null, Transaction);
    return timesheetHeader;
  }

  public async Task<IEnumerable<ShortageDto>> GetShortages()
  {
    string sql = $@"SELECT DISTINCT st.ReportID, st.UserCode, st.Shiftdate, st.Shift, st.Shortage 
                    FROM TimesheetHeader AS th
                    INNER JOIN SHORTAGES AS st
	                    ON th.UserCode = st.UserCode 
                    WHERE (th.Status = 'Working') 
                      AND th.PeriodStarting <= st.ShiftDate 
                      AND th.PeriodEnding >= st.ShiftDate 
                      AND (st.ReportId IS NULL OR st.ReportId = '') ";

    var shortages = await Connection.QueryAsync<ShortageDto>(sql, null, Transaction);
    return shortages;
  }

  public async Task<IEnumerable<OutstandingStortageDto>> GetOutstandingShortages(string userCode, int reportId, DateTime periodStarting)
  {
    string sql = $@"SELECT ShiftDate, SUM(Shortage) AS Shortage 
                    FROM Shortages 
                    WHERE UserCode = '{userCode}'
                      AND ReportId = {reportId}
                      AND ShiftDate < '{periodStarting}'
                    GROUP BY ShiftDate 
                    ORDER BY ShiftDate ";

    var outstandingStortages = await Connection.QueryAsync<OutstandingStortageDto>(sql, null, Transaction);
    return outstandingStortages;
  }

  public async Task<int> UpdateReportIdInShortages(string userCode, int reportId, DateTime shiftDate)
  {
    string sql = $@"UPDATE Shortages 
                    SET ReportId = {reportId} 
                    WHERE UserCode = '{userCode}' 
                      AND ShiftDate = '{shiftDate}' ";

    var affectedRows = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return affectedRows;
  }

  public async Task<int> UpdateMissingReportIdInShortages(string userCode, int reportId, DateTime periodStarting)
  {
    string sql = $@"UPDATE Shortages 
                    SET ReportId = {reportId} 
                    WHERE UserCode = '{userCode}' 
                      AND ShiftDate < '{periodStarting}'
                      AND (ReportId IS NULL OR ReportId = '') ";

    var affectedRows = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return affectedRows;
  }

  public async Task<int> UpdateUniformPurchases(string userCode, double uniformPurchases, DateTime periodStarting, DateTime periodEnding)
  {
    string sql = $@"UPDATE TimesheetHeader 
                    SET UniformPurchases = {uniformPurchases} 
                    WHERE UserCode = '{userCode}' 
                      AND PeriodStarting = '{periodStarting}' 
                      AND PeriodEnding = '{periodEnding}'  ";

    var affectedRows = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return affectedRows;
  }

  public async Task<bool> MissingCheckOutsExist(string userCode, DateTime periodEnding)
  {
    string sql = $@"SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS MissingCheckOutExist
                    FROM TimesheetHeader AS th
                    INNER JOIN TimesheetDetail AS td
                        ON th.ReportID = td.ReportID 
                    WHERE td.TimeIN <> '00:00' 
                        AND td.TimeOut = '00:00' 
                        AND th.PeriodEnding = '{periodEnding}'
                        AND td.UserCode = '{userCode}' ";

    bool missingCheckOutsExist = await Connection.ExecuteScalarAsync<bool>(sql, null, Transaction);
    return missingCheckOutsExist;
  }

  public async Task<bool> MissingCommentExist(string userCode)
  {
    string sql = $@"SELECT CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS MissingCommentExist
                    FROM TimesheetHeader th 
                    INNER JOIN TimesheetDetail td 
                      ON th.ReportId = td.ReportId 
                    WHERE (td.Comment = '' OR td.Comment IS NULL) 
                      AND td.Hours >= 10 AND td.UserCode = '{userCode}' ";

    bool missingCommentExist = await Connection.ExecuteScalarAsync<bool>(sql, null, Transaction);
    return missingCommentExist;
  }

  public async Task<bool> PostPayPeriod(string userCode, DateTime periodStarting)
  {
    string sql = $@"UPDATE th
                    SET th.Status = 'Complete'
                    FROM TimesheetHeader AS th
                    INNER JOIN [CSCAdmin].[dbo].[User] u 
                        ON th.UserCode = u.[u_code] 
                    WHERE u.[u_code] = '{userCode}' 
                        AND th.PeriodStarting = '{periodStarting}' ";

    int affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows > 0;
  }

  public async Task<IEnumerable<TimeSheetHeader>> GetTimesheetHeadersForPosting(string userCode, DateTime periodStarting, DateTime periodEnding)
  {
    string sql = $@"SELECT [ReportID]
                          ,[UserCode]
                          ,[PayDate]
                          ,[PeriodStarting]
                          ,[PeriodEnding]
                          ,[UniformPurchases]
                          ,LTRIM(RTRIM([Status])) AS [Status]
                          ,[SplitShift]
                          ,[TransferFlag]
                          ,[ManagerSign]
                          ,[EmployeeSign]
                    FROM [CSCTrans].[dbo].[TimesheetHeader]
                    WHERE PeriodStarting >= '{periodStarting}' 
                      AND PeriodEnding   <= '{periodEnding}'
                      AND UserCode        = '{userCode}' ";

    var timesheetHeaders = await Connection.QueryAsync<TimeSheetHeader>(sql, null, Transaction);
    return timesheetHeaders;
  }

  public async Task<IEnumerable<ShortageDto>> GetShortagesForPosting(string userCode, DateTime periodStarting, int reportId)
  {
    string sql = $@"SELECT ShiftDate, Shortage 
                    FROM Shortages 
                    WHERE ReportId  = {reportId}
                      AND ShiftDate < '{periodStarting}' 
                      AND UserCode  = '{userCode}'
                    ORDER BY ShiftDate ";

    var shortages = await Connection.QueryAsync<ShortageDto>(sql, null, Transaction);
    return shortages;
  }

  public async Task<double> GetCashShortages(string userCode, int reportId)
  {
    string sql = $@"SELECT SUM(Shortages) AS TotalShort 
                    FROM TimesheetDetail 
                    WHERE ReportId = {reportId}
                      AND UserCode = '{userCode}'  ";

    double cashShortages = await Connection.ExecuteScalarAsync<double>(sql, null, Transaction);
    return cashShortages;
  }

  public async Task<double> GetPriorShortages(string userCode, int reportId, DateTime periodStarting)
  {
    string sql = $@"SELECT SUM(Shortages) AS TotalShort 
                    FROM TimesheetDetail 
                    WHERE ReportId  = {reportId}
                      AND UserCode  = '{userCode}'
                      AND ShiftDate = '{periodStarting}'  ";

    double priorShortages = await Connection.ExecuteScalarAsync<double>(sql, null, Transaction);
    return priorShortages;
  }

  public async Task<double> GetTotalHours(string userCode, int reportId, bool? stat = null)
  {
    string sql = $@"SELECT SUM(Hours) AS TotalHours
                    FROM TimesheetDetail 
                    WHERE ReportId = {reportId}
                      AND UserCode = '{userCode}' ";

    if (stat != null) sql += $" AND Stat =  {(stat == true ? "1" : "0")} ";

    double totalHours = await Connection.ExecuteScalarAsync<double>(sql, null, Transaction);
    return totalHours;
  }

  public async Task<IEnumerable<double>> GetOvertimes(string userCode, int reportId)
  {
    string sql = $@"SELECT SUM(Hours) AS TotalHours 
                    FROM TimesheetDetail 
                    WHERE ReportId = {reportId} AND UserCode = '{userCode}'
                    GROUP BY DATEADD(wk, DATEDIFF(wk, 6, ShiftDate), 6)   ";

    IEnumerable<double> overtimes = await Connection.QueryAsync<double>(sql, null, Transaction);
    return overtimes;
  }

  public async Task<double> GetTotalCommissions(string userCode, int reportId)
  {
    string sql = $@"SELECT SUM(Commission) AS TotalCommissions
                    FROM TimesheetDetail 
                    WHERE ReportId = {reportId} AND UserCode = '{userCode}'  ";

    double totalCommissions = await Connection.ExecuteScalarAsync<double>(sql, null, Transaction);
    return totalCommissions;
  }

  public async Task<IEnumerable<TimesheetDetailDto>> GetTimesheetDetailsBasedOnShiftType(string userCode, int reportId, bool splitShift)
  {
    string sql = "";

    if (splitShift)
    {
      // Combines all split shift records into one for every shiftdate
      sql = $@"SELECT ReportId
	                   ,UserCode
	                   ,ShiftDate
	                   ,DayOfWeek
	                   ,MIN(TimeIn)     AS TimeIn
	                   ,MAX(TimeOut)    AS TimeOut
	                   ,SUM(Hours)      AS Hours
	                   ,SUM(Shortages)  AS Shortages 
	                   ,Comment 
	                   ,Stat
	                   ,SUM(Commission) AS Commission
	                   ,Training
               FROM TimesheetDetail AS td
               WHERE UserCode  = '{userCode}'
                 AND ReportId  =  {reportId}
               GROUP BY ShiftDate, ReportId, UserCode, DayOfWeek, Comment, Stat, Training
               ORDER BY ShiftDate  ";
    }
    else
    {
      sql = @$"SELECT ReportId
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
               WHERE Usercode = '{userCode}' 
                 AND ReportId =  {reportId}
               ORDER BY ShiftDate, TimeIn  ; ";
    }

    var detailsInString = await Connection.QueryAsync<TimesheetDetail>(sql, null, Transaction);
    IEnumerable<TimesheetDetailDto> details = detailsInString.Select(CheckInCheckOutHelper.ConvertToTimeSheetDetailDto);
    return details;
  }

  public async Task<IEnumerable<TimeSheetMainReportDto>> GetTimeSheetReportDataAsync(long reportId, string userCode)
  {
    string sql = $@"SELECT 
                        th.ReportID, 
                        th.UserCode, 
                        th.PayDate, 
                        th.PeriodStarting, 
                        th.PeriodEnding, 
                        th.UniformPurchases, 
                        td.ShiftDate, 
                        td.DayOfWeek, 
                        td.TimeIn, 
                        td.TimeOut, 
                        td.Hours, 
                        td.Shortages, 
                        td.Comment, 
                        td.Stat, 
                        td.Commission, 
                        td.Training, 
                        u.U_NAME UserName, 
                        u.U_ID UserId, 
                        u.Type,
                        DATEFROMPARTS(YEAR(td.ShiftDate), MONTH(td.ShiftDate), 
                            CASE 
                                WHEN DAY(td.ShiftDate) <= 15 THEN 1
                                ELSE 16
                            END) AS SemiMonthlyGroupingColumn,
	                      DATEPART(WEEK, td.ShiftDate) AS BiWeeklyGroupingColumn
                    FROM TimesheetDetail AS td
                    INNER JOIN TimesheetHeader AS th ON td.ReportID = th.ReportID 
                                                    AND td.UserCode = th.UserCode
                    INNER JOIN cscadmin.dbo.[user] AS u ON u.u_code = th.UserCode 
                                                      AND u.u_code = td.UserCode 
                    WHERE th.ReportID = @ReportId AND th.UserCode = @UserCode";

    var parameters = new
    {
      ReportId = reportId,
      UserCode = userCode,
    };

    IEnumerable<TimeSheetMainReportDto> data = await Connection.QueryAsync<TimeSheetMainReportDto>(sql, parameters, Transaction);
    return data;
  }

  public async Task<IEnumerable<ShortageDto>> GetShortagesReportData(long reportId, string userCode, DateTime shiftDate)
  {
    string sql = $@"SELECT
                        SHORTAGES.ReportId,
                        SHORTAGES.UserCode,
                        SHORTAGES.ShiftDate,
                        SHORTAGES.Shortage
                    FROM CSCTrans.dbo.SHORTAGES SHORTAGES
                    WHERE
                        SHORTAGES.ReportID = @ReportId
                        AND SHORTAGES.UserCode = @UserCode
                        AND SHORTAGES.Shiftdate < @ShiftDate";

    var parameters = new
    {
      ReportId = reportId,
      UserCode = userCode,
      ShiftDate = shiftDate,
    };

    IEnumerable<ShortageDto> data = await Connection.QueryAsync<ShortageDto>(sql, parameters, Transaction);
    return data;
  }

  public async Task<IEnumerable<TimesheetDetail>> GetTimesheetDetailReportData(long reportId, string userCode)
  {
    string sql = @$"SELECT
                        TimesheetDetail.ReportID,
                        TimesheetDetail.UserCode,
                        TimesheetDetail.ShiftDate,
                        TimesheetDetail.Hours
                    FROM
                        CSCTrans.dbo.TimesheetDetail TimesheetDetail
                    WHERE
                        TimesheetDetail.Stat = 1
                        AND ReportID = @ReportId
                        AND UserCode = @UserCode";

    var parameters = new
    {
      ReportId = reportId,
      UserCode = userCode
    };

    IEnumerable<TimesheetDetail> data = await Connection.QueryAsync<TimesheetDetail>(sql, parameters, Transaction);
    return data;
  }
}
