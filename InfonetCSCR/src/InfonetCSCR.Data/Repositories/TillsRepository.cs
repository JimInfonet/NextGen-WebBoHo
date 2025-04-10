namespace InfonetCSCR.Data.Repositories;

public class TillsRepository : BaseRepository, ITillsRepository
{
  public TillsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<int> GetMaxTillAsync(int tillNo)
  {
    string query = "";

    if (tillNo == 99)
    {
      query = $@"Till_Num>={91} AND";
    }

    string sql = $@"SELECT 
                        MAX(Till_Num) AS MTill 
                      FROM 
                        [CSCMaster].[dbo].[Tills] 
                      WHERE 
                        {query} Till_Num<=@TillNo";

    int mTill = await Connection.QueryFirstOrDefaultAsync<int>(sql, new { TillNo = tillNo }, Transaction);
    return mTill;
  }

  public async Task<string> GetPumpFeaturesAsync()
  {
    string query = $@"
                SELECT TOP (1) [Pump_Features]
                FROM [CSCAdmin].[dbo].[Setup_C]";

    string pumpFeatures = await Connection.QueryFirstOrDefaultAsync<string>(query, null, Transaction);
    return pumpFeatures;
  }

  public async Task<IList<Till>> GetTillsAsync(int maxTill, bool isCashBonusPolicy)
  {
    string sqlExtraColumnOnPolicy = "";

    if (isCashBonusPolicy)
    {
      sqlExtraColumnOnPolicy = "CashBonusFloat, CashBonus,";
    }

    string sql = @$"SELECT 
                      [Till_Num] AS TillNum,
                      Active,
                      Process AS Processing,
                      Float,
                      Cash, 
                      {sqlExtraColumnOnPolicy}
                      Date_Open AS OpenDate,
                      Time_Open AS OpenTime,
                      ShiftNumber AS Shift,
                      ShiftDate AS ShiftDate,
                      UserLoggedOn AS UserLoggedOn,
                      posid AS POSID
                  FROM 
                      [CSCMaster].[dbo].[Tills]
                  WHERE 
                      Till_Num <= {maxTill}
                  ORDER BY 
                      Till_Num;
                  ";

    IList<Till> tills = (IList<Till>)await Connection.QueryAsync<Till>(sql, null, Transaction);
    return tills;
  }

  public async Task<int> ClearTillAsync(string[] tables, int tillNum)
  {
    var sql = new StringBuilder();

    foreach (string table in tables)
    {
      string tableName = table.ToUpper();
      string colName;

      if (tableName == "SALEHEAD" || tableName == "CASHDRAW" || tableName == "SUSHEAD")
      {
        colName = "Till";
      }
      else if (tableName == "PURCHASEITEM")
      {
        colName = "Till_No";
      }
      else
      {
        colName = "Till_Num";
      }

      string query = $@"DELETE FROM [CSCTills].[dbo].[{tableName}] WHERE {colName}={tillNum}";
      sql.AppendLine(query);
    }

    int isExecuted = await Connection.ExecuteAsync(sql.ToString(), null, Transaction);
    return isExecuted;
  }

  public async Task<bool> AddTillAsync(Till till)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[Tills]
            (
                [Till_Num],
                [Active],
                [Process], 
                [Float], 
                [Cash], 
                [CashBonusFloat], 
                [CashBonus], 
                [Date_Open], 
                [Time_Open], 
                [ShiftNumber], 
                [ShiftDate], 
                [UserLoggedOn], 
                [posid]
            )
            VALUES
            (
                @TillNum,
                @Active,
                @Processing, 
                @Float, 
                @Cash, 
                @CashBonusFloat, 
                @CashBonus, 
                @OpenDate, 
                @OpenTime, 
                @Shift, 
                @ShiftDate, 
                @UserLoggedOn, 
                @POSID
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, till, Transaction);
    return rowsAffected > 0;
  }

  public async Task<int> GetTillsCountAsync(int tillNo)
  {
    string query = $@"SELECT COUNT(*) FROM [CSCTills].[dbo].[SaleHead] WHERE Till=@TillNo";

    int tillCount = await Connection.QueryFirstOrDefaultAsync<int>(query, new { TillNo = tillNo }, Transaction);
    return tillCount;
  }

  public async Task<bool> DeleteTillAsync(int tillNo)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[Tills]
                        WHERE Till_Num = @TillNo";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { TillNo = tillNo }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateTillByFlipActiveOrFlipProcessAsync(Till till)
  {
    string sql = @$"
            UPDATE [CSCMaster].[dbo].[Tills]
            SET
              Active = @Active, 
              Process = @Processing, 
              posid = @POSID
            WHERE
              TILL_NUM = @TillNum";

    int rowsAffected = await Connection.ExecuteAsync(sql, till, Transaction);
    return rowsAffected > 0;
  }
}
