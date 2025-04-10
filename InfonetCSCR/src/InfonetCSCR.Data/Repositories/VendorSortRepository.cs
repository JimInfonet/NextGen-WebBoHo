namespace InfonetCSCR.Data.Repositories;

public class VendorSortRepository : BaseRepository, IVendorSortRepository
{
  public VendorSortRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<VendorSort>> GetVendorSortsAsync(string vendorId)
  {
    const string sql = @"SELECT
	                          VendorID,
	                          OrderNo,
	                          SortItem,
	                          Ascending
                          FROM [CSCMaster].[dbo].[VendorSort]
                          WHERE VendorID = @VendorId
                          Order by OrderNo";

    var parameter = new { VendorId = vendorId };
    IEnumerable<VendorSort> vendorSorts = await Connection.QueryAsync<VendorSort>(sql, parameter, Transaction);
    return vendorSorts;
  }

  public async Task<IEnumerable<VendorSort>> GetAllVendorSortsByVendorListAsync()
  {
    const string sql = @"SELECT
	                          VendorID,
	                          OrderNo,
	                          SortItem,
	                          Ascending
                          FROM [CSCMaster].[dbo].[VendorSort]
                          Order by OrderNo";

    IEnumerable<VendorSort> vendorSorts = await Connection.QueryAsync<VendorSort>(sql, null, Transaction);
    return vendorSorts;
  }

  public async Task<VendorSortSetup> GetVendorSortSetupsAsync(string sortItem)
  {
    const string sql = @"SELECT
	                          SortItem,
	                          LinkTable,
	                          LinkField
                          FROM [CSCMaster].[dbo].[VendorSortSetup]
                          WHERE SortItem = @SortItem";

    var parameters = new { SortItem = sortItem };
    VendorSortSetup vendorSortSetup = await Connection.QueryFirstOrDefaultAsync<VendorSortSetup>(sql, parameters, Transaction);
    return vendorSortSetup;
  }

  public async Task<IEnumerable<VendorSortSetup>> GetVendorSortSetupsBySortItemsAsync(IEnumerable<string> sortItems)
  {
    const string sql = @"SELECT
	                          SortItem,
	                          LinkTable,
	                          LinkField
                          FROM [CSCMaster].[dbo].[VendorSortSetup]
                          WHERE SortItem in @SortItems";

    var parameters = new { SortItems = sortItems };
    IEnumerable<VendorSortSetup> vendorSortSetups = await Connection.QueryAsync<VendorSortSetup>(sql, parameters, Transaction);
    return vendorSortSetups;
  }

  public async Task<IEnumerable<VendorSortSetup>> GetAllVendorSortSetupsBySortItemsAsync()
  {
    const string sql = @"SELECT
	                          SortItem,
	                          LinkTable,
	                          LinkField
                          FROM [CSCMaster].[dbo].[VendorSortSetup]";

    IEnumerable<VendorSortSetup> vendorSortSetups = await Connection.QueryAsync<VendorSortSetup>(sql, null, Transaction);
    return vendorSortSetups;
  }

  public async Task<int> GetNextSortOrderAsync(string vendorId)
  {
    string sql = $@"
          SELECT IsNull(max(Orderno), 0) as MaxOrder from [CSCMaster].[dbo].[VendorSort] Where VendorID = @VendorId";
    return (await Connection.QueryFirstAsync<int>(sql, new { VendorId = vendorId }, Transaction)) + 1;
  }

  public async Task DeleteVendorSortAsync(IEnumerable<string> sortItems, string vendorId)
  {
    string sql = $@"Delete from [CSCMaster].[dbo].[VendorSort] 
                      Where VendorId = @VendorId And SortItem in @SortItems";
    await Connection.ExecuteAsync(sql, new {
      VendorId = vendorId,
      SortItems = sortItems,
    }, Transaction);
  }

  public async Task AddVendorSortAsync(IEnumerable<VendorSort> sortItems)
  {
    string sql = $@"Insert into [CSCMaster].[dbo].[VendorSort] 
                    (VendorId, OrderNo, SortItem, Ascending) Values
                    (@VendorId, @OrderNo, @SortItem, @Ascending)";
    await Connection.ExecuteAsync(sql, sortItems, Transaction);
  }
}
