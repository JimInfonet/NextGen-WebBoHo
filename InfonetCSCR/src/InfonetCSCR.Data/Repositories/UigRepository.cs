namespace InfonetCSCR.Data.Repositories;

//CSCAdmin Connection
public class UigRepository : BaseRepository, IUigRepository
{
  public UigRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<GroupInfoDto>> GetGroupInfoAsync(string userCode)
  {
    string query = $@"SELECT 
                        [UIG].[UGroup] as [Group], 
                        [UGroup].[UG_Name] as [GroupName]
                    FROM [CSCAdmin].[dbo].[UIG] INNER JOIN [UGroup] 
                    ON [UIG].[UGroup] = [UGroup].[UGroup] 
                    WHERE [Uig].[User] = '{userCode}';";

    IEnumerable<GroupInfoDto> groupInfo = await Connection.QueryAsync<GroupInfoDto>(query, new { userCode = userCode }, Transaction);
    return groupInfo;
  }

  public async Task<bool> AddUigRecordAsync(Uig uig)
  {
    string query = $@"INSERT INTO [CSCAdmin].[dbo].[UIG] (
                        [USER], 
                        [UGROUP]
                    )
                    VALUES (
                        @User,
                        @Group
                    );";

    int rowAffected = await Connection.ExecuteAsync(query, uig, Transaction);
    return rowAffected > 0;
  }

  public async Task<bool> DeleteUigRecordAsync(Uig uig, bool isGroupIncluded = true)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"DELETE FROM [CSCAdmin].[dbo].[UIG] WHERE [UIG].[User] = '{uig.User}'");
    query.Append(isGroupIncluded ? $@" AND [UIG].[UGroup] = '{uig.Group}';" : ";");

    int rowAffected = await Connection.ExecuteAsync(query.ToString(), null, Transaction);
    return rowAffected > 0;
  }
}
