namespace InfonetCSCR.Data.Repositories;

public class PolicyByEntityRepository : BaseRepository, IPolicyByEntityRepository
{
  public PolicyByEntityRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<Entity>> GetEntityListAsync()
  {
    const string sql = @"
      SELECT
        E_NAME AS [EntityName],
        E_DESC AS [Description],
        E_Val_File AS [EntityValueFile]
      FROM [CSCAdmin].[dbo].[E_ENT]
      ORDER BY E_DESC;";

    return await Connection.QueryAsync<Entity>(sql, null, Transaction);
  }

  public async Task<IEnumerable<PolicyCompany>> GetPolicySettingsByEntityAsync(Entity entity)
  {
    const string sql = @"
      SELECT
        PCMP.P_Name AS [Name],
        PCMP.P_Desc AS [Description],
        PCMP.P_Choices AS [Choices],
        ISNULL(PS.P_Level, 'Company') AS [Levels],
        ISNULL(PS.P_Value, '') AS [Value],
        ISNULL(PS.P_Set, PCMP.P_Set) AS [Settings]
      FROM [CSCAdmin].[dbo].[P_Comp] PCMP
      INNER JOIN [CSCAdmin].[dbo].[P_Canbe] PCNB
        ON PCMP.P_Name = PCNB.P_Name AND PCNB.P_Canbe = @EntityName
      LEFT JOIN [CSCAdmin].[dbo].[P_Set] PS
        ON PCMP.P_Name = PS.P_Name AND PS.P_Level = @EntityName AND PS.P_Value = @Value
      WHERE PCMP.Implemented = 1;";

    return await Connection.QueryAsync<PolicyCompany>(sql, entity, Transaction);
  }
}
