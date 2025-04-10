namespace InfonetCSCR.Data.Repositories;

public class FunctionButtonsRepository : BaseRepository, IFunctionButtonsRepository
{
  public FunctionButtonsRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<FunctionButton>> GetAllFunctionButtonsDataAsync()
  {
    string sql = $@"SELECT 
                      ButtonID,
                      FunctionID,
                      PictureID,
                      ButtonType
                    FROM [CSCMaster].[dbo].[FunctionButtons];";

    IList<FunctionButton> functionButtons = (IList<FunctionButton>)await Connection.QueryAsync<FunctionButton>(sql, null, Transaction);
    return functionButtons;
  }

  public async Task<FunctionButton> GetFunctionButtonsDataByButtonIdAsync(int buttonId)
  {
    string sql = $@"SELECT 
                      ButtonID,
                      FunctionID,
                      PictureID,
                      ButtonType
                    FROM [CSCMaster].[dbo].[FunctionButtons]
                    WHERE ButtonID = @ButtonID;";

    var parameters = new { ButtonID = buttonId };
    FunctionButton functionButton = await Connection.QueryFirstOrDefaultAsync<FunctionButton>(sql, parameters, Transaction);
    return functionButton;
  }

  public async Task<bool> InsertFunctionButtonAsync(FunctionButton functionButton)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[FunctionButtons] (ButtonID, FunctionID, PictureID, ButtonType)
                   VALUES (@ButtonID, @FunctionID, @PictureID, @ButtonType);";

    int rowsAffected = await Connection.ExecuteAsync(sql, functionButton, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateFunctionButtonAsync(FunctionButton functionButton)
  {
    string sql = @"UPDATE [CSCMaster].[dbo].[FunctionButtons]
                   SET FunctionID = @FunctionID,
                       PictureID = @PictureID,
                       ButtonType = @ButtonType
                   WHERE ButtonID = @ButtonID;";

    int rowsAffected = await Connection.ExecuteAsync(sql, functionButton, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteFunctionButtonAsync(int buttonId)
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[FunctionButtons]
                   WHERE ButtonID = @ButtonID;";

    var parameters = new { ButtonID = buttonId };

    int rowsAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);
    return rowsAffected > 0;
  }
}
