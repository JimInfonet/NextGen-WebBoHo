namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
internal class CategoryRepository : BaseRepository, ICategoryRepository
{
  public CategoryRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<Department>> GetDepartmentsAsync()
  {
    const string sql = @"SELECT 
	                       [DEPT] DeptId,
	                       [DEPT_NAME] DeptName,
	                       [EOD_Detail] EODDetail,
	                       [EodGroup] EODGroup,
	                       [Cnt_Detail] CntDetail
                       FROM [CSCMaster].[dbo].[Dept] 
                       ORDER BY [CSCMaster].[dbo].[Dept].DEPT";

    IEnumerable<Department> departments = await Connection.QueryAsync<Department>(sql, null, Transaction);
    return departments;
  }

  public async Task<IEnumerable<SubDepartment>> GetSubDepartmentsAsync(string department = "")
  {
    string sql = $@"SELECT 
	                     [DEPT] DeptId,
	                     [SUB_DEPT] SubDeptId,
	                     [SUB_NAME] SubDeptName
                      FROM  [CSCMaster].[dbo].[SubDept]
                      {(string.IsNullOrWhiteSpace(department) ? "" : $"WHERE  [CSCMaster].[dbo].[SubDept].Dept = '{department}'")}
                      ORDER BY DeptId, SubDeptId";

    IEnumerable<SubDepartment> SubDepartments = await Connection.QueryAsync<SubDepartment>(sql, null, Transaction);
    return SubDepartments;
  }

  public async Task<IEnumerable<SubDetail>> GetSubDetailsAsync(string department = "", string subDepartment = "")
  {
    string sql = $@"SELECT 
	                [Dept] DeptId,
	                [SubDept] SubDeptId,
	                [SubDetail] SubDetailId,
	                [SubDetail_Name] SubDetailName
                FROM [CSCMaster].[dbo].[SubDetail] 
                {(string.IsNullOrWhiteSpace(department) || string.IsNullOrWhiteSpace(subDepartment) ? "" :
              $"WHERE [CSCMaster].[dbo].[SubDetail].Dept = '{department}' AND [CSCMaster].[dbo].[SubDetail].SubDept = '{subDepartment}'")}
                ORDER BY DeptId, SubDeptId, SubDetailId";

    IEnumerable<SubDetail> subDetails = await Connection.QueryAsync<SubDetail>(sql, null, Transaction);
    return subDetails;
  }

  public async Task<bool> MakeEODGroupNullInDeptAsync(string eodGroupId)
  {
    const string sql = @"UPDATE [CSCMaster].[dbo].[Dept] set EodGroup = NULL where EodGroup = @EodGroupId";

    var parameters = new { EodGroupId = eodGroupId };
    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateEODGroupByDeptAsync(string eodGroupId, string dept)
  {
    const string sql = @"UPDATE [CSCMaster].[dbo].[Dept] SET [EodGroup] = @EodGroupId WHERE [Dept] = @Dept";

    var parameters = new { EodGroupId = eodGroupId, Dept = dept };
    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateEodDetailByDeptAsync(string eodDetail, string dept)
  {
    const string sql = @"UPDATE [CSCMaster].[dbo].[Dept] SET [EOD_Detail] = @EodDetail WHERE [Dept] = @Dept";

    var parameters = new { EodDetail = eodDetail, Dept = dept };
    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateEodCountDetailByDeptAsync(string eodCountDetail, string dept)
  {
    const string sql = @"UPDATE [CSCMaster].[dbo].[Dept] SET [Cnt_Detail] = @EodCountDetail WHERE [Dept] = @Dept";

    var parameters = new { EodCountDetail = eodCountDetail, Dept = dept };
    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> CheckDepartmentExistsByEodGroupIdAsync(string eodGroupId)
  {
    StringBuilder query = new();
    query.Append($@"SELECT DEPT FROM [CSCMaster].[dbo].[Dept] WHERE eodgroup = @EodGroupId;");

    var parameters = new { EODGroupId = eodGroupId };
    string department = await Connection.QueryFirstOrDefaultAsync<string>(query.ToString(), parameters, Transaction);
    return (department != null);
  }

  public async Task<bool> CheckDepartmentIdExistsAsync(string departmentId)
  {
    const string sql = @"SELECT [DEPT] FROM [CSCMaster].[dbo].[Dept] WHERE DEPT = @DepartmentId";

    var parameters = new { DepartmentId = departmentId };
    string department = await Connection.QueryFirstOrDefaultAsync<string>(sql, parameters, Transaction);
    return (department != null);
  }

  public async Task<bool> AddDepartmentAsync(Department department)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[Dept] (
                        [DEPT],
                        [DEPT_NAME],
                        [EOD_Detail],
                        [EodGroup],
                        [Cnt_Detail]
                    ) VALUES (
                        @DeptId,
                        @DeptName,
                        @EODDetail,
                        @EODGroup,
                        @CntDetail
                    );";

    int rowsAffected = await Connection.ExecuteAsync(sql, department, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteCategoryByDepartmentIdAsync(string departmentId)
  {
    const string sql = @"Delete From [CSCMaster].[dbo].[Dept] Where Dept = @DepartmentId";

    var parameters = new { DepartmentId = departmentId };
    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<ClCategory>> GetClCategoryListAsync()
  {
    const string sql = @"
      SELECT
        CLCateg AS [Category],
        CLCAT_Name AS [ClCategoryName]
      FROM [CSCMaster].[dbo].[CLCATEG]
      ORDER BY CLCATEG";

    return await Connection.QueryAsync<ClCategory>(sql, null, Transaction);
  }

  public async Task<IEnumerable<DiscCode>> GetDiscCodesListAsync()
  {
    const string sql = @"
      SELECT
        Disc_Code AS [Code],
        Disc_Desc AS [DiscCodeDescription]
      FROM [CSCMaster].[dbo].[DISCCODE]
      ORDER BY Disc_Code";

    return await Connection.QueryAsync<DiscCode>(sql, null, Transaction);
  }

  public async Task<IEnumerable<Branch>> GetBranchesListAsync()
  {
    const string sql = @"
      SELECT
        BR_CODE AS [BranchCode],
        BR_NAME AS [BranchName]
      FROM [CSCMaster].[dbo].[BRANCH]
      ORDER BY BR_CODE";

    return await Connection.QueryAsync<Branch>(sql, null, Transaction);
  }

  // This method retrieves data from a different table than the GetAllUnitOfMeasuresAsync method of UnitOfMeasureRepository
  public async Task<IEnumerable<UnitOfMeasure>> GetUomListAsync()
  {
    const string sql = @"
      SELECT
        UOM      AS [UnitOfMeasureID],
        Descript AS [Description]
      FROM [CSCMaster].[dbo].[UOM]
      ORDER BY UOM";

    return await Connection.QueryAsync<UnitOfMeasure>(sql, null, Transaction);
  }
}
