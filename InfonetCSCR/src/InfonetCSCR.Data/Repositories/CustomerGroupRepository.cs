namespace InfonetCSCR.Data.Repositories;

public class CustomerGroupRepository : BaseRepository, ICustomerGroupRepository
{
  public CustomerGroupRepository(IDbTransaction transaction) : base(transaction) { }

  private string GetAllCustomerGroupsQueryString()
  {
    return $@"SELECT [GroupID], 
                  [GroupName], 
                  [DiscountType], 
                  [DiscountRate], 
                  [DiscountName], 
                  [LoyaltyFooter] 
                FROM 
                  [CSCMaster].[dbo].[ClientGroup]";
  }

  public async Task<IEnumerable<CustomerGroup>> GetAllCustomerGroupsByCashBonusAsync(bool cashBonus)
  {
    StringBuilder query = new StringBuilder();
    query.Append(GetAllCustomerGroupsQueryString());
    if (!cashBonus)
    {
      query.Append(" WHERE [DiscountType] <> 'B'");
    }
    query.Append(" ORDER BY [GroupID]");

    IEnumerable<CustomerGroup> customerGroups = await Connection.QueryAsync<CustomerGroup>(query.ToString(), null, Transaction);
    return customerGroups;
  }

  public async Task<bool> AddCustomerGroupAsync(CustomerGroup customerGroup)
  {
    string query = $@"INSERT INTO [CSCMaster].[dbo].[ClientGroup] (
                        [GroupID], 
                        [GroupName], 
                        [DiscountType], 
                        [DiscountRate], 
                        [DiscountName], 
                        [LoyaltyFooter] 
                    )
                    VALUES (
                        @GroupID,
                        @GroupName,
                        @DiscountType,
                        @DiscountRate,
                        @DiscountName,
                        @LoyaltyFooter
                    );";

    int rowsAffected = await Connection.ExecuteAsync(query, customerGroup, Transaction);
    return rowsAffected > 0;
  }
  
  public async Task<bool> UpdateCustomerGroupAsync(CustomerGroup customerGroup)
  {
    string query = $@"UPDATE [CSCMaster].[dbo].[ClientGroup]
                      SET 
                          [GroupName] = @GroupName, 
                          [DiscountType] = @DiscountType, 
                          [DiscountRate] = @DiscountRate, 
                          [DiscountName] = @DiscountName, 
                          [LoyaltyFooter] = @LoyaltyFooter
                      WHERE 
                          [GroupID] = @GroupID;";

    int rowsAffected = await Connection.ExecuteAsync(query, customerGroup, Transaction);
    return rowsAffected > 0;
  }
  
  public async Task<bool> DeleteCustomerGroupAsync(string groupID)
  {
    string query = $@"DELETE FROM [CSCMaster].[dbo].[ClientGroup]
                      WHERE 
                          [GroupID] = @GroupID;";

    int rowsAffected = await Connection.ExecuteAsync(query, new { GroupID = groupID }, Transaction);
    return rowsAffected > 0;
  }
  
  public async Task<CustomerGroup> GetCustomerGroupAsync(string groupID)
  {
    StringBuilder query = new StringBuilder();
    query.Append(GetAllCustomerGroupsQueryString());
    query.Append(" WHERE [GroupID] = @GroupID;");

    var customerGroup = await Connection.QueryFirstOrDefaultAsync<CustomerGroup>(query.ToString(), new { GroupID = groupID }, Transaction);
    return customerGroup;
  }
}
