namespace InfonetCSCR.Data.Repositories;

public class FunctionListRepository : BaseRepository, IFunctionListRepository
{
  public FunctionListRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<FunctionList>> GetAllFunctionListDataAsync()
  {
    string sql = $@"SELECT 
                     ButtonType,
                     FunctionID,
                     FunctionName,
                     PictureName,
                     DisplayName,
                     TenderName
                  FROM CSCMaster.dbo.FunctionList;";

    IList<FunctionList> functionLists = (IList<FunctionList>)await Connection.QueryAsync<FunctionList>(sql, null, Transaction);
    return functionLists;
  }

  public async Task<IList<FunctionList>> GetFunctionListByButtonTypeAsync(int buttonType)
  {
    string sql = $@"SELECT 
                     ButtonType,
                     FunctionID,
                     FunctionName,
                     PictureName,
                     DisplayName,
                     TenderName
                  FROM CSCMaster.dbo.FunctionList
                  WHERE ButtonType = @buttonType;";

    var parameters = new { ButtonType = buttonType };
    IList<FunctionList> functionLists = (IList<FunctionList>)await Connection.QueryAsync<FunctionList>(sql, parameters, Transaction);
    return functionLists;
  }
}
