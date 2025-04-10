namespace InfonetCSCR.Data.Repositories;

public class GroupButtonsRepository : BaseRepository, IGroupButtonsRepository
{
  public GroupButtonsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<GroupButton>> GetAllGroupButtonsDataAsync()
  {
    string sql = $@"SELECT
                        Button ButtonId,
                        Product ProductCode,
                        Description_1 Description1,
                        Description_2 Description2,
                        ButtonType
                    FROM
                        [CSCMaster].[dbo].[GroupButtons];";

    IList<GroupButton> groupButtons = (IList<GroupButton>)await Connection.QueryAsync<GroupButton>(sql, null, Transaction);
    return groupButtons;
  }

  public async Task<GroupButton> GetGroupButtonByIdAsync(int buttonId)
  {
    string sql = @"SELECT Button ButtonId, Product ProductCode, Description_1 AS Description1, Description_2 AS Description2, ButtonType
                   FROM [CSCMaster].[dbo].[GroupButtons]
                   WHERE Button = @ButtonId;";

    var parameters = new { ButtonId = buttonId };
    GroupButton groupButton = await Connection.QueryFirstOrDefaultAsync<GroupButton>(sql, parameters, Transaction);
    return groupButton;
  }

  public async Task<bool> InsertGroupButtonAsync(GroupButton groupButton)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[GroupButtons]
                   (Button, Product, Description_1, Description_2, ButtonType)
                   VALUES
                   (@ButtonId, @ProductCode, @Description1, @Description2, @ButtonType);";

    int rowsAffected = await Connection.ExecuteAsync(sql, groupButton, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateGroupButtonAsync(GroupButton groupButton)
  {
    string sql = @"UPDATE [CSCMaster].[dbo].[GroupButtons]
                   SET Product = @ProductCode,
                       Description_1 = @Description1,
                       Description_2 = @Description2,
                       ButtonType = @ButtonType
                   WHERE Button = @ButtonId;";

    int rowsAffected = await Connection.ExecuteAsync(sql, groupButton, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteGroupButtonAsync(int buttonId)
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[GroupButtons]
                   WHERE Button = @ButtonId;";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { ButtonId = buttonId }, Transaction);
    return rowsAffected > 0;
  }

}
