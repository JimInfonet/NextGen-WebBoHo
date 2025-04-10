namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
internal class CardProductRestrictionRepository : BaseRepository, ICardProductRestrictionRepository
{
  public CardProductRestrictionRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> AddProductRestrictionAsync(ProductRestriction productRestriction)
  {
    string sql = @$"INSERT INTO CardProductRestriction (
                      [BankCardID],
                      [CardProductCode],
                      [CardDescription],
                      [CardProductDescription],
                      [Dept],
                      [SubDept],
                      [SubDetail],
                      [StockCode],
                      [Description],
                      [CardNumber],
                      [ProfileID],
                      [Amount]
                  ) VALUES (
                      @BankCardID,
                      @CardProductCode,
                      @CardDescription,
                      COALESCE(@CardProductDescription, ''),
                      COALESCE(@Dept, '<NONE>'),
                      COALESCE(@SubDept, '<NONE>'),
                      COALESCE(@SubDetail, '<NONE>'),
                      COALESCE(@StockCode, '<NONE>'),
                      @Description,
                      @CardNumber,
                      @ProfileID,
                      @Amount)";

    int rowsAffected = await Connection.ExecuteAsync(sql, productRestriction, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteProductRestrictionAsync(ProductRestriction productRestriction)
  {
    StringBuilder sql = new();
    sql.Append(@$"DELETE FROM [CSCMaster].[dbo].[CardProductRestriction]
                      Where [BankCardID] = COALESCE(@BankCardID, '')
                        AND [CardProductCode] = COALESCE(@CardProductCode, '')
                        AND [CardProductDescription] = COALESCE(@CardProductDescription, '')
                        AND [Dept] = COALESCE(@Dept, '<NONE>')
                        AND [SubDept] = COALESCE(@SubDept, '<NONE>')
                        AND [SubDetail] = COALESCE(@SubDetail, '<NONE>')
                        AND [StockCode] = COALESCE(@StockCode, '<NONE>')
                        AND [Description] {(productRestriction.Description == null ? " is NULL " : " = @Description ")}
                        AND ([CardNumber] {(string.IsNullOrEmpty(productRestriction.CardNumber) ? " IS NULL OR CardNumber = '' " : " = @CardNumber ")})
                        AND [ProfileID]  {(productRestriction.ProfileId == null ? " is NULL " : " = @ProfileID ")} ");

    if (productRestriction.ProfileId == null)
    {
      sql.Append($@"AND [Amount] {(productRestriction.Amount == null ? " is NULL " : " = @Amount ")}");
    }

    int rowsAffected = await Connection.ExecuteAsync(sql.ToString(), productRestriction, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<ProductRestriction>> GetProductRestrictionsAsync(string? bankCardId, string? profileId)
  {
    StringBuilder sql = new();
    sql.Append(@$"SELECT cpr.BankCardID,
                             cpr.CardProductCode,
                             cpr.CardDescription,
                             cpr.CardProductDescription,
                             cpr.Dept,
                             cpr.SubDept,
                             cpr.SubDetail,
                             LTRIM(RTRIM(cpr.StockCode)) AS StockCode,
                             cpr.[Description],
                             cpr.CardNumber,
                             cpr.ProfileID,
                             cpr.Amount
                    FROM [dbo].[CardProductRestriction] AS cpr ");

    if (profileId != null)
    {
      sql.Append(@$"WHERE ProfileID = @ProfileId
                    ORDER BY ProfileID");
    }
    else if (bankCardId != null)
    {
      sql.Append(@$"WHERE BankCardID = @BankCardId 
                    ORDER BY BankCardID");
    }

    var parameter = new { BankCardId = bankCardId, ProfileId = profileId };

    IEnumerable<ProductRestriction> productRestrictions = await Connection.QueryAsync<ProductRestriction>(sql.ToString(), parameter, Transaction);
    return productRestrictions;
  }

  public async Task<IEnumerable<StockCodesForModalDto>> GetStockCodesForProductRestrictions(bool sellInactivePolicy, bool? isOrderByStockCode = false)
  {
    string sql;
    if (sellInactivePolicy)
    {
      sql = @$"select LTRIM(RTRIM(Stock_Code)) AS StockCode, descript AS Description from StockMst";
      if (isOrderByStockCode == true)
      {
        sql += $@" Order By StockMst.Stock_Code";
      }
    }
    else
    {
      sql = $@"select LTRIM(RTRIM(stockmst.Stock_Code)) AS StockCode, descript AS Description
                 from StockMst inner join Stock_Br  On Stockmst.stock_code = stock_br.stock_code  
                 Where ( Stock_br.activestock is null or stock_br.activestock =1 ) 
                 Order by StockCode";
    }

    IEnumerable<StockCodesForModalDto> stockCodes = await Connection.QueryAsync<StockCodesForModalDto>(sql, null, Transaction);
    return stockCodes;
  }

  public async Task<bool> IsProductRestrictionExist(ProductRestriction productRestriction)
  {
    string sql;

    if (productRestriction.ProfileId == null)
    {
      // Card Definitions
      sql = @$"SELECT * FROM [dbo].[CardProductRestriction] AS cpr
                    Where [BankCardID] = @BankCardID
                          AND [CardProductCode] = @CardProductCode
                          AND [CardDescription] {(productRestriction.CardDescription == null ? " is NULL " : " = @CardDescription ")}
                          AND [Dept] = COALESCE(@Dept, '<NONE>')
                          AND [SubDept] = COALESCE(@SubDept, '<NONE>')
                          AND [SubDetail] = COALESCE(@SubDetail, '<NONE>')
                          AND [StockCode] = COALESCE(@StockCode, '<NONE>')
                          AND [Description] {(productRestriction.Description == null ? " is NULL " : " = @Description ")}
                          AND [CardNumber] {(productRestriction.CardNumber == null ? " is NULL " : " = @CardNumber ")}
                          AND [ProfileID]  {(productRestriction.ProfileId == null ? " is NULL " : " = @ProfileID ")}";
    }
    else
    {
      // Card Restriction Profile
      sql = @$"SELECT * FROM CardProductRestriction 
                        WHERE ProfileID = @ProfileId
                          AND [Dept] = COALESCE(@Dept, '<NONE>')
                          AND [SubDept] = COALESCE(@SubDept, '<NONE>')
                          AND [SubDetail] = COALESCE(@SubDetail, '<NONE>')
                          AND [StockCode] = COALESCE(@StockCode, '<NONE>');";
    }

    var result = await Connection.QueryAsync<ProductRestriction>(sql, productRestriction, Transaction);
    return result.Any();
  }

  public async Task<bool> UpdateProductRestriction(ProductRestriction productRestriction)
  {
    string sql;
    if (productRestriction.ProfileId == null)
    {
      sql = @$"UPDATE [CSCMaster].[dbo].[CardProductRestriction] SET
                            [BankCardID] = @BankCardID,
                            [CardProductCode] = @CardProductCode,
                            [CardDescription] = COALESCE(@CardDescription, ''),
                            [CardProductDescription] = COALESCE(@CardProductDescription, ''),
                            [Dept] = COALESCE(@Dept, '<NONE>'),
                            [SubDept] = COALESCE(@SubDept, '<NONE>'),
                            [SubDetail] = COALESCE(@SubDetail, '<NONE>'),
                            [StockCode] = COALESCE(@StockCode, '<NONE>'),
                            [Description] = @Description,
                            [CardNumber] = @CardNumber,
                            [ProfileID] = @ProfileID,
                            [Amount] = @Amount
                      Where [BankCardID] = @PreBankCardID
                        AND [CardProductCode] = @PreCardProductCode
                        AND [CardDescription] = COALESCE(@PreCardDescription, '')
                        AND [CardProductDescription] = COALESCE(@PreCardProductDescription, '')
                        AND [Dept] = COALESCE(@PreDept, '<NONE>')
                        AND [SubDept] = COALESCE(@PreSubDept, '<NONE>')
                        AND [SubDetail] = COALESCE(@PreSubDetail, '<NONE>')
                        AND [StockCode] = COALESCE(@PreStockCode, '<NONE>')
                        AND [Description] {(productRestriction.PreDescription == null ? " is NULL " : " = @PreDescription ")}
                        AND [CardNumber] {(productRestriction.PreCardNumber == null ? " is NULL " : " = @PreCardNumber ")}
                        AND [ProfileID]  {(productRestriction.PreProfileId == null ? " is NULL " : " = @PreProfileID ")}
                        AND [Amount] {(productRestriction.PreAmount == null ? " is NULL " : " = @PreAmount ")}";
    }
    else
    {
      sql = @$"UPDATE [CSCMaster].[dbo].[CardProductRestriction] SET
                            [Dept] = COALESCE(@Dept, '<NONE>'),
                            [SubDept] =  COALESCE(@SubDept, '<NONE>'),
                            [SubDetail] = COALESCE(@SubDetail, '<NONE>'),
                            [StockCode] = COALESCE(@StockCode, '<NONE>'),
                            [Description] = COALESCE(@Description, '<NONE>')
                      Where [ProfileID] = @ProfileId
                        AND [Dept] = COALESCE(@PreDept, '<NONE>')
                        AND [SubDept] = COALESCE(@PreSubDept, '<NONE>')
                        AND [SubDetail] = COALESCE(@PreSubDetail, '<NONE>')
                        AND [StockCode] = COALESCE(@PreStockCode, '<NONE>')
                        AND [Description] = COALESCE(@PreDescription, '<NONE>')";
    }

    int rowsAffected = await Connection.ExecuteAsync(sql, productRestriction, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteCardProductRestrictionByBankCardIDAsync(string bankCardID)
  {
    const string sql = @"DELETE from [CSCMaster].[dbo].[CardProductRestriction]
                        WHERE BankCardID = @BankCardID";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@BankCardID", SqlDbType.VarChar, 10).Value = bankCardID;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }
}
