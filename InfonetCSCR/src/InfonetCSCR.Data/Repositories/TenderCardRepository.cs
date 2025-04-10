namespace InfonetCSCR.Data.Repositories;

public class TenderCardRepository : BaseRepository, ITenderCardRepository
{
  public TenderCardRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> CheckTendCodeExistAsync(string tendCode)
  {
    StringBuilder query = new();
    query.Append($@"SELECT TENDCODE FROM [CSCMaster].[dbo].[TendCard] WHERE TENDCODE = @TendCode;");

    var parameters = new { TendCode = tendCode };
    string tendCodeData = await Connection.QuerySingleOrDefaultAsync<string>(query.ToString(), parameters, Transaction);
    return (tendCodeData != null);
  }

  public async Task<TenderCard> GetTenderCardDataByCardCodeAsync(string cardCode)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT 
                      [TENDCODE] as TendCode,
                      [ReportGroup] as ReportGroup,
                      [BankCardID] as BankCardId,
                      [Call] as Call,
                      [Signature] as Signature,
                      [DiscountRate] as DiscountRate,
                      [DiscountType] as DiscountType,
                      [RefundAllowed] as RefundAllowed,
                      [PurchaseLimit] as PurchaseLimit,
                      [FloorLimit] as FloorLimit,
                      [PrintCopies] as PrintCopies,
                      [ReceiptTotalText] as ReceiptTotalText,
                      [LimitToSale] as LimitToSale,
                      [MaxCashBack] as MaxCashBack,
                      [CardCode] as CardCode,
                      [AllowPayPump] as AllowPayPump,
                      [CardProductRestrict] as CardProductRestrict,
                      [PrintVechicleNo] as PrintVehicleNo,
                      [PrintDriverNo] as PrintDriverNo,
                      [PrintIdentificationNo] as PrintIdentificationNo,
                      [PrintOdometer] as PrintOdometer,
                      [PrintUsage] as PrintUsage,
                      [OptDataProfileID] as OptionalDataProfilID,
                      [Trans_Fee_Amt] as TransactionFeeAmount,
                      [Trans_Fee_Perc] as TransactionFeePercentage
                  FROM [CSCMaster].[dbo].[TendCard]
                  WHERE CardCode = @CardCode");

