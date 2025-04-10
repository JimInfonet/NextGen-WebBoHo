namespace InfonetCSCR.Data.Repositories;

public class TaxMasterRepository : BaseRepository, ITaxMasterRepository
{
  public TaxMasterRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<TaxMasterDto>> GetTaxMastersAsync()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"Select DISTINCT 
	                      TaxRate.Tax_Code Code, 
	                      TaxMast.Tax_Name Name, 
	                      TaxRate.TAX_RATE Rate
                      From TaxMast INNER JOIN TaxRate ON
                      TaxMast.Tax_Name = TaxRate.Tax_Name
                      WHERE TaxMast.Tax_Active = 1 ");

    return await Connection.QueryAsync<TaxMasterDto>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<TaxMasterDto>> GetStockTaxByStockCodeAndStoreAsync(string stockCode, string storeId)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT DISTINCT Tax_Name Name, Tax_Code Code
                      FROM StockTax 
                      WHERE Stock_Code = '{stockCode}' and storeid = '{storeId}'");

    return await Connection.QueryAsync<TaxMasterDto>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<ProductTaxDto>> GetProductTaxesAsync()
  {
    const string sql = @"SELECT DISTINCT 
	                        tm.Tax_Name TaxName, 
	                        tm.Tax_Desc TaxDescription, 
	                        tr.Tax_Code TaxCode
                        FROM [CSCMaster].[dbo].[TAXMAST] tm
                        INNER JOIN [CSCMaster].[dbo].[TAXRATE] tr
	                        ON 
	                        tm.Tax_Name = tr.Tax_Name 
                        WHERE tm.Tax_Active = 1
                        ";

    IEnumerable<ProductTaxDto> productTaxes = await Connection.QueryAsync<ProductTaxDto>(sql, null, Transaction);
    return productTaxes;
  }

  public async Task<IEnumerable<double>> GetProductTaxRateByTaxCodeAndTaxNameAsync(string taxCode, string taxName, bool? taxIncl = null)
  {
    string sql = @"SELECT Tax_Rate
                          FROM [CSCMaster].[dbo].[TAXRATE]
                          INNER JOIN [CSCMaster].[dbo].[TAXMAST] ON taxmast.Tax_name = TaxRate.tax_name
                          WHERE TaxRate.Tax_name = @TaxName
                            AND TaxRate.Tax_Code = @TaxCode";

    if(taxIncl != null)
    {
      sql += @" and TaxRate.Tax_INCL= 0";
    }

    sql += @" AND taxmast.Tax_Active = 1";

    var parameter = new { TaxCode = taxCode, TaxName = taxName };
    IEnumerable<double> taxRates = await Connection.QueryAsync<double>(sql, parameter, Transaction);
    return taxRates;
  }

  public async Task<IEnumerable<TaxMasterDto>> GetProductTaxeRateDataByTaxCodesAndTaxNamesAsync(
    IList<(string taxCode, string taxName)> taxCodeTaxNamePairs, bool? taxIncl = null)
  {
    string sql = @"SELECT 
	                    Tax_Rate Rate,
	                    TAX_CODE Code,
	                    taxmast.TAX_NAME Name
                    FROM [CSCMaster].[dbo].[TAXRATE]
                    INNER JOIN [CSCMaster].[dbo].[TAXMAST] ON taxmast.Tax_name = TaxRate.tax_name";

    if (taxCodeTaxNamePairs.Count() > 0)
    {
      sql += " WHERE taxmast.Tax_Active = 1 AND ";

      if (taxIncl != null)
      {
        sql += @$" TaxRate.Tax_INCL = '{taxIncl}' AND ";
      }

      sql += "(";
      bool isFirstIteration = true;
      foreach(var taxCodeTaxNamePair in taxCodeTaxNamePairs)
      {
        if(!isFirstIteration)
        {
          sql += " OR";
        }

        isFirstIteration = false;
        sql += $@"(TaxRate.Tax_name = '{taxCodeTaxNamePair.taxName}' AND TaxRate.Tax_Code = '{taxCodeTaxNamePair.taxCode}')";
      }

      sql += ")";
    }
    else
    {
      sql += " WHERE TaxRate.Tax_name = null AND TaxRate.Tax_Code = null AND TaxRate.Tax_INCL = null AND taxmast.Tax_Active = null";
    }

    IEnumerable<TaxMasterDto> taxMasterDtos = await Connection.QueryAsync<TaxMasterDto>(sql, null, Transaction);
    return taxMasterDtos;
  }

  public async Task<IEnumerable<TaxMaster>> GetTaxesAsync()
  {
    string sql = @$"SELECT TaxMast.Tax_Name as [Name],
                           TaxMast.Tax_Active as [Active],
                           TaxMast.Tax_Ord as [Order],
                           TaxMast.Tax_Desc as [Description],
                           TaxMast.Tax_Def as [Default]
                    FROM
                           [CSCMaster].[dbo].[TaxMast]
                    ORDER BY TaxMast.Tax_Ord ";

    var taxes = await Connection.QueryAsync<TaxMaster>(sql, null, Transaction);
    return taxes;
  }

