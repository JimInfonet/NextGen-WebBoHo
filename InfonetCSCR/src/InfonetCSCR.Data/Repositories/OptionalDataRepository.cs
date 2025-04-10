namespace InfonetCSCR.Data.Repositories;

public class OptionalDataRepository : BaseRepository, IOptionalDataRepository
{
  public OptionalDataRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<List<string>> GetOptionalProfileIDsAsync()
  {
    StringBuilder query = new();
    query.Append($@"SELECT DISTINCT ProfileID From OptData ORDER BY ProfileID;");

    List<string> profileIDs = (List<string>)await Connection.QueryAsync<string>(query.ToString(), null, Transaction);
    return (List<string>)profileIDs;
  }
}
