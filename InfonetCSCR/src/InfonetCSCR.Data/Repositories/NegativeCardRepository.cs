namespace InfonetCSCR.Data.Repositories;

public class NegativeCardRepository : BaseRepository, INegativeCardRepository
{
  public NegativeCardRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<NegativeCard>> GetAllNegativeCardsDataAsync()
  {
    string sql = $@"SELECT 
                        CardNumber,
                        MessageCode,
                        ExpiryDate,
                        EntryDate
                    FROM 
                        [CSCMaster].[dbo].[negcardstbl];";

    IList<NegativeCard> negativeCards =
      (IList<NegativeCard>)await Connection.QueryAsync<NegativeCard>(sql, null, Transaction);
    return negativeCards;
  }

  public async Task<bool> InsertNegativeCardAsync(NegativeCard negativeCard)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[negcardstbl]
                   (CardNumber, MessageCode, ExpiryDate, EntryDate)
                   VALUES (@CardNumber, @MessageCode, @ExpiryDate, @EntryDate);";
    int rowsAffected = await Connection.ExecuteAsync(sql, negativeCard, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteNegativeCardAsync(NegativeCard negativeCard)
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[negcardstbl]
                   WHERE CardNumber = @CardNumber;";
    int rowsAffected = await Connection.ExecuteAsync(sql, negativeCard, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> InsertNegativeCardsBatchAsync(IList<NegativeCard> negativeCards)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[negcardstbl]
                   (CardNumber, MessageCode, ExpiryDate, EntryDate)
                   VALUES (@CardNumber, @MessageCode, @ExpiryDate, @EntryDate);";
    int rowsAffected = await Connection.ExecuteAsync(sql, negativeCards, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteNegativeCardsBatchAsync(IList<NegativeCard> negativeCards)
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[negcardstbl]
                   WHERE CardNumber = @CardNumber;";
    int rowsAffected = await Connection.ExecuteAsync(sql, negativeCards, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteAllNegativeCardsDataAsync()
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[negcardstbl];";
    int rowsAffected = await Connection.ExecuteAsync(sql, null, Transaction);
    return rowsAffected > 0;
  }
}
