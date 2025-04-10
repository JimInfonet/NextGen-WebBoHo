namespace InfonetCSCR.Data.Repositories;

public class TendersRepository : BaseRepository, ITendersRepository
{
  public TendersRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<Tender>> GetTendersWithCardCodeAsync()
  {
    string sql = @$"SELECT  Tendmast.TendCode as [Code],
                            Tendmast.ShortDesc as [ShortName],                   
                            Tendmast.TendDesc  as [FullName],
                            Tendmast.TendClass as [Class], 
                            Tendmast.Exchange as [ExchangeRate], 
                            Tendmast.DisplaySeq as [Sequence],
                            TendCard.CardCode as [CardCode],
                            Tendmast.EXACTCHG AS ExactChange,
                            Tendmast.GIVECHG AS GiveChange,
                            Tendmast.AcceptAsPayment AS AcceptAsPayment,
                            Tendmast.GIVEASREF AS GiveAsRefund,
                            Tendmast.CANADJUST AS CanAdjust,
                            Tendmast.BANKABLE AS Bankable,
                            Tendmast.IncludeInClose AS IncludeInClose,
                            Tendmast.Put_In_Drawer AS PutInDrawer,
                            Tendmast.Signature AS Signature,
                            Tendmast.Inactive AS Inactive,
                            Tendmast.MINAMOUNT AS MinAmount,
                            Tendmast.MAXAMOUNT AS MaxAmount,
                            Tendmast.SMALLUNIT AS SmallestUnit,
                            Tendmast.PrintCopies AS PrintCopies,
                            Tendmast.INSTANCES AS Instances,
                            Tendmast.PayoutCode AS PayoutCode
                        FROM TendMast LEFT JOIN TendCard 
                        ON TendMast.TendCode = TendCard.TendCode 
                        ORDER BY Tendmast.DisplaySeq";

    IEnumerable<Tender> tenders = await Connection.QueryAsync<Tender>(sql, null, Transaction);
    return tenders;
  }

  public async Task<Tender> GetSingleTenderAsync(string tendCode)
  {
    string sql = @$"SELECT  Tendmast.TendCode as [Code],
                            Tendmast.ShortDesc as [ShortName],                   
                            Tendmast.TendDesc  as [FullName],
                            Tendmast.TendClass as [Class], 
                            Tendmast.Exchange as [ExchangeRate], 
                            Tendmast.DisplaySeq as [Sequence],
                            TendCard.CardCode as [CardCode],
                            Tendmast.EXACTCHG AS ExactChange,
                            Tendmast.GIVECHG AS GiveChange,
                            Tendmast.AcceptAsPayment AS AcceptAsPayment,
                            Tendmast.GIVEASREF AS GiveAsRefund,
                            Tendmast.CANADJUST AS CanAdjust,
                            Tendmast.BANKABLE AS Bankable,
                            Tendmast.IncludeInClose AS IncludeInClose,
                            Tendmast.Put_In_Drawer AS PutInDrawer,
                            Tendmast.Signature AS Signature,
                            Tendmast.Inactive AS Inactive,
                            Tendmast.MINAMOUNT AS MinAmount,
                            Tendmast.MAXAMOUNT AS MaxAmount,
                            Tendmast.SMALLUNIT AS SmallestUnit,
                            Tendmast.PrintCopies AS PrintCopies,
                            Tendmast.INSTANCES AS Instances,
                            Tendmast.PayoutCode AS PayoutCode
                        FROM TendMast LEFT JOIN TendCard 
                        ON TendMast.TendCode = TendCard.TendCode 
                        Where Tendmast.TendCode = @TendCode";

    var parameters = new { TendCode = tendCode };
    return await Connection.QueryFirstOrDefaultAsync<Tender>(sql, parameters, Transaction);
  }

  public async Task<IEnumerable<Payout>> GetPayoutAsync()
  {
    string sql = @$"SELECT Code, Reason FROM Reasons WHERE Type = 'P'";
    IEnumerable<Payout> payouts = await Connection.QueryAsync<Payout>(sql, null, Transaction);
    return payouts;
  }

  public async Task<IEnumerable<TenderClass>> GetTenderClassesAsync()
  {
    string sql = @$"SELECT * FROM TendClas";
    IEnumerable<TenderClass> tenderClasses = await Connection.QueryAsync<TenderClass>(sql, null, Transaction);
    return tenderClasses;
  }

