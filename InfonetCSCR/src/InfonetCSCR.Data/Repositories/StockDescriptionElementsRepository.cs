namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class StockDescriptionElementsRepository : BaseRepository, IStockDescriptionElementsRepository
{
  public StockDescriptionElementsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<Warehouse>> GetWarehouseAsync()
  {
    const string sql = @"SELECT Store_ID AS StoreID,
                        Store_Desc AS StoreDescription,
                        isWarehouse AS IsWarehouse
                        FROM Store where isWarehouse = 1
                        ORDER BY Store_ID";

    IEnumerable<Warehouse> resultWarehouse  = await Connection.QueryAsync<Warehouse>(sql, null, Transaction);
    return resultWarehouse;
  }

  public async Task<IEnumerable<Grade>> GetGradeAsync(string stockCode)
  {
    string sql = $@"SELECT * FROM [CSCPump].[dbo].[Grade] AS G WHERE G.Stock_Code='{stockCode}'";

    IEnumerable<Grade> resultGrade = await Connection.QueryAsync<Grade>(sql, null, Transaction);
    return resultGrade;
  }
}
