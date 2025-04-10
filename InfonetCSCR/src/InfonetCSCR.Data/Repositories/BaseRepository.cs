namespace InfonetCSCR.Data.Repositories;

public class BaseRepository
{
  protected IDbTransaction? Transaction { get; private set; }
  protected IDbConnection? Connection => Transaction?.Connection;

  public BaseRepository(IDbTransaction transaction)
  {
    Transaction = transaction;
  }
}