  public async Task<bool> UpdateTenderAsync(Tender tender)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[TendMast] SET
                           DisplaySeq = @Sequence,
                           Exchange = @ExchangeRate,
                           EXACTCHG = @ExactChange,
                           GIVECHG = @GiveChange,
                           AcceptAsPayment = @AcceptAsPayment,
                           GIVEASREF = @GiveAsRefund,
                           CANADJUST = @CanAdjust,
                           BANKABLE = @Bankable,
                           IncludeInClose = @IncludeInClose,
                           Put_In_Drawer = @PutInDrawer,
                           Signature = @Signature,
                           Inactive = @Inactive,
                           MINAMOUNT = @MinAmount,
                           MAXAMOUNT = @MaxAmount,
                           SMALLUNIT = @SmallestUnit,
                           PrintCopies = @PrintCopies,
                           INSTANCES = @Instances,
                           PayoutCode = @PayoutCode,
                           TENDCLASS = @TendClass
                           WHERE Tendcode = @TendCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@TendCode", SqlDbType.VarChar, 10).Value = tender.Code;
    cmd.Parameters.Add("@TendClass", SqlDbType.VarChar, 10).Value = tender.Class ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Sequence", SqlDbType.TinyInt).Value = tender.Sequence ?? 1;
    cmd.Parameters.Add("@ExactChange", SqlDbType.Bit).Value = tender.ExactChange ?? false;
    cmd.Parameters.Add("@GiveChange", SqlDbType.Bit).Value = tender.GiveChange ?? false;
    cmd.Parameters.Add("@AcceptAsPayment", SqlDbType.Bit).Value = tender.AcceptAsPayment ?? false;
    cmd.Parameters.Add("@GiveAsRefund", SqlDbType.Bit).Value = tender.GiveAsRefund ?? false;
    cmd.Parameters.Add("@CanAdjust", SqlDbType.Bit).Value = tender.CanAdjust ?? false;
    cmd.Parameters.Add("@MaxAmount", SqlDbType.SmallMoney).Value = tender.MaxAmount ?? 0;
    cmd.Parameters.Add("@MinAmount", SqlDbType.SmallMoney).Value = tender.MinAmount ?? 0;
    cmd.Parameters.Add("@ExchangeRate", SqlDbType.SmallMoney).Value = tender.ExchangeRate ?? 0;
    cmd.Parameters.Add("@SmallestUnit", SqlDbType.SmallMoney).Value = tender.SmallestUnit ?? (decimal)0.01;
    cmd.Parameters.Add("@Bankable", SqlDbType.Bit).Value = tender.Bankable ?? false;
    cmd.Parameters.Add("@IncludeInClose", SqlDbType.Bit).Value = tender.IncludeInClose ?? false;
    cmd.Parameters.Add("@Instances", SqlDbType.TinyInt).Value = tender.Instances ?? 0;
    cmd.Parameters.Add("@PutInDrawer", SqlDbType.Bit).Value = tender.PutInDrawer ?? false;
    cmd.Parameters.Add("@PrintCopies", SqlDbType.SmallMoney).Value = tender.PrintCopies ?? 1;
    cmd.Parameters.Add("@Signature", SqlDbType.Bit).Value = tender.Signature ?? false;
    cmd.Parameters.Add("@PayoutCode", SqlDbType.VarChar, 4).Value = tender.PayoutCode ?? (object)DBNull.Value;
    cmd.Parameters.Add("@Inactive", SqlDbType.Bit).Value = tender.Inactive ?? false;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> AddTenderAsync(Tender tender)
  {
    string sql = @$"INSERT INTO [CSCMaster].[dbo].[TendMast]
                                                  ([TENDCODE]
                                                 ,[SHORTDESC]
                                                 ,[TENDDESC]
                                                 ,[TENDCLASS]
                                                 ,[EXCHANGE]
                                                 ,[DISPLAYSEQ]
                                                 ,[EXACTCHG]
                                                 ,[GIVECHG]
                                                 ,[AcceptAsPayment]
                                                 ,[GIVEASREF]
                                                 ,[CANADJUST]
                                                 ,[BANKABLE]
                                                 ,[SMALLUNIT]
                                                 ,[MAXAMOUNT]
                                                 ,[MINAMOUNT]
                                                 ,[IncludeInClose]
                                                 ,[Put_In_Drawer]
                                                 ,[PrintCopies]
                                                 ,[Signature]
                                                 ,[Inactive])
                                          VALUES(
                                                  @TendCode, 
                                                  @ShortName,
                                                  @FullName,
                                                  @TendClass,              
                                                  @ExchangeRate,
                                                  @Sequence,
                                                  @ExactChange,              
                                                  @GiveChange,
                                                  @AcceptAsPayment,
                                                  @GiveAsRefund,
                                                  @CanAdjust,
                                                  @Bankable,
                                                  @SmallestUnit,
                                                  @MaxAmount,
                                                  @MinAmount,         
                                                  @IncludeInClose,
                                                  @PutInDrawer,
                                                  @PrintCopies,
                                                  @Signature,
                                                  @Inactive)";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@TendCode", SqlDbType.VarChar, 10).Value = tender.Code;
    cmd.Parameters.Add("@ShortName", SqlDbType.VarChar, 8).Value = tender.ShortName ?? (object)DBNull.Value;
    cmd.Parameters.Add("@FullName", SqlDbType.VarChar, 20).Value = tender.FullName ?? (object)DBNull.Value;
    cmd.Parameters.Add("@TendClass", SqlDbType.VarChar, 10).Value = tender.Class ?? (object)DBNull.Value;
    cmd.Parameters.Add("@ExchangeRate", SqlDbType.SmallMoney).Value = tender.ExchangeRate ?? 0;
    cmd.Parameters.Add("@Sequence", SqlDbType.VarChar, 10).Value = tender.Sequence ?? (object)DBNull.Value;
    cmd.Parameters.Add("@ExactChange", SqlDbType.Bit).Value = tender.ExactChange ?? false;
    cmd.Parameters.Add("@GiveChange", SqlDbType.Bit).Value = tender.GiveChange ?? false;
    cmd.Parameters.Add("@AcceptAsPayment", SqlDbType.Bit).Value = tender.AcceptAsPayment ?? false;
    cmd.Parameters.Add("@GiveAsRefund", SqlDbType.Bit).Value = tender.GiveAsRefund ?? false;
    cmd.Parameters.Add("@CanAdjust", SqlDbType.Bit).Value = tender.CanAdjust ?? false;
    cmd.Parameters.Add("@Bankable", SqlDbType.Bit).Value = tender.Bankable ?? false;
    cmd.Parameters.Add("@SmallestUnit", SqlDbType.SmallMoney).Value = tender.SmallestUnit ?? (decimal)0.01;
    cmd.Parameters.Add("@MaxAmount", SqlDbType.SmallMoney).Value = tender.MaxAmount ?? 0;
    cmd.Parameters.Add("@MinAmount", SqlDbType.SmallMoney).Value = tender.MinAmount ?? 0;
    cmd.Parameters.Add("@IncludeInClose", SqlDbType.Bit).Value = tender.IncludeInClose ?? false;
    cmd.Parameters.Add("@PutInDrawer", SqlDbType.Bit).Value = tender.PutInDrawer ?? false;
    cmd.Parameters.Add("@PrintCopies", SqlDbType.SmallMoney).Value = tender.PrintCopies ?? 1;
    cmd.Parameters.Add("@Signature", SqlDbType.Bit).Value = tender.Signature ?? false;
    cmd.Parameters.Add("@Inactive", SqlDbType.Bit).Value = tender.Inactive ?? false;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeleteTenderAsync(string tendCode)
  {
    const string sql = @"Delete from [CSCMaster].[dbo].[TendMast]
                        Where Tendcode = @TendCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@TendCode", SqlDbType.VarChar, 10).Value = tendCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> IsTenderCodeExistAsync(string tendCode)
  {
    const string sql = @"SELECT * FROM TendMast WHERE Tendcode = @tendCode";
    var tender = await Connection.QueryFirstOrDefaultAsync<Tender>(sql, new { tendCode }, Transaction);
    return tender != null;
  }

  public async Task<bool> IsFullNameExistAsync(string fullName)
  {
    const string sql = @"SELECT * FROM TendMast WHERE TendDesc = @fullName";
    var tender = await Connection.QueryFirstOrDefaultAsync<Tender>(sql, new { fullName }, Transaction);
    return tender != null;
  }

  public async Task<IEnumerable<Tender>> GetTendersListAsync()
  {
    const string sql = @"
      SELECT
        TendCode  AS [Code],              
        TendDesc  AS [FullName]
      FROM [CSCMaster].[dbo].[TendMast]
      ORDER BY DisplaySeq";

    return await Connection.QueryAsync<Tender>(sql, null, Transaction);
  }

  public async Task<IEnumerable<Tender>> GetTendersListByClassNameAsync(string tendClass)
  {
    const string sql = @"
      SELECT
        TendCode  AS [Code],              
        TendDesc  AS [FullName]
      FROM [CSCMaster].[dbo].[TendMast]
      WHERE TENDCLASS = @TendClass
      ORDER BY TENDCODE";

    return await Connection.QueryAsync<Tender>(sql, new { TendClass = tendClass }, Transaction);
  }
}
