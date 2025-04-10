namespace InfonetCSCR.Data.Repositories;

public class CardCodesRepository : BaseRepository, ICardCodesRepository
{
  public CardCodesRepository(IDbTransaction transaction) : base(transaction) { }
  public async Task<IEnumerable<CardRange>> GetCardRangesByCardCodeAsync(string cardCode)
  {
    StringBuilder query = new();
    query.Append($@"SELECT CardCode, LowerLimit, UpperLimit FROM CardCodes WHERE CardCode=@CardCode;");

    var parameters = new { CardCode = cardCode };
    IEnumerable<CardRange> cardRanges = await Connection.QueryAsync<CardRange>(query.ToString(), parameters, Transaction);
    return cardRanges;
  }

  public async Task<CardRange> GetCardRangeByLowerLimitAsync(int cardCode, string lowerLimit)
  {
    StringBuilder query = new();
    query.Append($@"SELECT CardCode, LowerLimit, UpperLimit FROM CardCodes WHERE CardCode=@CardCode and LowerLimit=@LowerLimit;");

    var parameters = new { CardCode = cardCode, LowerLimit = lowerLimit };
    CardRange cardRange = await Connection.QueryFirstOrDefaultAsync<CardRange>(query.ToString(), parameters, Transaction);
    return cardRange;
  }

  public async Task<bool> AddCardRangeAsync(int cardCode, string lowerLimit, string upperLimit)
  {
    const string sql = @"INSERT INTO [CSCMaster].[dbo].[CardCodes] 
                          (CardCode, LowerLimit, UpperLimit)
                          VALUES (@CardCode, @LowerLimit, @UpperLimit)";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@CardCode", SqlDbType.Int).Value = cardCode;
    cmd.Parameters.Add("@LowerLimit", SqlDbType.VarChar, 20).Value = lowerLimit;
    cmd.Parameters.Add("@UpperLimit", SqlDbType.VarChar, 20).Value = upperLimit;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteCardRangesByCardCodeDataAsync(string cardCode, string lowerLimit, string upperLimit)
  {
    const string sql = @"DELETE from [CSCMaster].[dbo].[CardCodes]
                        WHERE CardCode = @CardCode
                        AND LowerLimit= @LowerLimit
                        AND UpperLimit= @UpperLimit";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@CardCode", SqlDbType.VarChar, 10).Value = cardCode;
    cmd.Parameters.Add("@LowerLimit", SqlDbType.VarChar, 20).Value = lowerLimit;
    cmd.Parameters.Add("@UpperLimit", SqlDbType.VarChar, 20).Value = upperLimit;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteCardRangesByCardCodeAsync(string cardCode)
  {
    const string sql = @"DELETE from [CSCMaster].[dbo].[CardCodes]
                        WHERE CardCode = @CardCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@CardCode", SqlDbType.VarChar, 10).Value = cardCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }
}
