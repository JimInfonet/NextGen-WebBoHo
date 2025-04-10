namespace InfonetCSCR.Data.Repositories;

public class PositiveCardRepository : BaseRepository, IPositiveCardRepository
{
  public PositiveCardRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<PositiveCard>> GetAllPositiveCardsDataAsync()
  {
    string sql = $@"SELECT CardNumber, Active, PIN FROM [CSCMaster].[dbo].[poscardstbl];";

    IList<PositiveCard> positiveCards = 
      (IList<PositiveCard>)await Connection.QueryAsync<PositiveCard>(sql, null, Transaction);
    return positiveCards;
  }

  public async Task<bool> InsertPositiveCardAsync(PositiveCard positiveCard)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[poscardstbl]
                   (CardNumber, Active, PIN)
                   VALUES (@CardNumber, @Active, @PIN);";

    int rowsAffected = await Connection.ExecuteAsync(sql, positiveCard, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> InsertPositiveCardsBatchAsync(IList<PositiveCard> positiveCards)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[poscardstbl]
                   (CardNumber, Active, PIN)
                   VALUES (@CardNumber, @Active, @PIN);";

    int rowsAffected = await Connection.ExecuteAsync(sql, positiveCards, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeletePositiveCardAsync(PositiveCard positiveCard)
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[poscardstbl]
                   WHERE CardNumber = @CardNumber;";

    int rowsAffected = await Connection.ExecuteAsync(sql, positiveCard, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeletePositiveCardsBatchAsync(IList<PositiveCard> positiveCards)
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[poscardstbl]
                   WHERE CardNumber = @CardNumber;";

    int rowsAffected = await Connection.ExecuteAsync(sql, positiveCards, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteAllPositiveCardsAsync()
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[poscardstbl];";

    int rowsAffected = await Connection.ExecuteAsync(sql, null, Transaction);
    return rowsAffected > 0;
  }
}
