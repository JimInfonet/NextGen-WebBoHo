namespace InfonetCSCR.Data.Repositories;

public class PromotionalItemsRepository : BaseRepository, IPromotionalItemsRepository
{
  private readonly IApplicationConfig _config;
  public PromotionalItemsRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
  {
    _config = config;
  }

  public async Task<IEnumerable<PromoHeader>> GetPromoHeadersAsync()
  {
    string sql = @"SELECT PromoId, Description, StartDate, EndDate
                   FROM [CSCMaster].[dbo].[PromoHeader] 
                   ORDER BY PromoId ";

    var promoHeaders = await Connection.QueryAsync<PromoHeader>(sql, null, Transaction);
    return promoHeaders;
  }

  public async Task<PromoHeader> GetPromoHeaderAsync(string PromoId)
  {
    string sql = @"SELECT PromoId, Description, Price, StartDate, EndDate, DiscType, Items, Day, PrType
                   FROM [CSCMaster].[dbo].[PromoHeader] 
                   WHERE  PromoId = @PromoID";

    var parameters = new
    {
      PromoID = PromoId
    };

    var promoHeader = await Connection.QueryFirstOrDefaultAsync<PromoHeader>(sql, parameters, Transaction);

    return promoHeader;
  }

  public async Task<IEnumerable<PromoDetail>> GetPromoDetailAsync(string PromoId)
  {
    string sql = @"SELECT DISTINCT 
                      PromoDetail.PromoID, LTRIM(RTRIM(PromoDetail.Stock_Code)) AS StockCode,
                      PromoDetail.Dept, PromoDetail.SUB_DEPT AS SubDept,  
                      PromoDetail.SubDetail, PromoDetail.Link, 
                      PromoDetail.Qty,PromoDetail.Amount, 
                      STOCKMST.DESCRIPT AS Descript, Dept.DEPT_NAME AS DeptName, 
                      SubDept.SUB_NAME AS SubName , SubDetail.SubDetail_Name as SubDetailName 

                  FROM SubDetail RIGHT JOIN (SubDept RIGHT JOIN 

                  (STOCKMST RIGHT JOIN (Dept RIGHT JOIN PromoDetail ON 

                      Dept.DEPT =PromoDetail.Dept) ON 
                      STOCKMST.STOCK_CODE =PromoDetail.Stock_Code) ON 
                      (SubDept.DEPT =PromoDetail.Dept) AND 
                      (SubDept.SUB_DEPT =PromoDetail.SUB_DEPT)) ON 
                      ((SubDetail.Dept =PromoDetail.Dept) AND 
                      (SubDetail.SubDept =PromoDetail.SUB_DEPT)) AND 
                      (SubDetail.SubDetail =PromoDetail.SubDetail)
                      WHERE PromoDetail.PromoID=@PromoId";

    var parameters = new
    {
      PromoID = PromoId
    };

    var promoDetail = await Connection.QueryAsync<PromoDetail>(sql, parameters, Transaction);

    return promoDetail;
  }

  public async Task<IEnumerable<string>> GetAllPromoIdsAsync()
  {
    string sql = @"SELECT PromoId
                   FROM [CSCMaster].[dbo].[PromoHeader]";

    var promoHeaders = await Connection.QueryAsync<string>(sql, null, Transaction);
    return promoHeaders;
  }

  public async Task<int> IsPromoIdExist(string PromoId)
  {
    string sql = @"SELECT COUNT(*)
                    FROM [CSCMaster].[dbo].[PromoHeader]
                    WHERE PromoId = @PromoId";

    var parameters = new
    {
      PromoID = PromoId
    };

    var promoIdCount = await Connection.ExecuteScalarAsync<int>(sql, parameters, Transaction);
    return promoIdCount;
  }

  public async Task<bool> AddPromotionalItemHeaderAsync(PromoHeader promoHeader)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[PromoHeader] (
                        [PromoID],
                        [Description],
                        [Price],
                        [StartDate],
                        [EndDate],
                        [DiscType],
                        [Items],
                        [Day],
                        [PrType]
                    )
                    VALUES (
                        @PromoId, 
                        @Description, 
                        @Price, 
                        @StartDate, 
                        @EndDate, 
                        @DiscType, 
                        @Items, 
                        @Day, 
                        @PrType
                    )";

    int rowsAffected = await Connection.ExecuteAsync(sql, promoHeader, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdatePromotionalItemHeaderAsync(PromoHeader promoHeader)
  {
    string sql = $@"UPDATE [CSCMaster].[dbo].[PromoHeader]
                        SET
                            [PromoID] = @PromoId,
                            [Description] = @Description,
                            [Price] = @Price,
                            [StartDate] = @StartDate,
                            [EndDate] = @EndDate,
                            [DiscType] = @DiscType,
                            [Items] = @Items,
                            [Day] = @Day,
                            [PrType] = @PrType
                        WHERE [PromoID] = @PromoId";

    var parameters = new
    {
      PromoID = promoHeader.PromoId,
      Description = promoHeader.Description,
      Price = promoHeader.Price,
      StartDate = promoHeader.StartDate,
      EndDate = promoHeader.EndDate,
      DiscType = promoHeader.DiscType,
      Items = promoHeader.Items,
      Day = promoHeader.Day,
      PrType = promoHeader.PrType
    };
    int rowsAffected = await Connection.ExecuteAsync(sql, promoHeader, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> AddPromotionalItemDetailsAsync(IEnumerable<PromoDetail> promoDetails)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[PromoDetail] (
                        [PromoID],
                        [Stock_Code],
                        [Dept],
                        [SUB_DEPT],
                        [SubDetail],
                        [Link],
                        [Qty],
                        [Amount]
                    )
                    VALUES (
                        @PromoID, 
                        @StockCode, 
                        @Dept, 
                        @SubDept,  
                        @SubDetail, 
                        @Link, 
                        @Qty, 
                        @Amount
                    )";

    int rowsAffected = await Connection.ExecuteAsync(sql, promoDetails, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeletePromotionalItemDetailsAsync(string promoId)
  {
    const string sql = @"Delete From [CSCMaster].[dbo].[PromoDetail]
                         where PromoID = @PromoID";

    var parameters = new
    {
      PromoID = promoId
    };

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);

    int rowAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeletePromotionalItemHeaderAsync(string promoId)
  {
    const string sql = @"Delete From [CSCMaster].[dbo].[PromoHeader]
                         where PromoID = @PromoID";

    var parameters = new
    {
      PromoID = promoId
    };

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);

    int rowAffected = await Connection.ExecuteAsync(sql, parameters, Transaction);

    return await Task.FromResult(rowAffected > 0);
  }
}