    var parameters = new { CardCode = cardCode };
    TenderCard tenderCard = await Connection.QueryFirstOrDefaultAsync<TenderCard>(query.ToString(), parameters, Transaction);
    return tenderCard;
  }

  public async Task<bool> AddTenderCardAsync(TenderCard tenderCard)
  {
    string sql = $@"INSERT INTO [dbo].[TendCard] (
                    [TENDCODE], 
                    [ReportGroup], 
                    [BankCardID], 
                    [Call], 
                    [Signature], 
                    [DiscountRate], 
                    [DiscountType], 
                    [RefundAllowed], 
                    [PurchaseLimit], 
                    [FloorLimit], 
                    [PrintCopies], 
                    [ReceiptTotalText], 
                    [LimitToSale], 
                    [MaxCashBack], 
                    [CardCode], 
                    [AllowPayPump], 
                    [CardProductRestrict], 
                    [PrintVechicleNo], 
                    [PrintDriverNo], 
                    [PrintIdentificationNo], 
                    [PrintOdometer], 
                    [PrintUsage], 
                    [OptDataProfileID], 
                    [Trans_Fee_Amt], 
                    [Trans_Fee_Perc]
                  ) VALUES (
                    @TendCode, 
                    @ReportGroup, 
                    @BankCardID, 
                    @Call, 
                    @Signature, 
                    @DiscountRate, 
                    @DiscountType, 
                    @RefundAllowed, 
                    @PurchaseLimit, 
                    @FloorLimit, 
                    @PrintCopies, 
                    @ReceiptTotalText, 
                    @LimitToSale, 
                    @MaxCashBack, 
                    @CardCode, 
                    @AllowPayPump, 
                    @CardProductRestrict, 
                    @PrintVechicleNo, 
                    @PrintDriverNo, 
                    @PrintIdentificationNo, 
                    @PrintOdometer, 
                    @PrintUsage, 
                    @OptionalDataProfilID, 
                    @TransactionFeeAmount, 
                    @TransactionFeePercentage
                  );";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@TendCode", SqlDbType.NVarChar, 10).Value = tenderCard.TendCode ?? (object)DBNull.Value;
    cmd.Parameters.Add("@ReportGroup", SqlDbType.TinyInt).Value = tenderCard.ReportGroup ?? 0;
    cmd.Parameters.Add("@BankCardID", SqlDbType.NVarChar, 50).Value = tenderCard.BankCardID ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Call", SqlDbType.Bit).Value = tenderCard.Call;
    cmd.Parameters.Add("@Signature", SqlDbType.Bit).Value = tenderCard.Signature;
    cmd.Parameters.Add("@DiscountRate", SqlDbType.SmallMoney).Value = tenderCard.DiscountRate ?? 0;
    cmd.Parameters.Add("@DiscountType", SqlDbType.NVarChar, 1).Value = tenderCard.DiscountType ?? "";
    cmd.Parameters.Add("@RefundAllowed", SqlDbType.Bit).Value = tenderCard.RefundAllowed;
    cmd.Parameters.Add("@PurchaseLimit", SqlDbType.SmallMoney).Value = tenderCard.PurchaseLimit ?? 0;
    cmd.Parameters.Add("@FloorLimit", SqlDbType.SmallMoney).Value = tenderCard.FloorLimit ?? 0;
    cmd.Parameters.Add("@PrintCopies", SqlDbType.TinyInt).Value = tenderCard.PrintCopies ?? 0;
    cmd.Parameters.Add("@ReceiptTotalText", SqlDbType.NVarChar, 50).Value = tenderCard.ReceiptTotalText ?? "";
    cmd.Parameters.Add("@LimitToSale", SqlDbType.Bit).Value = tenderCard.LimitToSale;
    cmd.Parameters.Add("@MaxCashBack", SqlDbType.SmallMoney).Value = tenderCard.MaxCashBack ?? 0;
    cmd.Parameters.Add("@CardCode", SqlDbType.SmallInt).Value = tenderCard.CardCode ?? (object)DBNull.Value;
    cmd.Parameters.Add("@AllowPayPump", SqlDbType.Bit).Value = tenderCard.AllowPayPump ?? false;
    cmd.Parameters.Add("@CardProductRestrict", SqlDbType.Bit).Value = tenderCard.CardProductRestrict ?? false;
    cmd.Parameters.Add("@PrintVechicleNo", SqlDbType.Bit).Value = tenderCard.PrintVechicleNo ?? false;
    cmd.Parameters.Add("@PrintDriverNo", SqlDbType.Bit).Value = tenderCard.PrintDriverNo ?? false;
    cmd.Parameters.Add("@PrintIdentificationNo", SqlDbType.Bit).Value = tenderCard.PrintIdentificationNo ?? false;
    cmd.Parameters.Add("@PrintOdometer", SqlDbType.SmallInt).Value = tenderCard.PrintOdometer ?? false;
    cmd.Parameters.Add("@PrintUsage", SqlDbType.Bit).Value = tenderCard.PrintUsage ?? false;
    cmd.Parameters.Add("@OptionalDataProfilID", SqlDbType.NVarChar, 10).Value = tenderCard.OptionalDataProfilID ?? "";
    cmd.Parameters.Add("@TransactionFeeAmount", SqlDbType.SmallMoney).Value = tenderCard.TransactionFeeAmount ?? 0;
    cmd.Parameters.Add("@TransactionFeePercentage", SqlDbType.SmallMoney).Value = tenderCard.TransactionFeePercentage ?? 0;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> UpdateCardCodeOnTenderCardAsync(string tendCode, string cardCode)
  {
    string sql = $@"UPDATE [dbo].[TendCard]
                    SET
                      [CardCode] = @CardCode
                     WHERE [TENDCODE] = @TendCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@TendCode", SqlDbType.NVarChar, 10).Value = tendCode;
    cmd.Parameters.Add("@CardCode", SqlDbType.SmallInt).Value = cardCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> UpdateTenderCardAsync(TenderCard tenderCard)
  {
    string sql = $@"UPDATE [dbo].[TendCard]
                    SET
                      [ReportGroup] = @ReportGroup, 
                      [BankCardID] = @BankCardID, 
                      [Call] = @Call, 
                      [Signature] = @Signature, 
                      [DiscountRate] = @DiscountRate, 
                      [DiscountType] = @DiscountType, 
                      [RefundAllowed] = @RefundAllowed, 
                      [PurchaseLimit] = @PurchaseLimit, 
                      [FloorLimit] = @FloorLimit, 
                      [PrintCopies] = @PrintCopies, 
                      [ReceiptTotalText] = @ReceiptTotalText, 
                      [LimitToSale] = @LimitToSale, 
                      [MaxCashBack] = @MaxCashBack, 
                      [CardCode] = @CardCode, 
                      [AllowPayPump] = @AllowPayPump, 
                      [CardProductRestrict] = @CardProductRestrict, 
                      [PrintVechicleNo] = @PrintVechicleNo, 
                      [PrintDriverNo] = @PrintDriverNo, 
                      [PrintIdentificationNo] = @PrintIdentificationNo, 
                      [PrintOdometer] = @PrintOdometer, 
                      [PrintUsage] = @PrintUsage, 
                      [OptDataProfileID] = @OptionalDataProfilID, 
                      [Trans_Fee_Amt] = @TransactionFeeAmount, 
                      [Trans_Fee_Perc] = @TransactionFeePercentage 
                     WHERE [TENDCODE] = @TendCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@TendCode", SqlDbType.NVarChar, 10).Value = tenderCard.TendCode ?? (object)DBNull.Value;
    cmd.Parameters.Add("@ReportGroup", SqlDbType.TinyInt).Value = tenderCard.ReportGroup ?? 0;
    cmd.Parameters.Add("@BankCardID", SqlDbType.NVarChar, 50).Value = tenderCard.BankCardID ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Call", SqlDbType.Bit).Value = tenderCard.Call;
    cmd.Parameters.Add("@Signature", SqlDbType.Bit).Value = tenderCard.Signature;
    cmd.Parameters.Add("@DiscountRate", SqlDbType.SmallMoney).Value = tenderCard.DiscountRate ?? 0;
    cmd.Parameters.Add("@DiscountType", SqlDbType.NVarChar, 1).Value = tenderCard.DiscountType ?? "";
    cmd.Parameters.Add("@RefundAllowed", SqlDbType.Bit).Value = tenderCard.RefundAllowed;
    cmd.Parameters.Add("@PurchaseLimit", SqlDbType.SmallMoney).Value = tenderCard.PurchaseLimit ?? 0;
    cmd.Parameters.Add("@FloorLimit", SqlDbType.SmallMoney).Value = tenderCard.FloorLimit ?? 0;
    cmd.Parameters.Add("@PrintCopies", SqlDbType.TinyInt).Value = tenderCard.PrintCopies ?? 0;
    cmd.Parameters.Add("@ReceiptTotalText", SqlDbType.NVarChar, 50).Value = tenderCard.ReceiptTotalText ?? "";
    cmd.Parameters.Add("@LimitToSale", SqlDbType.Bit).Value = tenderCard.LimitToSale;
    cmd.Parameters.Add("@MaxCashBack", SqlDbType.SmallMoney).Value = tenderCard.MaxCashBack ?? 0;
    cmd.Parameters.Add("@CardCode", SqlDbType.SmallInt).Value = tenderCard.CardCode ?? (object)DBNull.Value;
    cmd.Parameters.Add("@AllowPayPump", SqlDbType.Bit).Value = tenderCard.AllowPayPump ?? (object)DBNull.Value;
    cmd.Parameters.Add("@CardProductRestrict", SqlDbType.Bit).Value = tenderCard.CardProductRestrict ?? (object)DBNull.Value;
    cmd.Parameters.Add("@PrintVechicleNo", SqlDbType.Bit).Value = tenderCard.PrintVechicleNo ?? false;
    cmd.Parameters.Add("@PrintDriverNo", SqlDbType.Bit).Value = tenderCard.PrintDriverNo ?? false;
    cmd.Parameters.Add("@PrintIdentificationNo", SqlDbType.Bit).Value = tenderCard.PrintIdentificationNo ?? false;
    cmd.Parameters.Add("@PrintOdometer", SqlDbType.SmallInt).Value = tenderCard.PrintOdometer ?? false;
    cmd.Parameters.Add("@PrintUsage", SqlDbType.Bit).Value = tenderCard.PrintUsage ?? false;
    cmd.Parameters.Add("@OptionalDataProfilID", SqlDbType.NVarChar, 10).Value = tenderCard.OptionalDataProfilID ?? "";
    cmd.Parameters.Add("@TransactionFeeAmount", SqlDbType.SmallMoney).Value = tenderCard.TransactionFeeAmount ?? 0;
    cmd.Parameters.Add("@TransactionFeePercentage", SqlDbType.SmallMoney).Value = tenderCard.TransactionFeePercentage ?? 0;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteTenderCardByTendCodeAsync(string tendCode)
  {
    const string sql = @"DELETE from [CSCMaster].[dbo].[TendCard]
                        WHERE TENDCODE = @TendCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@TendCode", SqlDbType.VarChar, 10).Value = tendCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }
}
