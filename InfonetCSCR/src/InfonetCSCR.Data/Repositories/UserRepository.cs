namespace InfonetCSCR.Data.Repositories;

//CSCAdmin Connection
public class UserRepository : BaseRepository, IUserRepository
{
  public UserRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<UserDetails> GetUserDetailsAsync(string code)
  {
    UserDetails user = await Connection.QuerySingleOrDefaultAsync<UserDetails>
        ("Sproc_GetUser", new { code }, Transaction, commandType: CommandType.StoredProcedure);
    return user;
  }

  public async Task<UserGroup> GetUserGroupDetailsAsync(string code)
  {
    UserGroup userGroupDetails = await Connection.QuerySingleOrDefaultAsync<UserGroup>
        ("Sproc_GetUserGroupDetails", new { code }, Transaction, commandType: CommandType.StoredProcedure);
    return userGroupDetails;
  }

  public async Task<int> GetUserId(string userCode)
  {
    string sql = $@"SELECT U_ID FROM [USER] WHERE UPPER(U_CODE) = '{userCode}' ";

    int userId = await Connection.ExecuteScalarAsync<int>(sql, null, Transaction);
    return userId;
  }

  public async Task<User?> GetByCodeAsync(string code)
  {
    User? user = await Connection.QuerySingleOrDefaultAsync<User>
        ("Sproc_GetUser", new { CODE = code }, Transaction, commandType: CommandType.StoredProcedure);
    return user;
  }

  public async Task<bool> GetUserAccessRightAsync(string permission, User user)
  {
    var hasAccessRight = await Connection.QuerySingleOrDefaultAsync<bool>
        ("Sproc_HasAccessRight", new { P_NAME = permission, USERNAME = user.Code }, Transaction, commandType: CommandType.StoredProcedure);
    return hasAccessRight;
  }

  public async Task<string> GetUserNameByCodeAsync(string code)
  {
    string sql = $"SELECT U_NAME AS NAME FROM [USER] WHERE U_CODE = '{code}'";

    string userName = await Connection.QuerySingleOrDefaultAsync<string>(sql, null, Transaction, commandType: CommandType.Text);
    return userName;
  }

  public async Task<string> GetUserTypeByCodeAsync(string userCode)
  {
    string sql = $"SELECT Type FROM [USER] WHERE U_CODE = '{userCode}'";

    string userType = await Connection.QuerySingleOrDefaultAsync<string>(sql, null, Transaction);
    return userType;
  }

  public async Task<IEnumerable<UserDetails>> GetAllUserDetailsAsync(bool timeSheetsAndHaveUserIdPolicy)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT [U_Code] AS [Code], 
                           [U_Name] AS [Name]");
    if (timeSheetsAndHaveUserIdPolicy)
    {
      query.Append($@",
                      [U_ID] AS [ID], 
                      [Type] AS [Type]");
    }
    query.Append($@"
                    FROM [CSCAdmin].[dbo].[USER] 
                    ORDER BY [User].[U_Code];");

    IEnumerable<UserDetails> userDetails = await Connection.QueryAsync<UserDetails>(query.ToString(), null, Transaction);
    return userDetails;
  }

  public async Task<bool> AddUserDetailsAsync(UserDetails userDetails)
  {
    string query = $@"INSERT INTO [CSCAdmin].[dbo].[USER] (
                        [U_CODE], 
                        [U_NAME], 
                        [EPW], 
                        [U_ID], 
                        [Type], 
                        [DOB]
                    )
                    VALUES (
                        @Code, 
                        @Name, 
                        @Password, 
                        @Id, 
                        @Type, 
                        @DateOfBirth
                    );";

    int rowsAffected = await Connection.ExecuteAsync(query, userDetails, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateUserDetailsAsync(UserDetails userDetails)
  {
    string query = $@"UPDATE [CSCAdmin].[dbo].[USER]
                        SET
                            [U_NAME] = @Name,
                            [Type] = @Type,
                            [DOB] = @DateOfBirth
                        WHERE
                            [U_CODE] = @Code;";

    int rowsAffected = await Connection.ExecuteAsync(query, userDetails, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteUserDetailsAsync(string code)
  {
    string query = $@"DELETE FROM [CSCAdmin].[dbo].[USER]
                        WHERE
                            [U_CODE] = @Code;";

    int rowsAffected = await Connection.ExecuteAsync(query, new { Code = code }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<UserDetails>?> GetAllUsersByGroupAsync(string groupCode)
  {
    string query = $@"SELECT [USER].[U_CODE] AS [Code], 
                             [USER].[U_NAME] AS [Name]  
			                    FROM [CSCAdmin].[dbo].[USER] INNER JOIN [CSCAdmin].[dbo].[UIG]
                             ON [USER].[U_CODE] = [UIG].[USER] 
			                       WHERE [UIG].[UGROUP] = '{groupCode}' 
			                       ORDER BY [User].[U_CODE];";

    IEnumerable<UserDetails>? groupMembers = await Connection.QueryAsync<UserDetails>(query, new { groupCode = groupCode }, Transaction);
    return groupMembers;
  }

  public async Task<IEnumerable<string>?> GetUserCodesAsync(int securityLevel)
  {
    string query = $@"SELECT [U_CODE] AS [CODE] 
                      FROM [CSCAdmin].[dbo].[USER] WHERE [USER].[U_CODE] NOT IN (
                          SELECT [UIG].[USER]
                          FROM [CSCAdmin].[dbo].[UIG]
                          INNER JOIN [CSCAdmin].[dbo].[UGROUP] ON [UIG].[UGROUP] = [UGROUP].[UGROUP]
                          WHERE [UGROUP].[SecurityLevel] > {securityLevel}
                      )
                      ORDER BY [USER].[U_CODE];";

    IEnumerable<string>? userCodes = await Connection.QueryAsync<string>(query, new { securityLevel = securityLevel }, Transaction);
    return userCodes;
  }

  public async Task<bool> ChangeUserPasswordAsync(UserDetails userDetails)
  {
    string query = $@"UPDATE [CSCAdmin].[dbo].[USER]
                        SET
                            [EPW] = @Password
                        WHERE
                            [U_CODE] = @Code;";

    int rowsAffected = await Connection.ExecuteAsync(query, userDetails, Transaction);
    return rowsAffected > 0;
  }

  public async Task<int> GetMaxUserIDAsync()
  {
    const string query = "SELECT MAX(U_ID) FROM [CSCAdmin].[dbo].[USER]";
    return await Connection.ExecuteScalarAsync<int>(query, null, Transaction);
  }
}
