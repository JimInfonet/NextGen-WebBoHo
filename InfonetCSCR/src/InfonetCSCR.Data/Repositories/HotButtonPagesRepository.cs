namespace InfonetCSCR.Data.Repositories;

public class HotButtonPagesRepository : BaseRepository, IHotButtonPagesRepository
{
  public HotButtonPagesRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<HotButtonPages>> GetAllHotButtonPagesAsync()
  {
    string sql = $@"SELECT
	                    PageID,
	                    PageName
                    FROM [CSCMaster].[dbo].[HotButtonPages]";

    IList<HotButtonPages> hotButtonPages = (IList<HotButtonPages>)await Connection.QueryAsync<HotButtonPages>(sql, null, Transaction);
    return hotButtonPages;
  }

  public async Task<HotButtonPages?> GetHotButtonPageByIdAsync(int pageId)
  {
    string sql = $@"SELECT
	                    PageID,
	                    PageName
                    FROM [CSCMaster].[dbo].[HotButtonPages]
                    WHERE PageID = @PageId";

    var parameters = new { PageId = pageId };
    HotButtonPages? hotButtonPage = await Connection.QueryFirstOrDefaultAsync<HotButtonPages?>(sql, parameters, Transaction);
    return hotButtonPage;
  }

  public async Task<bool> InsertHotButtonPageAsync(HotButtonPages hotButtonPages)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[HotButtonPages]
                    (PageID, PageName)
                    VALUES (@PageID, @PageName);";


    int rowsAffected = await Connection.ExecuteAsync(sql, hotButtonPages, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateHotButtonPageAsync(HotButtonPages hotButtonPages)
  {
    string sql = @"UPDATE [CSCMaster].[dbo].[HotButtonPages]
                    SET PageName = @PageName
                    WHERE PageID = @PageID;";

    int rowsAffected = await Connection.ExecuteAsync(sql, hotButtonPages, Transaction);
    return rowsAffected > 0;
  }
}
