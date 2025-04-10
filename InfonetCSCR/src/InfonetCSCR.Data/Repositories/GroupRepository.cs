namespace InfonetCSCR.Data.Repositories;

public class GroupRepository : BaseRepository, IGroupRepository
{
  public GroupRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<FamilyGroupJoinFamily>> GetAllGroupsAsync()
  {
    const string sql = @"SELECT a.GroupID, a.GroupName, a.FamilyID, b.description
                          FROM FamilyGroup a
                          INNER JOIN FAMILY b ON a.Familyid = b.familyid
                          ORDER BY a.GroupID, a.FamilyID";

    IList<FamilyGroupJoinFamily> groups = (IList<FamilyGroupJoinFamily>)await Connection.QueryAsync<FamilyGroupJoinFamily>(sql, null, Transaction);
    return groups;
  }

  public async Task<bool> AddGroupAsync(IList<FamilyGroup> groups)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[FamilyGroup]
            (
                [GroupID],
                [FamilyID], 
                [GroupName]
            )
            VALUES
            (
                @GroupID,
                @FamilyID, 
                @GroupName
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, groups, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteGroupAsync(FamilyGroupJoinFamily group)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[FamilyGroup]
                        WHERE GroupID = @GroupID
                        AND FamilyID = @FamilyID";

    int rowsAffected = await Connection.ExecuteAsync(sql, group, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IList<FamilyGroup>> GetAllFamilyGroupsAsync()
  {
    const string sql = @"Select distinct GroupID, GroupName from [CSCMaster].[dbo].[FamilyGroup] order by GroupId";

    IList<FamilyGroup> groups = (IList<FamilyGroup>)await Connection.QueryAsync<FamilyGroup>(sql, null, Transaction);
    return groups;
  }
}
