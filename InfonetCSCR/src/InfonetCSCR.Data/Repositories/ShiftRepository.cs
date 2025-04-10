namespace InfonetCSCR.Data.Repositories;

public class ShiftRepository : BaseRepository, IShiftRepository
{
  public ShiftRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<ShiftRecHeaderDto>> GetShiftIDandDateByShiftDateAsync(string shiftDate)
  {
    string query = @"SELECT
                            DISTINCT ID AS ShiftID, ShiftDate
                        FROM
                            [CSCTrans].[dbo].[ShiftRecHeader]
                        WHERE
                            ShiftDate = @ShiftDate
                            ORDER BY ID DESC";
    IEnumerable<ShiftRecHeaderDto> shiftRecHeaders = await Connection.QueryAsync<ShiftRecHeaderDto>(query, new { ShiftDate = shiftDate }, Transaction);
    return shiftRecHeaders;
  }

  public async Task<string> GetShiftNumberByShiftAndDateAsync(string shiftDate, int shift)
  {
    string query = @"SELECT
                            count(*) as CNT
                        FROM
                            [CSCTrans].[dbo].[ShiftRecHeader]
                        WHERE
                            ShiftDate = @ShiftDate
                            AND Shift = @Shift
                            AND Closed = 1";
    int countClosed = await Connection.QueryFirstOrDefaultAsync<int>(query, new { ShiftDate = shiftDate, Shift = shift }, Transaction);
    return shift.ToString() + (countClosed > 0 ? " - Closed" : "");
  }
  public async Task<IEnumerable<int>> GetShiftNumbersAsync()
  {
    string query = @"SELECT
                            ShiftNumber
                        FROM
                            [CSCMaster].[dbo].[ShiftStore]
                        WHERE
                            active = 1
                            ORDER BY ShiftNumber";
    IEnumerable<int> shiftNumbers = await Connection.QueryAsync<int>(query, null, Transaction);
    return shiftNumbers;
  }
  public async Task<bool> UpdateRepostedByShiftNumberAndShiftIDAsync(int? shiftID, int? shiftNumber)
  {
    string query = $@"UPDATE [CSCTrans].[dbo].[ShiftRecHeader]
                        SET
                            Reposted = 1
                        WHERE
                            ID = @ShiftID
                            AND Shift = @ShiftNumber";
    var parameters = new { ShiftID = shiftID, ShiftNumber = shiftNumber };
    int rowsAffected = await Connection.ExecuteAsync(query, parameters, Transaction);
    return rowsAffected > 0;
  }
}