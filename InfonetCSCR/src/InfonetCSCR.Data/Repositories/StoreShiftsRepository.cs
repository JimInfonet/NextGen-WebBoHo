namespace InfonetCSCR.Data.Repositories;

public class StoreShiftsRepository : BaseRepository, IStoreShiftsRepository
{
  public StoreShiftsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<StoreShift>> GetStoreShiftsAsync()
  {
    string sql = @$"SELECT 
                            ShiftNumber,
                            StartTime,
                            Active AS ActiveStatus, 
                            CurrentDay
                        FROM 
                            [CSCMaster].[dbo].[ShiftStore]
                        ORDER BY ShiftNumber";

    IList<StoreShift> storeShifts = (IList<StoreShift>)await Connection.QueryAsync<StoreShift>(sql, null, Transaction);
    return storeShifts;
  }

  public async Task<bool> AddStoreShiftAsync(StoreShiftDto storeShift)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[ShiftStore]
            (
                [ShiftNumber],
                [StartTime],
                [Active], 
                [CurrentDay]
            )
            VALUES
            (
                @ShiftNumber,
                @StartTime,
                @ActiveStatus, 
                @CurrentDay
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, storeShift, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteStoreShiftAsync(int shiftNumber)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[ShiftStore]
                        WHERE ShiftNumber = @ShiftNumber";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { ShiftNumber = shiftNumber }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateStoreShiftsAsync(IList<StoreShiftDto> storeShifts, bool isUpdateAll)
  {
    string sql;

    if (isUpdateAll)
    {
      // Delete the table itmes
      string query = $@"DELETE FROM [CSCMaster].[dbo].[ShiftStore]";
      await Connection.ExecuteAsync(query, null, Transaction);

      // Insert into the table
      sql = @$"
            INSERT INTO [CSCMaster].[dbo].[ShiftStore]
            (
                [ShiftNumber],
                [StartTime],
                [Active], 
                [CurrentDay]
            )
            VALUES
            (
                @ShiftNumber,
                @StartTime,
                @ActiveStatus, 
                @CurrentDay
            )";
    }
    else
    {
      sql = @$"UPDATE [CSCMaster].[dbo].[ShiftStore]
                   SET
                      StartTime = @StartTime,
                      Active = @ActiveStatus 
                   WHERE
                      ShiftNumber = @ShiftNumber";
    }

    int rowsAffected = await Connection.ExecuteAsync(sql, storeShifts, Transaction);
    return rowsAffected > 0;
  }
}
