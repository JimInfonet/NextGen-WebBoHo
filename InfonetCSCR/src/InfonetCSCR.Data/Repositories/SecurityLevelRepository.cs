namespace InfonetCSCR.Data.Repositories;

//CSCAdmin Connection
public class SecurityLevelRepository : BaseRepository, ISecurityLevelRepository
{
  public SecurityLevelRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<SecurityLevel>?> GetSecurityLevels(int loggedInUserSecurityLevel)
  {
    string query = $@"SELECT [Groupid] AS [GroupId],
                             [GroupName] AS [GroupName]
                          FROM [CSCAdmin].[dbo].[UserGroup] 
                          WHERE [UserGroup].[Groupid] <= {loggedInUserSecurityLevel};";

    IEnumerable<SecurityLevel>? securityLevels = await Connection.QueryAsync<SecurityLevel>(query,
      new { loggedInUserSecurityLevel = loggedInUserSecurityLevel }, Transaction);

    return securityLevels;
  }
}
