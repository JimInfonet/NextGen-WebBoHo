namespace InfonetCSCR.Data.Repositories;

public class SubDetailRepository : BaseRepository, ISubDetailRepository
{
  public SubDetailRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> CheckDetailIdExistsForDepartmentAndSubDepartmentAsync(string departmentId, string subDepartmentId, string detailId)
  {
    const string sql = @"SELECT SubDetail FROM [CSCMaster].[dbo].[SubDetail] 
                           WHERE Dept = @DepartmentId AND SubDept = @SubDepartmentId AND SubDetail = @SubDetail";

    var parameters = new { DepartmentId = departmentId, SubDepartmentId = subDepartmentId, SubDetail = detailId };
    string detail = await Connection.QueryFirstOrDefaultAsync<string>(sql, parameters, Transaction);
    return (detail != null);
  }

  public async Task<bool> AddDetailAsync(SubDetail subDetail)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[SubDetail] (
                        [Dept],
                        [SubDept],
                        [SubDetail],
                        [SubDetail_Name]
                    ) VALUES (
                        @DeptId,
                        @SubDeptId,
                        @SubDetailId,
                        @SubDetailName
                    );";

    int rowsAffected = await Connection.ExecuteAsync(sql, subDetail, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteSubDetailAsync(string departmentId, string? subDepartmentId = null, string? detailId = null)
  {
    string sql = @"Delete From [CSCMaster].[dbo].[SubDetail] Where Dept = @DepartmentId";

    if (subDepartmentId != null)
    {
      sql += " AND SubDept = @SubDepartmentId";
    }

    if (detailId != null)
    {
      sql += " AND SubDetail = @SubDetailId";
    }

    var parameters = new { DepartmentId = departmentId, SubDepartmentId = subDepartmentId, SubDetailId = detailId };

    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);

    return rowsAffected > 0;
  }

  public async Task<IEnumerable<SubDetail>> GetSubDetailsListAsync()
  {
    const string sql = @"
      SELECT 
	      [Dept] DeptId,
	      [SubDept] SubDeptId,
	      [SubDetail] SubDetailId,
	      [SubDetail_Name] SubDetailName
      FROM [CSCMaster].[dbo].[SubDetail]";

    return await Connection.QueryAsync<SubDetail>(sql, null, Transaction);
  }

  public async Task<IEnumerable<SubDetail>?> GetSubDetailsByDepartmentsAsync(List<string> departments)
  {
    if (departments == null || !departments.Any())
      return null;

    string sql = $@"SELECT
                        [Dept] DeptId,
                        [SubDept] SubDeptId,
                        [SubDetail] SubDetailId,
                        [SubDetail_Name] SubDetailName
                    FROM [CSCMaster].[dbo].[SubDetail]
                    WHERE Dept IN @Departments
                    ORDER BY DeptId, SubDeptId, SubDetailId";

    IEnumerable<SubDetail>? subDetails = await Connection.QueryAsync<SubDetail>(sql, new { Departments = departments }, Transaction);
    return subDetails;
  }
}
