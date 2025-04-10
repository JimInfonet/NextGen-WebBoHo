namespace InfonetCSCR.Data.Repositories;

public class PolicyByClassRepository : BaseRepository, IPolicyByClassRepository
{
  public PolicyByClassRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<PolicyClass>> GetPolicyClassesAsync()
  {
    const string sql = @"
      SELECT DISTINCT
        PCMP.P_CLASS   AS [Class],
        PCLS.P_CL_NAME AS [ClassName]
      FROM [CSCAdmin].[dbo].[P_CLASS] AS PCLS
      INNER JOIN [CSCAdmin].[dbo].[P_Comp] AS PCMP 
      ON PCLS.P_CLASS = PCMP.P_CLASS
      WHERE PCMP.Implemented = 1
      ORDER BY PCLS.P_CL_NAME;";

    return await Connection.QueryAsync<PolicyClass>(sql, null, Transaction);
  }

  public async Task<IEnumerable<PolicyCompany>> GetPoliciesByClassAsync(string className)
  {
    return await Connection.QueryAsync<PolicyCompany>("Sproc_GetPoliciesByClass", new { ClassName = className }, Transaction, commandType: CommandType.StoredProcedure);
  }

  public async Task<IEnumerable<PolicyCompany>> GetPolicySettingsByNameAsync(string policyName)
  {
    const string sql = @"
      SELECT
        'Company' AS [Levels],
        ' '       AS [Value],
        P_Set     AS [Settings]
      From [CSCAdmin].[dbo].P_Comp
      WHERE P_Name = @PolicyName 
      UNION
      SELECT
        P_LEVEL AS [Levels],
        P_VALUE AS [Value],
        P_Set   AS [Settings]
      FROM [CSCAdmin].[dbo].P_Set
      WHERE P_Name = @PolicyName";

    IEnumerable<PolicyCompany> items = await Connection.QueryAsync<PolicyCompany>(sql, new { PolicyName = policyName }, Transaction);
    return items;
  }

  public async Task<bool> UpdateTimesheetPayPeriodOnLoadAsync(long initialEmpID)
  {
    const string sql = @"
      UPDATE CSCTrans.[dbo].[TimesheetPayPeriod]
      SET InitialEmpID = @InitialEmpID
      WHERE InitialEmpID = 0";

    var affectedRows = await Connection.ExecuteAsync(sql, new { InitialEmpID = initialEmpID }, Transaction);
    return affectedRows > 0;
  }
}
