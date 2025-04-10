namespace InfonetCSCR.Data.Repositories;

//CSCTrans Connection
public class TimesheetPayPeriodRepository : BaseRepository, ITimesheetPayPeriodRepository
{
  public TimesheetPayPeriodRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<int> GetInitialEmployeeIdAsync()
  {
    string query = $@"SELECT InitialEmpID FROM [CSCTrans].[dbo].[TimesheetPayPeriod];";

    return await Connection.QueryFirstOrDefaultAsync<int>(query, null, Transaction);
  }

  public async Task<bool> IncrementInitialEmployeeId(int updatedInitialEmployeeId)
  {
    string query = $@"UPDATE [CSCTrans].[dbo].[TimesheetPayPeriod] SET InitialEmpID = {updatedInitialEmployeeId};";

    var rowAffected = await Connection.QueryFirstOrDefaultAsync<int>(query, null, Transaction);
    return rowAffected > 0;
  }
}
