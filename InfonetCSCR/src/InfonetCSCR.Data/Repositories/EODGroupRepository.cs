namespace InfonetCSCR.Data.Repositories;

public class EODGroupRepository : BaseRepository, IEODGroupRepository
{
  public EODGroupRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> CheckGroupIdExistsAsync(string eodGroupId)
  {
    StringBuilder query = new();
    query.Append($@"SELECT [EodGroupID] FROM [CSCMaster].[dbo].[EodGroups] WHERE EodGroupID = @EODGroupId;");

    var parameters = new { EODGroupId = eodGroupId };
    string eodGroupIdData = await Connection.QuerySingleOrDefaultAsync<string>(query.ToString(), parameters, Transaction);
    return (eodGroupIdData != null);
  }

  public async Task<bool> AddEODGroupAsync(EodGroup eodGroup)
  {
    const string sql = @"INSERT INTO [CSCMaster].[dbo].[EodGroups] (
                          [EodGroupID],
                          [EodGroupName]
                        ) VALUES (
                          @EodGroupID,
                          @EodGroupName
                        )";

    var parameters = new { EodGroupID = eodGroup.EodGroupID, EodGroupName = eodGroup.EodGroupName };
    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<EodGroup>> GetGroupsAsync()
  {
    StringBuilder query = new();
    query.Append($@"SELECT 
	                      EodGroupID,
	                      EodGroupName
                      FROM [CSCMaster].[dbo].[EodGroups];");

    IEnumerable<EodGroup> eodGroups = await Connection.QueryAsync<EodGroup>(query.ToString(), null, Transaction);
    return eodGroups;
  }

  public async Task<bool> DeleteEodGroupByEodGroupId(string eodGroupId)
  {
    const string sql = @"DELETE FROM [CSCMaster].[dbo].[EodGroups] WHERE EodGroupID = @EodGroupId";

    var parameters = new { EodGroupId = eodGroupId };
    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return rowsAffected > 0;
  }
}
