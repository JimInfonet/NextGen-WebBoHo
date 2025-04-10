namespace InfonetCSCR.Data.Repositories;

public class SubDepartmentRepository : BaseRepository, ISubDepartmentRepository
{
  public SubDepartmentRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> CheckSubDepartmentIdExistsForDepartmentAsync(string departmentId, string subDepartmentId)
  {
    const string sql = @"SELECT [SUB_DEPT] FROM [CSCMaster].[dbo].[SubDept] WHERE DEPT = @DepartmentId AND SUB_DEPT = @SubDepartmentId";

    var parameters = new { DepartmentId = departmentId, SubDepartmentId = subDepartmentId };
    string subDepartment = await Connection.QueryFirstOrDefaultAsync<string>(sql, parameters, Transaction);
    return (subDepartment != null);
  }

  public async Task<bool> AddSubDepartmentAsync(SubDepartment subDepartment)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[SubDept] (
                    [DEPT],
                    [SUB_DEPT],
                    [SUB_NAME]
                  ) VALUES (
                    @DeptId,
                    @SubDeptId,
                    @SubDeptName
                  );";

    int rowsAffected = await Connection.ExecuteAsync(sql, subDepartment, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteSubDepartmentAsync(string departmentId, string? subDepartmentId = null)
  {
    string sql = @"Delete From [CSCMaster].[dbo].[SubDept] Where Dept = @DepartmentId";

    if (subDepartmentId != null)
    {
      sql += " AND Sub_Dept = @SubDepartmentId";
    }

    var parameters = new { DepartmentId = departmentId, SubDepartmentId = subDepartmentId };

    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);

    return rowsAffected > 0;
  }

  public async Task<IEnumerable<SubDepartment>> GetSubDepartmentsListAsync()
  {
    const string sql = @"
      SELECT 
	      [DEPT]      DeptId,
	      [SUB_DEPT]  SubDeptId,
	      [SUB_NAME]  SubDeptName
      FROM [CSCMaster].[dbo].[SubDept]";

    return await Connection.QueryAsync<SubDepartment>(sql, null, Transaction);
  }

  public async Task<IEnumerable<SubDepartment>?> GetSubDepartmentsByDepartmentsAsync(List<string> departments)
  {
    if (departments == null || !departments.Any())
      return null;

    string sql = $@"SELECT 
                        [DEPT] AS DeptId,
                        [SUB_DEPT] AS SubDeptId,
                        [SUB_NAME] AS SubDeptName
                    FROM [CSCMaster].[dbo].[SubDept]
                    WHERE [DEPT] IN @Departments
                    ORDER BY DeptId, SubDeptId";

    IEnumerable<SubDepartment>? subDepartments = await Connection.QueryAsync<SubDepartment>(sql, new { Departments = departments }, Transaction);
    return subDepartments;
  }
}
