namespace InfonetCSCR.Data.Repositories;

public class TillsSaleTendRepository : BaseRepository, ITillsSaleTendRepository
{
  public TillsSaleTendRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> CheckTranscationExistsForTendNameAsync(string tendName)
  {
    StringBuilder query = new();
    query.Append($@"Select TENDNAME from [CSCTills].[dbo].[SALETEND] where TENDNAME=@TendName");

    var parameters = new { TendName = tendName };
    string tendNameData = await Connection.QueryFirstOrDefaultAsync<string>(query.ToString(), parameters, Transaction);
    return (tendNameData != null);
  }
}
