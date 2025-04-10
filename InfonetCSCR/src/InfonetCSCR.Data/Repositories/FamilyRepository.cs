namespace InfonetCSCR.Data.Repositories;

public class FamilyRepository : BaseRepository, IFamilyRepository
{
  public FamilyRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<Family>> GetAllFamilyAsync()
  {
    const string sql = @"SELECT 
	                        [FamilyID] FamilyId,
	                        [Description] Description
                        FROM [CSCMaster].[dbo].[Family]  
                        ORDER BY [CSCMaster].[dbo].[Family].FamilyID";

    IList<Family> families = (IList<Family>)await Connection.QueryAsync<Family>(sql, null, Transaction);
    return families;
  }

  public async Task<bool> AddFamilyAsync(Family family)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[Family]
            (
                [FamilyID],
                [Description]
            )
            VALUES
            (
                @FamilyID,
                @Description
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, family, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateFamilyAsync(Family family)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[Family]
                      SET
                          Description = @Description
                      WHERE
                          FamilyID = @FamilyID";
    int rowsAffected = await Connection.ExecuteAsync(sql, family, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteFamilyAsync(string familyId)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[Family]
                        WHERE FamilyID = @FamilyID";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { FamilyID = familyId }, Transaction);
    return rowsAffected > 0;
  }
}
