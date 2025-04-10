namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class PLUMastRepository : BaseRepository, IPLUMastRepository
{
  public PLUMastRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> CheckPLUCodeExistsForPLUPrimeAsync(string pluPrime, string pluCode)
  {
    const string sql = @"SELECT PLU_CODE FROM PluMast
                         WHERE PluMast.Plu_Code = @PLUCode";

    var parameters = new { PLUPrime = pluPrime, PLUCode = pluCode };
    string? resultPLUCode = await Connection.QuerySingleOrDefaultAsync<string?>(sql, parameters, Transaction);
    return (resultPLUCode != null);
  }

  public async Task<PLUMast?> GetPLUMastByPLUCodeAsync(string pluCode)
  {
    const string sql = @"SELECT 
	                        [PLU_CODE] PLUCode,
	                        [PLU_PRIM] PLUPrim,
	                        [PLU_TYPE] PLUType
                        FROM [CSCMaster].[dbo].[PLUMAST]
                        WHERE [PLU_CODE] = @PLUCode";

    var parameters = new { PLUCode = pluCode };
    PLUMast? resultPLUMast = await Connection.QuerySingleOrDefaultAsync<PLUMast?>(sql, parameters, Transaction);
    return resultPLUMast;
  }

  public async Task<IEnumerable<PLUMast>?> GetPLUCodesByPLUPrimeAsync(string pluPrime)
  {
    const string sql = @"SELECT DISTINCT
                          PLU_CODE PLUCode,
                          PLU_PRIM PLUPrim,
                          PLU_TYPE PLUType
                         FROM PluMast 
                         WHERE  PluMast.Plu_Prim = @PLUPrime AND PluMast.Plu_Code <> @PLUPrime ORDER BY PluMast.Plu_Code";

    var parameters = new { PLUPrime = pluPrime };
    IEnumerable<PLUMast>? pluCodes = await Connection.QueryAsync<PLUMast>(sql, parameters, Transaction);
    return pluCodes;
  }

  public async Task<bool> AddPLUMastAsync(PLUMast pluMast)
  {
    const string sql = @"INSERT INTO [CSCMaster].[dbo].[PLUMAST] (
	                        [PLU_CODE],
	                        [PLU_PRIM],
	                        [PLU_TYPE]
                        )
                        VALUES (
                          @PLUCode,
                          @PLUPrim, 
                          @PLUType
                        )";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@PLUCode", SqlDbType.VarChar, 25).Value = pluMast.PLUCode;
    cmd.Parameters.Add("@PLUPrim", SqlDbType.VarChar, 16).Value = pluMast.PLUPrim;
    cmd.Parameters.Add("@PLUType", SqlDbType.VarChar, 1).Value = pluMast.PLUType;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeletePLUMastByPLUPrimAsync(string pluPrim)
  {
    const string sql = @"Delete from PluMast
                        Where PluMast.Plu_Prim = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 16).Value = pluPrim;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> DeletePLUMastByPLUCodeAsync(string pluCode)
  {
    const string sql = @"DELETE FROM PluMast WHERE Plu_Code = @PLUCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@PLUCode", SqlDbType.VarChar, 25).Value = pluCode;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<IEnumerable<PLUMast?>> GetPLUMast()
  {
    const string sql = @"SELECT 
	                        [PLU_CODE] PLUCode,
	                        [PLU_PRIM] PLUPrim,
	                        [PLU_TYPE] PLUType
                        FROM [CSCMaster].[dbo].[PLUMAST]";

    var resultPLUMast = await Connection.QueryAsync<PLUMast?>(sql, null, Transaction);
    return resultPLUMast;
  }

  public async Task<IEnumerable<PLUMast>> GetPluMastsForStockTakes()
  {
    const string sql = @"SELECT A.PLU_CODE PLUCode, 
                                  A.PLU_PRIM PLUPrim, 
                                  A.PLU_TYPE PLUType
                           FROM PLUMAST AS A 
                           INNER JOIN REC_LINE AS B ON A.PLU_PRIM = B.STOCK_CODE 
                           WHERE B.Purpose = 2";

    IEnumerable<PLUMast> pluMasts = await Connection.QueryAsync<PLUMast>(sql, null, Transaction);
    return pluMasts;
  }
}
