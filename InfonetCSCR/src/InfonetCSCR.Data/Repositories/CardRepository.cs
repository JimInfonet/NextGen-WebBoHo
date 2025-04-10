namespace InfonetCSCR.Data.Repositories;

public class CardRepository : BaseRepository, ICardRepository
{
  public CardRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> CheckCardCodeExistAsync(string cardCode)
  {
    StringBuilder query = new();
    query.Append($@"SELECT CardCode FROM [CSCMaster].[dbo].[Cards] WHERE CardCode = @CardCode;");

    var parameters = new { CardCode = cardCode };
    string cardCodeData = await Connection.QuerySingleOrDefaultAsync<string>(query.ToString(), parameters, Transaction);
    return (cardCodeData != null);
  }

  public async Task<IEnumerable<Card>> GetCardsAsync()
  {
    StringBuilder query = new();
    query.Append($@"SELECT
                       [CardCode] as CardCode,
                       [Description] as Description,
                       [CardType] as CardType,
                       [VerifyCheckDigit] as VerifyCheckDigit,
                       [Encode] as Encode,
                       [Negative] as Negative,
                       [Positive] as Positive,
                       [Expiry] as Expiry,
                       [LanguageDigit] as LanguageDigit,
                       [AllowPayment] as AllowPayment,
                       [MinLength] as MinLength,
                       [MaxLength] as MaxLength,
                       [CheckMask] as CheckMask,
                       [UsageIDPosition] as UsageIDPosition,
                       [UsageIDLength] as UsageIDLength,
                       [PromptIDPosition] as PromptIDPosition,
                       [PromptIDLength] as PromptIDLength,
                       [defaultpromptcode] as DefaultPromptCode,
                       [defaultusagecode] as DefaultUsageCode,
                       [Userenteredrestriction] as UserEnteredRestriction,
                       [ProducttPerTransaction] as ProductPerTransaction,
                       [ServiceTypeCode] as ServiceTypeCode,
                       [VerifyThirdPartySw] as VerifyThirdPartySw,
                       [VerifyCardNumber] as VerifyCardNumber,
                       [GiftType] as GiftType,
                       [EncryptCardNumber] as EncryptCardNumber
                FROM [CSCMaster].[dbo].[Cards]
                ");

    IEnumerable<Card> card = await Connection.QueryAsync<Card>(query.ToString(), null, Transaction);
    return card;
  }

  public async Task<Card> GetCardDefinitionByCardCodeAsync(string cardCode)
  {
    StringBuilder query = new();
    query.Append($@"SELECT
                       [CardCode] as CardCode,
                       [Description] as Description,
                       [CardType] as CardType,
                       [VerifyCheckDigit] as VerifyCheckDigit,
                       [Encode] as Encode,
                       [Negative] as Negative,
                       [Positive] as Positive,
                       [Expiry] as Expiry,
                       [LanguageDigit] as LanguageDigit,
                       [AllowPayment] as AllowPayment,
                       [MinLength] as MinLength,
                       [MaxLength] as MaxLength,
                       [CheckMask] as CheckMask,
                       [UsageIDPosition] as UsageIDPosition,
                       [UsageIDLength] as UsageIDLength,
                       [PromptIDPosition] as PromptIDPosition,
                       [PromptIDLength] as PromptIDLength,
                       [defaultpromptcode] as DefaultPromptCode,
                       [defaultusagecode] as DefaultUsageCode,
                       [Userenteredrestriction] as UserEnteredRestriction,
                       [ProducttPerTransaction] as ProductPerTransaction,
                       [ServiceTypeCode] as ServiceTypeCode,
                       [VerifyThirdPartySw] as VerifyThirdPartySw,
                       [VerifyCardNumber] as VerifyCardNumber,
                       [GiftType] as GiftType,
                       [EncryptCardNumber] as EncryptCardNumber
                FROM [CSCMaster].[dbo].[Cards]
                WHERE CardCode = @CardCode;
                ");

    var parameters = new { CardCode = cardCode };
    Card card = await Connection.QuerySingleOrDefaultAsync<Card>(query.ToString(), parameters, Transaction);
    return card;
  }

  public async Task<bool> AddCardAsync(Card card)
  {
    string sql = $@"INSERT INTO [dbo].[Cards] (
                      [CardCode],
                      [Description],
                      [CardType],
                      [VerifyCheckDigit],
                      [Encode],
                      [Negative],
                      [Positive],
                      [Expiry],
                      [LanguageDigit],
                      [AllowPayment],
                      [MinLength],
                      [MaxLength],
                      [CheckMask],
                      [UsageIDPosition],
                      [UsageIDLength],
                      [PromptIDPosition],
                      [PromptIDLength],
                      [defaultpromptcode],
                      [defaultusagecode],
                      [Userenteredrestriction],
                      [ProducttPerTransaction],
                      [ServiceTypeCode],
                      [VerifyThirdPartySw],
                      [VerifyCardNumber],
                      [GiftType],
                      [EncryptCardNumber]
                    ) VALUES (
                          @CardCode,
                          @Description,
                          @CardType,
                          @VerifyCheckDigit,
                          @Encode,
                          @Negative,
                          @Positive,
                          @Expiry,
                          @LanguageDigit,
                          @AllowPayment,
                          @MinLength,
                          @MaxLength,
                          @CheckMask,
                          @UsageIDPosition,
                          @UsageIDLength,
                          @PromptIDPosition,
                          @PromptIDLength,
                          @DefaultPromptCode,
                          @DefaultUsageCode,
                          @UserEnteredRestriction,
                          @ProductPerTransaction,
                          @ServiceTypeCode,
                          @VerifyThirdPartySw,
                          @VerifyCardNumber,
                          @GiftType,
                          @EncryptCardNumber
                          );";

    int rowsAffected = await Connection.ExecuteAsync(sql, card, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateCardAsync(Card card)
  {
    string sql = $@"UPDATE [dbo].[Cards]
                    SET
                       [CardType] = @CardType,
                       [VerifyCheckDigit] = @VerifyCheckDigit,
                       [Encode] = @Encode,
                       [Negative] = @Negative,
                       [Positive] = @Positive,
                       [Expiry] = @Expiry,
                       [LanguageDigit] = @LanguageDigit,
                       [AllowPayment] = @AllowPayment,
                       [MinLength] = @MinLength,
                       [MaxLength] = @MaxLength,
                       [CheckMask] = @CheckMask,
                       [UsageIDPosition] = @UsageIDPosition,
                       [UsageIDLength] = @UsageIDLength,
                       [PromptIDPosition] = @PromptIDPosition,
                       [PromptIDLength] = @PromptIDLength,
                       [defaultpromptcode] = @DefaultPromptCode,
                       [defaultusagecode] = @DefaultUsageCode,
                       [Userenteredrestriction] = @UserEnteredRestriction,
                       [ProducttPerTransaction] = @ProductPerTransaction,
                       [ServiceTypeCode] = @ServiceTypeCode,
                       [VerifyThirdPartySw] = @VerifyThirdPartySw,
                       [VerifyCardNumber] = @VerifyCardNumber,
                       [GiftType] = @GiftType,
                       [EncryptCardNumber] = @EncryptCardNumber
                    WHERE [CardCode] = @CardCode;";

    int rowsAffected = await Connection.ExecuteAsync(sql, card, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteCardByCardCodeAsync(string cardCode)
  {
    const string sql = @"DELETE from [CSCMaster].[dbo].[Cards]
                        WHERE CardCode = @CardCode";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { CardCode = cardCode }, Transaction);
    return rowsAffected > 0;
  }
}
