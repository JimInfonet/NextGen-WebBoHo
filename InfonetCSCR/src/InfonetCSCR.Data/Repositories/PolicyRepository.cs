namespace InfonetCSCR.Data.Repositories;

//CSCAdmin Connection
public class PolicyRepository : BaseRepository, IPolicyRepository
{
  public PolicyRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<PolicyCompany>> GetPolicyAllCompany()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  [P_CLASS] AS Class, [P_SEQ] AS Sequence, [Implemented] AS Implemented, [P_NAME] AS Name, [P_DESC] AS Description,
                              [P_LEVELS] AS Levels, [P_CHOICES] AS Choices, [P_SET] AS Settings, [P_VARTYPE] AS VarType, [P_ACTIVE] AS Active, [P_USED] AS Used
                      FROM    [CSCAdmin].[dbo].[P_COMP]");


    return await Connection.QueryAsync<PolicyCompany>(query.ToString(), null, Transaction);

  }

  public async Task<IEnumerable<PolicySettings>> GetPolicyAllSettings()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  [P_NAME] AS Name, [P_LEVEL] AS Level, [P_VALUE] AS Value, [P_SET] AS Settings
                      FROM    [CSCAdmin].[dbo].[P_SET]");

    return await Connection.QueryAsync<PolicySettings>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<PolicyCanbe>> GetPolicyAllCanbe()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  [P_NAME] AS Name, [P_SEQ] AS Sequence, [P_Canbe] AS Canbe
                      FROM    [CSCAdmin].[dbo].[P_CANBE]");

    return await Connection.QueryAsync<PolicyCanbe>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<PolicyClass>> GetPolicyAllClass()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT  [P_CLASS] AS Class, [P_CL_NAME] AS ClassName
                      FROM    [CSCAdmin].[dbo].[P_CLASS]");

    return await Connection.QueryAsync<PolicyClass>(query.ToString(), null, Transaction);
  }

  public async Task<bool> GetPolicyNetworkUpdateByPriceGroup()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select P_NAME from [CSCAdmin].[dbo].[P_SET] where P_name = 'NETWORKUPD'  and P_LEVEL = 'PRICEGROUP' and P_SET = 'Yes'");

    var result = await Connection.QueryAsync<string>(query.ToString(), null, Transaction);
    return (result.Count() != 0);
  }

  public async Task<bool> GetPolicyNetworkUpdateByRegion()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select P_NAME from [CSCAdmin].[dbo].[P_SET] where P_name = 'NETWORKUPD'  and P_LEVEL = 'REGION' and P_SET = 'Yes'");

    var result = await Connection.QueryAsync<string>(query.ToString(), null, Transaction);
    return (result.Count() != 0);
  }

  public async Task<bool> GetPolicyNetworkUpdate()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select P_NAME from [CSCAdmin].[dbo].[P_SET] where P_name = 'NETWORKUPD'  and P_SET = 'Yes'");

    var result = await Connection.QueryAsync<string>(query.ToString(), null, Transaction);
    return (result.Count() != 0);
  }

  public async Task<bool> DeletePSetAsync(string departmentId, string? subDepartmentId = null, string? detailId = null)
  {
    string pLevel = "DEPT";
    string pValue = departmentId;

    if ( subDepartmentId != null )
    {
      pLevel = "SUBDEPT";
      pValue = departmentId + "ÿ" + subDepartmentId; // ÿ is hard coded in VB code
    }

    if (detailId != null )
    {
      pLevel = "SUBDETAIL";
      pValue = departmentId + "ÿ" + subDepartmentId + "ÿ" + detailId; // ÿ is hard coded in VB code
    }

    const string sql = @"Delete From [CSCAdmin].[dbo].[P_SET] Where P_Level = @PLevel AND P_Value = @PValue";

    var parameters = new { PLevel = pLevel, PValue = pValue };

    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);

    return rowsAffected > 0;
  }

  public async Task<bool> CheckPSetExistsAsync(string pName, string pSet, string? pLevel = null, string? pValue = null)
  {
    string sql = @"SELECT P_SET from [CSCAdmin].[dbo].[P_SET] where P_NAME=@PName and P_SET=@PSet";

    if(pLevel != null)
    {
      sql += " and P_LEVEL=@PLevel and P_Value= @PValue";
    }

    var parameters = new { PName = pName, PSet = pSet, PLevel = pLevel, PValue = pValue };
    string pSetData = await Connection.QueryFirstOrDefaultAsync<string>(sql, parameters, Transaction);
    return (pSetData != null);
  }

  public async Task<IEnumerable<PSet>> GetPSetsForExptTaxAndTaxNameAsync(IEnumerable<string> pSets)
  {
    string sql = @$"SELECT 
	                    [P_NAME] PName,
	                    [P_LEVEL] PLevel,
	                    [P_VALUE] PValue,
	                    [P_SET] PSetData
                    from [CSCAdmin].[dbo].[P_SET] 
                    where P_NAME='EXPT_TAX' and P_SET in @PSets";

    var parameters = new { PSets = pSets };
    IEnumerable<PSet> pSetsData = await Connection.QueryAsync<PSet>(sql, parameters, Transaction);
    return pSetsData;
  }

  public async Task<IEnumerable<PSet>> GetPSetsByPNamePSetPLevelPValueAsync(
    IList<(string pName, string pSet, string pLevel, string pValue)> pSets)
  {
    string sql = @$"SELECT 
	                    [P_NAME] PName,
	                    [P_LEVEL] PLevel,
	                    [P_VALUE] PValue,
	                    [P_SET] PSetData
                    from [CSCAdmin].[dbo].[P_SET]";

    if(pSets.Count() > 0)
    {
      sql += " where (";
      bool firstIteration = true;
      foreach(var pSet in pSets)
      {
        if(!firstIteration)
        {
          sql += " OR";
        }

        firstIteration = false;
        sql += @$" (P_NAME='{pSet.pName}' and P_SET = '{pSet.pSet}' and P_LEVEL = '{pSet.pLevel}' and P_VALUE = '{pSet.pValue}')";
      }

      sql += " );";
    }
    else
    {
      sql += "where (P_NAME = null and P_SET = null and P_LEVEL = null and P_VALUE = null)";
    }

    var parameters = new { PSets = pSets };
    IEnumerable<PSet> pSetsData = await Connection.QueryAsync<PSet>(sql, parameters, Transaction);
    return pSetsData;
  }

  public async Task<bool> CheckIfPolicySettingExistsAsync(PolicyCompany policySetting)
  {
    const string sql = @"
      SELECT COUNT(*)
      FROM [CSCAdmin].[dbo].P_Set
      WHERE
        P_Name = @Name AND
        P_LEVEL = @Levels AND
        P_VALUE = @Value";

    var count = await Connection.ExecuteScalarAsync<int>(sql, policySetting, Transaction);
    return count > 0;
  }

  public async Task<bool> AddPolicySettingAsync(PolicyCompany policySetting)
  {
    const string sql = @"
      INSERT INTO [CSCAdmin].[dbo].P_Set (P_NAME, P_LEVEL, P_VALUE, P_SET)
      VALUES (@Name, @Levels, @Value, @Settings)";

    var affectedRows = await Connection.ExecuteAsync(sql, policySetting, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> UpdatePolicySetSettingAsync(PolicyCompany policySetting)
  {
    const string sql = @"
      UPDATE [CSCAdmin].[dbo].P_Set
      SET P_SET = @Settings
      WHERE
        P_Name = @Name AND
        P_LEVEL = @Levels AND
        P_VALUE = @Value";

    var affectedRows = await Connection.ExecuteAsync(sql, policySetting, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> UpdatePolicyCompSettingAsync(PolicyCompany policySetting)
  {
    const string sql = @"
      UPDATE [CSCAdmin].[dbo].P_Comp
      SET P_SET = @Settings
      WHERE P_Name = @Name";

    var affectedRows = await Connection.ExecuteAsync(sql, policySetting, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> UpdateTimesheetPayPeriodAsync(string initialEmpID)
  {
    const string sql = "UPDATE CSCTrans.[dbo].[TimesheetPayPeriod] SET InitialEmpID = @InitialEmpID";
    var affectedRows = await Connection.ExecuteAsync(sql, new { InitialEmpID = initialEmpID }, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> UpdateSetVendYesAsync()
  {
    const string sql = @"
      IF EXISTS (
	      SELECT SM.Stock_Code
	      FROM [CSCMaster].[dbo].[StockMst] AS SM
	      INNER JOIN [CSCMaster].[dbo].[StockCosts] AS SC
	      ON SM.Stock_Code = SC.Stock_Code
	      WHERE SC.VendorID = 'ALL'
      )
      BEGIN
	      DELETE FROM [CSCMaster].[dbo].[StockCosts] WHERE VendorID = 'ALL'
      END";

    var affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> UpdateSetVendNoAsync()
  {
    const string sql = @"
      IF EXISTS (
	      SELECT DISTINCT SC.Stock_Code
	      FROM [CSCMaster].[dbo].[StockCosts] AS SC
	      INNER JOIN [CSCMaster].[dbo].[StockMst] AS SM
	      ON SC.Stock_Code = SM.Stock_Code 
	      WHERE
		      VendorID != 'ALL' AND
		      SC.Stock_Code NOT IN (SELECT Stock_Code FROM [CSCMaster].[dbo].[StockCosts] WHERE VendorID = 'ALL')
      )
      BEGIN
	      INSERT INTO [CSCMaster].[dbo].[StockCosts] (Stock_Code, Cost, ActiveVendor,OrderNo, StoreID, VendorID)
	      SELECT DISTINCT SC.Stock_Code, SM.STD_COST, 0, 0, 'ALL', 'ALL' 
	      FROM [CSCMaster].[dbo].[StockCosts] AS SC
	      INNER JOIN [CSCMaster].[dbo].[StockMst] AS SM
	      ON SC.Stock_Code = SM.Stock_Code
	      WHERE
		      VendorID != 'ALL' AND 
		      SC.Stock_code NOT IN (SELECT Stock_Code FROM [CSCMaster].[dbo].[StockCosts] WHERE VendorID = 'ALL')
      END";

    var affectedRows = await Connection.ExecuteAsync(sql, null, Transaction);
    return affectedRows > 0;
  }

  public async Task<bool> DeletePolicySettingAsync(PolicyCompany policySetting)
  {
    const string sql = @"
      DELETE FROM [CSCAdmin].[dbo].P_Set
      WHERE
        P_Name = @Name AND
        P_LEVEL = @Levels AND
        P_VALUE = @Value";

    var affectedRows = await Connection.ExecuteAsync(sql, policySetting, Transaction);
    return affectedRows > 0;
  }
}
