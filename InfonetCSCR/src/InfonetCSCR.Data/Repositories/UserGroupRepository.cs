namespace InfonetCSCR.Data.Repositories;

//CSCAdmin Connection
public class UserGroupRepository : BaseRepository, IUserGroupRepository
{
  public UserGroupRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<string>> GetGroupCodesBySecurityLevelAsync(int securityLevel)
  {
    string query = $@"SELECT [UGroup].[UGroup] 
                      FROM [CSCAdmin].[dbo].[UGROUP] 
                      WHERE [UGroup].[SecurityLevel] <= {securityLevel} 
                      ORDER BY [UGroup].[UGroup];";

    IEnumerable<string> groupCodes = await Connection.QueryAsync<string>(query, new { securityLevel = securityLevel }, Transaction);
    return groupCodes;
  }
  
  public async Task<int> GetSecurityLevelForDeleteUIGAsync(string group)
  {
    string query = $@"SELECT [SecurityLevel] FROM [CSCAdmin].[dbo].[UGROUP] 
                      WHERE [UGROUP].[UGROUP] = '{group}';";

    int securityLevel = await Connection.QueryFirstOrDefaultAsync<int>(query, new { group = group }, Transaction);
    return securityLevel;
  }

  public async Task<IEnumerable<UserGroup>?> GetAllUserGroups()
  {
    string query = $@"SELECT [UGROUP] AS [Code],
                             [UG_NAME] AS [Name],
	                           [SecurityLevel] AS [SecurityLevel]
                          FROM [CSCAdmin].[dbo].[UGROUP]
                          ORDER BY [UGroup].[UGroup];";

    IEnumerable<UserGroup>? userGroups = await Connection.QueryAsync<UserGroup>(query, null, Transaction);
    return userGroups;
  }

  public async Task<UserGroup?> GetUserGroupAsync(string groupCode)
  {
    string query = $@"SELECT [UGROUP] AS [Code],
	                           [UG_NAME] AS [Name],
	                           [SecurityLevel] AS [SecurityLevel]
	                        FROM [CSCAdmin].[dbo].[UGROUP]
	                        WHERE [UGROUP].[UGROUP] = '{groupCode}';";

    UserGroup? userGroup = await Connection.QueryFirstOrDefaultAsync<UserGroup?>(query, new { groupCode = groupCode }, Transaction);
    return userGroup;
  }

  public async Task<bool> AddUserGroupAsync(UserGroup userGroup)
  {
    string query = $@"INSERT INTO [CSCAdmin].[dbo].[UGROUP] (
                        [UGROUP], 
                        [UG_NAME], 
                        [SecurityLevel]
                    )
                    VALUES (
                        @Code, 
                        @Name, 
                        @SecurityLevel
                    );";

    int rowsAffected = await Connection.ExecuteAsync(query, userGroup, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> RemoveUserGroupAsync(string groupCode)
  {
    int rowsAffected = await Connection.ExecuteAsync("Sproc_RemoveUserGroup",
      new { Code = groupCode }, Transaction, commandType: CommandType.StoredProcedure);

    return rowsAffected > 0;
  }
}
