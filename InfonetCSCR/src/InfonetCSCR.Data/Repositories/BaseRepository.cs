namespace InfonetCSCR.Data.Repositories;

public class BaseRepository
{
    protected IDbTransaction Transaction { get; private set; }
    protected IDbConnection Connection => Transaction.Connection ?? throw new InvalidOperationException("Transaction.Connection is null");

    public BaseRepository(IDbTransaction transaction)
    {
        Transaction = transaction;
    }
}
