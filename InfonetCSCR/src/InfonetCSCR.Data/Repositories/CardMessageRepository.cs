namespace InfonetCSCR.Data.Repositories;

public class CardMessageRepository : BaseRepository, ICardMessageRepository
{
  public CardMessageRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<CardMessage>> GetAllCardMessagesDataAsync()
  {
    string sql = $@"SELECT
	                    Code,
	                    Message
                    FROM [CSCMaster].[dbo].[cardmsgtbl];";

    IList<CardMessage> cardMessageTables =
      (IList<CardMessage>)await Connection.QueryAsync<CardMessage>(sql, null, Transaction);
    return cardMessageTables;
  }

  public async Task<bool> InsertCardMessageAsync(CardMessage cardMessage)
  {
    string sql = @"INSERT INTO [CSCMaster].[dbo].[cardmsgtbl] (Code, Message)
                   VALUES (@Code, @Message);";

    int rowsAffected = await Connection.ExecuteAsync(sql, cardMessage, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteCardMessageAsync(CardMessage cardMessage)
  {
    string sql = @"DELETE FROM [CSCMaster].[dbo].[cardmsgtbl]
                   WHERE Code = @Code;";

    int rowsAffected = await Connection.ExecuteAsync(sql, cardMessage, Transaction);
    return rowsAffected > 0;
  }
}
