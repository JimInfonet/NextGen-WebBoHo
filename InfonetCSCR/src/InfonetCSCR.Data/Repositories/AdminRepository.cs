namespace InfonetCSCR.Data.Repositories;

public class AdminRepository : BaseRepository, IAdminRepository
{
  public AdminRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<Admin> GetAdminByName(string name)
  {
    string sql = $@"Select * from [CSCAdmin].[dbo].[Admin] Where Name = @Name";

    var parameter = new { Name = name };
    Admin admin = await Connection.QueryFirstOrDefaultAsync<Admin>(sql, parameter, Transaction);
    return admin;
  }
}
