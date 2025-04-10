namespace InfonetCSCR.Data.Repositories;

public class CustomerCardRepository : BaseRepository, ICustomerCardRepository
{
  public CustomerCardRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<CustomerCard>> GetCustomerCardsByCodeAsync(string code)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT [CardNum] AS CardNumber,
                                  [CL_Code] AS Code,
                                  [CardName] AS CardName,
                                  [ExpDate] AS ExpDate,
                                  [PIN] AS PIN,
                                  [CardStatus] AS CardStatus,
                                  [CreditLimiit] AS CreditLimit,
                                  [Balance] AS Balance,
                                  [AllowRedemption] AS AllowRedemption,
                                  [TECardNumber] AS TECardNumber,
                                  [ProfileID] AS ProfileID,
                                  [Sequence] AS Sequence
                           FROM [CSCMaster].[dbo].[ClientCard]
                           WHERE CL_CODE = @Code");

    IEnumerable<CustomerCard> customerCards = await Connection.QueryAsync<CustomerCard>(query.ToString(), new { Code = code }, Transaction);
    return customerCards;
  }

  public async Task<bool> AddCustomerCardAsync(CustomerCard customerCard)
  {
    string sql = @"
            INSERT INTO [CSCMaster].[dbo].[ClientCard]
            (
                [CardNum],
                [CL_Code],
                [CardName],
                [ExpDate],
                [PIN],
                [CardStatus],
                [CreditLimiit],
                [Balance],
                [AllowRedemption],
                [TECardNumber],
                [ProfileID],
                [Sequence]
            )
            VALUES
            (
                @CardNumber,
                @Code,
                @CardName,
                @ExpDate,
                ISNULL(@PIN, ''),
                @CardStatus,
                ISNULL(@CreditLimit, 0.00),
                ISNULL(@Balance, 0.00),
                @AllowRedemption,
                @TECardNumber,
                @ProfileID,
                @Sequence
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, customerCard, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateCustomerCardAsync(CustomerCard customerCard)
  {
    string sql = @"
            UPDATE [CSCMaster].[dbo].[ClientCard]
            SET
                [CL_Code] = @Code,
                [CardName] = @CardName,
                [ExpDate] = @ExpDate,
                [PIN] = ISNULL(@PIN, ''),
                [CardStatus] = @CardStatus,
                [CreditLimiit] = @CreditLimit,
                [Balance] = ISNULL(@Balance, 0.00),
                [AllowRedemption] = @AllowRedemption,
                [TECardNumber] = ISNULL(@TECardNumber, 0),
                [ProfileID] = @ProfileID,
                [Sequence] = @Sequence
            WHERE [CardNum] = @CardNumber";

    int rowsAffected = await Connection.ExecuteAsync(sql, customerCard, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteCustomerCardByNumberAsync(string customerCardNumber)
  {
    string sql = @"
            DELETE FROM [CSCMaster].[dbo].[ClientCard]
            WHERE [CardNum] = @CardNumber";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { CardNumber = customerCardNumber }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteRewardCardAsync(string customerCardNumber, string rewardCardNumber)
  {
    string sql = @"
            DELETE FROM [CSCMaster].[dbo].[KickBack]
            WHERE [CustomerCardNum] = @CustomerCardNumber
            AND [PointCardNum] = @RewardCardNumber";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { CustomerCardNumber = customerCardNumber, RewardCardNumber = rewardCardNumber }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> IsCustomerCardExistAsync(string number)
  {
    var sql = @"
                SELECT
                    [CardNum],
                    [CL_Code],
                    [CardName],
                    [ExpDate],
                    [PIN],
                    [CardStatus],
                    [CreditLimiit],
                    [Balance],
                    [AllowRedemption],
                    [TECardNumber],
                    [ProfileID],
                    [Sequence]
                FROM [CSCMaster].[dbo].[ClientCard]
                WHERE [CardNum] = @CardNumber";

    var parameters = new { CardNumber = number };

    CustomerCard customerCard = await Connection.QueryFirstOrDefaultAsync<CustomerCard>(sql, parameters, Transaction);
    return customerCard != null;
  }

  public async Task<IEnumerable<RewardCard>> GetRewardCardByNumberAsync(string number)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT [CustomerCardNum],
                           [PointCardNum],
                           [PhoneNum]
                    FROM [CSCMaster].[dbo].[KickBack]
                    WHERE [CustomerCardNum] = @Number");

    IEnumerable<RewardCard> rewardCards = await Connection.QueryAsync<RewardCard>(query.ToString(), new { Number = number }, Transaction);
    return rewardCards;
  }

  public async Task<bool> IsRewardCardExistByCustomerCardAndRewardCardNumberAsync(string custCardNumber, string rewardCardNumber)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT [CustomerCardNum],
                           [PointCardNum],
                           [PhoneNum]
                    FROM [CSCMaster].[dbo].[KickBack]
                    WHERE [CustomerCardNum] = @CustomerCardNumber
                    AND [PointCardNum] = @RewardCardNumber");

    RewardCard rewardCard = await Connection.QueryFirstOrDefaultAsync<RewardCard>(
        query.ToString(),
        new { CustomerCardNumber = custCardNumber, RewardCardNumber = rewardCardNumber },
        Transaction);

    return rewardCard != null;
  }

  public async Task<bool> IsRewardCardExistByCustomerCardAndPhoneAsync(string custCardNumber, string phoneNumber)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT [CustomerCardNum],
                           [PointCardNum],
                           [PhoneNum]
                    FROM [CSCMaster].[dbo].[KickBack]
                    WHERE [CustomerCardNum] = @CustomerCardNumber
                    AND [PhoneNum] = @PhoneNumber");

    RewardCard rewardCard = await Connection.QueryFirstOrDefaultAsync<RewardCard>(
        query.ToString(),
        new { CustomerCardNumber = custCardNumber, PhoneNumber = phoneNumber }, Transaction);

    return rewardCard != null;
  }

  public async Task<CustomerCard> GetCustomerCardByNumberAsync(string custCardNumber)
  {
    string query = @"SELECT [CardNum],
                               [CL_Code] AS Code,
                               [CardName],
                               [ExpDate],
                               [PIN],
                               [CardStatus],
                               [CreditLimiit] AS CreditLimit,
                               [Balance],
                               [AllowRedemption],
                               [TECardNumber],
                               [ProfileID],
                               [Sequence]
                        FROM [CSCMaster].[dbo].[ClientCard]
                        WHERE [CardNum] = @CustCardNumber";

    var parameters = new { CustCardNumber = custCardNumber };

    CustomerCard customerCard = await Connection.QueryFirstOrDefaultAsync<CustomerCard>(query, parameters, Transaction);
    return customerCard;
  }

  public async Task<bool> AddRewardCardAsync(RewardCard rewardCard)
  {
    string query = $@"INSERT INTO [CSCMaster].[dbo].[KickBack] (
                            [CustomerCardNum],
                            [PointCardNum],
                            [PhoneNum]
                        )
                         VALUES (
                            @CustomerCardNum,
                            @PointCardNum,
                            @PhoneNum
                         )";

    int rowsAffected = await Connection.ExecuteAsync(query, rewardCard, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateRewardCardAsync(RewardCard rewardCard)
  {
    string query = $@"UPDATE [CSCMaster].[dbo].[KickBack]
                        SET [PhoneNum] = @PhoneNum
                        WHERE [CustomerCardNum] = @CustomerCardNum
                        AND [PointCardNum] = @PointCardNum";

    int rowsAffected = await Connection.ExecuteAsync(query, rewardCard, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> IsArCustomerAsync(string code)
  {
    string query = $@"SELECT [CL_ARCUST]
                          FROM [CSCMaster].[dbo].[CLIENT]
                          WHERE [CL_CODE] = @Code";

    bool isArCustomer = await Connection.QueryFirstOrDefaultAsync<bool>(query, new { Code = code }, Transaction);
    return isArCustomer;
  }

  public async Task<int> GetNextSequenceNumberAsync(string customerCode)
  {
    var sql = @"SELECT Max([Sequence]) as MaxSequenceNumber
                FROM [CSCMaster].[dbo].[ClientCard]
                WHERE [CL_CODE] = @customerCode ";

    int? maxSequenceNumber = await Connection.QueryFirstOrDefaultAsync<int?>(sql, new { customerCode }, Transaction);
    return maxSequenceNumber == null ? 0 : maxSequenceNumber.Value + 1;
  }

  public async Task<bool> AddCustomerCardChangeAsync(ClientCardChange clientCardChange)
  {
    string sql = @"
            INSERT INTO [CSCTransfer].[dbo].[ClientCardChanges]
            (
                [CardNum],
                [OP_Status],
                [Trans_Flag]
            )
            VALUES
            (
                @CardNumber,
                @OPStatus,
                @TransFlag
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, clientCardChange, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteCustomerCardChangeAsync(ClientCardChange clientCardChange)
  {
    string sql = @"
            DELETE FROM [CSCTransfer].[dbo].[ClientCardChanges]
            WHERE [CardNum] = @CardNumber and [OP_Status] = @OPStatus and [Trans_Flag] = @TransFlag";

    int rowsAffected = await Connection.ExecuteAsync(sql, clientCardChange, Transaction);
    return rowsAffected > 0;
  }
}