  public async Task<bool> AddTaxAsync(TaxMaster tax)
  {
    string sql = @$"INSERT INTO [CSCMaster].[dbo].[TaxMast]
                        (Tax_Name, Tax_Active, Tax_Ord, Tax_Desc, Tax_Def)
                        VALUES (@Name, @Active, @Order, @Description, @Default)";

    int rowsAffected = await Connection.ExecuteAsync(sql, tax, Transaction);

    return rowsAffected > 0;
  }

  public async Task<bool> UpdateTaxAsync(TaxMaster tax)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[TaxMast]
                        SET Tax_Active = @Active,
                            Tax_Ord = @Order,
                            Tax_Desc = @Description,
                            Tax_Def = @Default
                        WHERE Tax_Name = @Name";

    int rowsAffected = await Connection.ExecuteAsync(sql, tax, Transaction);

    return rowsAffected > 0;
  }

  public async Task<bool> DeleteTaxAsync(string taxName)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[TaxMast]
                        WHERE Tax_Name = @TaxName";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { TaxName = taxName }, Transaction);

    return rowsAffected > 0;
  }

  public async Task<bool> DeleteTaxRateAsync(string taxName, string? taxCode = null)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[TaxRate]
                        WHERE Tax_Name = @TaxName
                        {(string.IsNullOrWhiteSpace(taxCode) ? "" : "AND Tax_Code = @TaxCode")}";

    try
    {
      int rowsAffected = await Connection.ExecuteAsync(sql, new { TaxName = taxName, TaxCode = taxCode }, Transaction);
      return rowsAffected >= 0;
    }
    catch (Exception)
    {
      return false;
    }
  }

  public async Task<bool> DeleteStockTaxAsync(string taxName, string? taxCode = null)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[StockTax]
                        WHERE Tax_Name = @TaxName
                        {(string.IsNullOrWhiteSpace(taxCode) ? "" : "AND Tax_Code = @TaxCode")}";

    try
    {
      int rowsAffected = await Connection.ExecuteAsync(sql, new { TaxName = taxName, TaxCode = taxCode }, Transaction);

      return rowsAffected >= 0;
    }
    catch (Exception)
    {
      return false;
    }
  }

  public async Task<bool> DeleteAssocTaxAsync(string taxName, string? taxCode = null)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[Assoc_Tax]
                        WHERE Tax_Name = @TaxName
                        {(string.IsNullOrWhiteSpace(taxCode) ? "" : "AND Tax_Code = @TaxCode")}";

    try
    {
      int rowsAffected = await Connection.ExecuteAsync(sql, new { TaxName = taxName, TaxCode = taxCode }, Transaction);

      return rowsAffected >= 0;
    }
    catch (Exception)
    {
      return false;
    }
  }

  public async Task<bool> TaxNameAlreadyExistsAsync(string taxName, string? taxCode)
  {
    string sql = @$"SELECT COUNT(1)
                        FROM [CSCMaster].[dbo].[TaxMast]
                        WHERE Tax_Name = @TaxName
                        {(string.IsNullOrWhiteSpace(taxCode) ? "" : "AND Tax_Code = @TaxCode")}";

    int count = await Connection.ExecuteScalarAsync<int>(sql, new { TaxName = taxName }, Transaction);

    return count > 0;
  }

  public async Task<int> GetTaxUsageCountAsync(string taxName, string? taxCode = null)
  {
    string sql = $@"
        SELECT SUM(TotalCount) as TotalSum
        FROM (
            SELECT COUNT(*) as TotalCount
            FROM Assoc_Tax
            WHERE Assoc_Tax.Tax_Name = @TaxName
            {(string.IsNullOrWhiteSpace(taxCode) ? "" : "AND Assoc_Tax.Tax_Code = @TaxCode")}
            UNION ALL
            
            SELECT COUNT(*) as TotalCount
            FROM StockTax
            WHERE StockTax.Tax_Name = @TaxName
            {(string.IsNullOrWhiteSpace(taxCode) ? "" : "AND StockTax.Tax_Code = @TaxCode")}
        ) AS CombinedCounts";

    var totalSum = await Connection.ExecuteScalarAsync<int>(sql, new { TaxName = taxName, TaxCode = taxCode }, Transaction);

    return totalSum;
  }

  public async Task<IEnumerable<TaxMaster>> GetTaxesOrderByTaxNameAsync()
  {
    const string sql = @"
      SELECT 
        Tax_Name as [Name],
        Tax_Desc as [Description]
      FROM [CSCMaster].[dbo].[TaxMast]
      ORDER BY Tax_name";

    return await Connection.QueryAsync<TaxMaster>(sql, null, Transaction);
  }
}
