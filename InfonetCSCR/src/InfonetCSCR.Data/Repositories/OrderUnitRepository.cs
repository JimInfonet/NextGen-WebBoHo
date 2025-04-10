namespace InfonetCSCR.Data.Repositories;

public class OrderUnitRepository : BaseRepository, IOrderUnitRepository
{
  public OrderUnitRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<OrderUnit>> GetAllOrderUnitsAsync()
  {
    const string sql = @"SELECT 
	                        [OrderID] OrderID,
	                        [Description] Description
                        FROM [CSCMaster].[dbo].[OrderUnit] 
                        ORDER BY [CSCMaster].[dbo].[OrderUnit].OrderID";

    IList<OrderUnit> orderUnits = (IList<OrderUnit>)await Connection.QueryAsync<OrderUnit>(sql, null, Transaction);
    return orderUnits;
  }

  public async Task<bool> AddOrderUnitAsync(OrderUnit orderUnit)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[OrderUnit]
            (
                [OrderID],
                [Description]
            )
            VALUES
            (
                @OrderID,
                @Description
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, orderUnit, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateOrderUnitAsync(OrderUnit orderUnit)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[OrderUnit]
                      SET
                          Description = @Description
                      WHERE
                          OrderID = @OrderID";
    int rowsAffected = await Connection.ExecuteAsync(sql, orderUnit, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteOrderUnitAsync(string orderUnitId)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[OrderUnit]
                        WHERE OrderID = @OrderID";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { OrderID = orderUnitId }, Transaction);
    return rowsAffected > 0;
  }
}
