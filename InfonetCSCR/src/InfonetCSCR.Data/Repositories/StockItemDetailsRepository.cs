namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class StockItemDetailsRepository : BaseRepository, IStockItemDetailsRepository
{
  private readonly IApplicationConfig _config;

  public StockItemDetailsRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
  {
    _config = config;
  }

  public async Task<int> GetTotalActiveCharges()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT COUNT(*) FROM [CSCMaster].[dbo].Assoc");

    return await Connection.QuerySingleOrDefaultAsync<int>(query.ToString(), null, Transaction);
  }

  public async Task<int> GetTotalActiveTaxes()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT COUNT(*) FROM [CSCMaster].[dbo].TaxMast WHERE TaxMast.Tax_Active = 1");

    return await Connection.QuerySingleOrDefaultAsync<int>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<ChargesAndDeposits>> GetChargesAndDeposits(string? stockCode, string? storeID)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT	DISTINCT LTRIM(RTRIM(A.STOCK_CODE)) AS StockCode, A.As_Code AS Charge, B.As_Desc AS Description, B.As_Price AS Price
		                  FROM	  [CSCMaster].[dbo].Stock_Ac AS A
				                      INNER JOIN [CSCMaster].[dbo].Assoc AS B ON A.As_Code = B.As_Code
		                  WHERE	  A.STOCK_CODE = '{stockCode}'");

    if (!_config.IsBackOffice)
    {
      query.Append(@$" AND A.StoreID = '{storeID}' ");
    }
    query.Append(" Order By A.As_Code");

    return await Connection.QueryAsync<ChargesAndDeposits>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<Assoc>> GetAssocNotInStockCodeAc(string? stockCode, string storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT DISTINCT AS_Code AS Code, As_Desc AS Description, As_Price AS Price
		                  FROM	[CSCMaster].[dbo].Assoc
		                  WHERE	AS_Code NOT IN 
		                  ( 
		                    SELECT A.As_Code FROM	[CSCMaster].[dbo].Stock_Ac AS A
		                    INNER JOIN [CSCMaster].[dbo].Assoc AS B ON A.As_Code = B.As_Code
		                    WHERE	A.STOCK_CODE = '{stockCode}'
		                  ");

    if (!_config.IsBackOffice)
    {
      query.Append(@$" AND A.StoreID IN ({storeIDs}) ");
    }
    query.Append(" )");

    return await Connection.QueryAsync<Assoc>(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteCharge(string? stockCode, string? chargeCode, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE FROM [CSCMaster].[dbo].Stock_Ac WHERE Stock_Code = '{stockCode}' AND As_Code = '{chargeCode}'");

    if (!_config.IsBackOffice)
    {
      query.Append(@$" AND StoreID IN ({storeIDs}) ");
    }

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<StockAc>> GetCharges(string? stockCode, string? chargeCode, string? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT LTRIM(RTRIM(STOCK_CODE)) AS StockCode,
                             AS_CODE AS Code
                             {(!_config.IsBackOffice ? ", [StoreID] AS StoreID" : "")} 
                      FROM	[CSCMaster].[dbo].[STOCK_AC]
                      WHERE Stock_Code = '{stockCode}' AND As_Code = '{chargeCode}'
                            {(!_config.IsBackOffice ? @$" AND StoreID IN ({storeIDs}) " : "")}");

    return await Connection.QueryAsync<StockAc>(query.ToString(), null, Transaction);
  }

  public async Task<int> InsertCharges(string? stockCode, string? chargeCode, string? selectionValue, List<string>? storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"INSERT INTO Stock_Ac ( Stock_Code, As_Code {(!_config.IsBackOffice ? @$", StoreID" : "")})
                      VALUES ");
    if (_config.IsBackOffice)
    {
      query.Append($@"('{stockCode}', '{chargeCode}')");
    }
    else
    {
      int len = storeIDs.Count;
      for(int i = 0; i < len; i++)
      {
        query.Append($@"('{stockCode}', '{chargeCode}', '{storeIDs[i]}'){((i < storeIDs.Count - 1) ? "," : "")}");
      }
    }

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<Tax>> GetTaxes(string? stockCode, string? storeID)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT DISTINCT LTRIM(RTRIM(T.Stock_Code)) AS StockCode, T.Tax_Name AS TaxName, T.Tax_Code AS TaxCode, R.Tax_Rate AS Rate, R.Tax_Incl AS Included
                      FROM	[CSCMaster].[dbo].StockTax AS T 
		                        INNER JOIN [CSCMaster].[dbo].TaxRate AS R ON T.Tax_Name = R.Tax_Name AND T.Tax_Code = R.Tax_Code 
		                        INNER JOIN [CSCMaster].[dbo].TaxMast AS M ON R.Tax_Name = M.Tax_Name
                      WHERE T.Stock_Code = '{stockCode}' AND 
                            M.Tax_Active = 1 ");

    if(!_config.IsBackOffice)
    {
      query.Append(@$" AND T.StoreID = '{storeID}' ");
    }
    query.Append("ORDER BY T.Tax_Name");

    return await Connection.QueryAsync<Tax>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<TaxNameAndCode>> GetTaxesAndCode(string? stockCode, string storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT	DISTINCT M.Tax_Name AS TaxName, R.Tax_Code AS TaxCode
                      FROM	[CSCMaster].[dbo].TaxMast AS M
		                        INNER JOIN [CSCMaster].[dbo].TaxRate AS R ON M.Tax_Name = R.Tax_Name 
                      WHERE	M.Tax_Active = 1 AND 
		                        M.Tax_Name NOT IN 
		                        (
			                        SELECT  ST.Tax_Name 
			                        FROM    [CSCMaster].[dbo].StockTax AS ST  
					                            INNER JOIN [CSCMaster].[dbo].TaxRate AS TR ON ST.Tax_Name = TR.Tax_Name AND ST.Tax_Code = TR.Tax_Code
					                            INNER JOIN TaxMast AS TM ON TR.Tax_Name = TM.Tax_Name 
			                        WHERE   ST.Stock_Code = '{stockCode}' AND 
					                            TM.Tax_Active = 1 
		                  ");

    if (!_config.IsBackOffice)
    {
      query.Append(@$" AND ST.StoreID IN ({storeIDs}) ");
    }
    query.Append(" )");

    return await Connection.QueryAsync<TaxNameAndCode>(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteTaxes(string? stockCode, string? taxName, string? taxCode, string? storeIDs, bool deleteForInsert = true)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE FROM [CSCMaster].[dbo].[StockTax] 
                      WHERE Stock_Code = '{stockCode}' AND 
                            Tax_Name = '{taxName}'");
    if(!deleteForInsert)
    {
      query.Append(@$" AND Tax_Code = '{taxCode}'");
    }

    if(!_config.IsBackOffice)
    {
      query.Append(@$"AND StoreID IN ({storeIDs})");
    }

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> InsertTaxes(string? stockCode, string? taxName, string? taxCode, List<string> storeIDs)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"INSERT INTO [CSCMaster].[dbo].[StockTax] (Stock_Code, Tax_Name, Tax_Code {(!_config.IsBackOffice ? @$", StoreID" : "")})
                      VALUES ");

    if(_config.IsBackOffice)
    {
      query.Append(@$"('{stockCode}', '{taxName}', '{taxCode}')");
    }
    else
    {
      int len = storeIDs.Count;
      for (int i = 0; i < len; i++)
      {
        query.Append($@"('{stockCode}', '{taxName}', '{taxCode}', {storeIDs[i]}){((i < storeIDs.Count - 1) ? "," : "")}");
      }
    }

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<AvailabilityByDay>> GetAvailabilityByDay(string stockCode)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT LTRIM(RTRIM(Stock_Code)) AS StockCode, DayOfWeek
                      FROM	[CSCMaster].[dbo].StockByDay
                      WHERE   Stock_Code = '{stockCode}' 
                      ORDER BY DayOfWeek");

    return await Connection.QueryAsync<AvailabilityByDay>(query.ToString(), null, Transaction);
  }

  public async Task<int> DeleteStockAvailabilityByDay(string stockCode)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE FROM	[CSCMaster].[dbo].StockByDay
                      WHERE   Stock_Code = '{stockCode}'");

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }

  public async Task<int> InsertStockAvailabilityByDay(List<AvailabilityByDay> days)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"INSERT INTO [CSCMaster].[dbo].StockByDay (Stock_Code, DayOfWeek)
                      VALUES ");
    int len = days.Count();
    if (len == 0) return 0;
    for(int i = 0; i < len; i++)
    {
      query.Append(@$" ('{days[i].StockCode}', {days[i].DayOfWeek}) ");
      if(i < len - 1)
      {
        query.Append(@$",");
      }
    }

    return await Connection.ExecuteAsync(query.ToString(), null, Transaction);
  }
}
