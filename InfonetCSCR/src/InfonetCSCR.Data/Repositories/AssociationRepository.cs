namespace InfonetCSCR.Data.Repositories;

public class AssociationRepository : BaseRepository, IAssociationRepository
{
  public AssociationRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<Assoc>> GetAssociationsAsync()
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT 
	                      AS_CODE Code, 
	                      AS_DESC Description, 
	                      AS_PRICE Price 
                      FROM Assoc ORDER BY Assoc.As_Code");

    return await Connection.QueryAsync<Assoc>(query.ToString(), null, Transaction);
  }

  public async Task<IEnumerable<Assoc>> GetStockAssociationsByStockCodeAndStoreAsync(string stockCode, string storeId)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"SELECT DISTINCT AS_CODE Code
                      FROM Stock_AC 
                      WHERE Stock_Code = '{stockCode}' and storeid = '{storeId}'
                      ORDER BY AS_CODE");

    return await Connection.QueryAsync<Assoc>(query.ToString(), null, Transaction);
  }

  public async Task<double> GetStockAssociationsChargeByPartNoAsync(string partNo)
  {
    string sql = $@"SELECT [AS_PRICE]
                      FROM [CSCMaster].[dbo].[ASSOC] ASS
                      JOIN [CSCMaster].[dbo].[STOCK_AC] AC ON ASS.[AS_CODE] = AC.[AS_CODE]
                      WHERE AC.STOCK_CODE = (
                          SELECT TOP 1 STOCK_CODE
                          FROM [CSCMaster].[dbo].[VendorParts]
                          WHERE PartNo = @PartNo)";

    var parameter = new { PartNo = partNo };
    double charges = await Connection.QueryFirstOrDefaultAsync<double>(sql, parameter, Transaction);
    return charges;
  }

  public async Task<IEnumerable<(string stockCode, double charge)>> GetStockCodeAndAssociationsChargeByStockCodesAsync(IList<string> stockCodes)
  {
    string sql = $@"SELECT LTRIM(RTRIM(AC.STOCK_CODE)) stockCode, [AS_PRICE] price
                      FROM [CSCMaster].[dbo].[ASSOC] ASS
                      JOIN [CSCMaster].[dbo].[STOCK_AC] AC ON ASS.[AS_CODE] = AC.[AS_CODE]
                      WHERE AC.STOCK_CODE IN @StockCodes";

    var parameter = new { StockCodes = stockCodes };
    IEnumerable<(string stockCode, double price)> stockCodeAndAssociationsCharges =
      await Connection.QueryAsync<(string stockCode, double price)>(sql, parameter, Transaction);
    return stockCodeAndAssociationsCharges;
  }

  public async Task<bool> AddAssociationAsync(Assoc assoc)
  {
    string query = @"INSERT INTO Assoc (AS_CODE, AS_DESC, AS_PRICE) 
                     VALUES (@Code, @Description, @Price)";

    int rowsAffected = await Connection.ExecuteAsync(query, assoc, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateAssociationAsync(Assoc assoc)
  {
    string query = @"UPDATE Assoc 
                      SET AS_DESC = @Description, AS_PRICE = @Price 
                      WHERE AS_CODE = @Code";

    int rowsAffected = await Connection.ExecuteAsync(query, assoc, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteAssociationAsync(string code)
  {
    string query = "DELETE FROM Assoc WHERE AS_CODE = @Code";

    int rowsAffected = await Connection.ExecuteAsync(query, new { Code = code }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> IsAssociationExistsAsync(string code)
  {
    string query = "SELECT COUNT(1) FROM Assoc WHERE AS_CODE = @Code";

    int count = await Connection.ExecuteScalarAsync<int>(query, new { Code = code }, Transaction);
    return count > 0;
  }

  public async Task<IEnumerable<Tax>?> GetTaxesAsync(string code)
  {
    string query = @"SELECT Assoc_Tax.As_Code,
                        Assoc_Tax.Tax_Name as [TaxName],
                        Assoc_Tax.Tax_Code as [TaxCode],
                        TaxRate.Tax_Rate  as [Rate],
                        TaxRate.Tax_Incl  as [Included] 
                      FROM
                        Assoc_Tax INNER JOIN (TaxRate INNER JOIN TaxMast
                        ON TaxRate.Tax_Name = TaxMast.Tax_Name )
                        ON Assoc_Tax.Tax_Name = TaxRate.Tax_Name AND
                        Assoc_Tax.Tax_Code = TaxRate.Tax_Code 
                      WHERE
                        Assoc_Tax.AS_Code = @Code AND
                        TaxMast.Tax_Active = 1 ORDER BY Assoc_Tax.Tax_Name ";

    var taxes = await Connection.QueryAsync<Tax>(query, new { Code = code }, Transaction);
    return taxes;
  }

  public async Task<IEnumerable<Tax>?> GetAvailableTaxesAsync(string code)
  {
    string query = @"Select DISTINCT TaxMast.Tax_Name AS TaxName,
                        TaxRate.Tax_Code AS TaxCode
                      From
                        TaxMast INNER JOIN TaxRate ON
                        TaxMast.Tax_Name = TaxRate.Tax_Name
                      Where
                        TaxMast.Tax_Name NOT IN
                        (SELECT Assoc_Tax.Tax_Name
                          FROM
                            Assoc_Tax INNER JOIN (TaxRate INNER JOIN TaxMast ON TaxRate.Tax_Name = TaxMast.Tax_Name )
                            ON Assoc_Tax.Tax_Name = TaxRate.Tax_Name AND
                            Assoc_Tax.Tax_Code = TaxRate.Tax_Code
                          WHERE
                            Assoc_Tax.AS_Code = @Code AND
                            TaxMast.Tax_Active = 1 ) AND
                        TaxMast.Tax_Active = 1";

    var taxes = await Connection.QueryAsync<Tax>(query, new { Code = code }, Transaction);
    return taxes;
  }

  public async Task<bool> AddTaxAsync(AssocTax assocTax)
  {
    string query = @"INSERT INTO [CSCMaster].[dbo].[Assoc_Tax]
                        (As_Code, Tax_Name, Tax_Code)
                        VALUES (@Code, @TaxName, @TaxCode)";

    var rowsAffected = await Connection.ExecuteAsync(query, assocTax, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteTaxAsync(AssocTax assocTax)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[Assoc_Tax]
                        WHERE Tax_Name = @TaxName
                        AND As_Code = @Code
                        AND Tax_Code = @TaxCode";

      int rowsAffected = await Connection.ExecuteAsync(sql, assocTax , Transaction);
      return rowsAffected > 0;
  }

  public async Task<bool> IsChargeInUseAsync(string code)
  {
    string query = @"SELECT COUNT(1) FROM [CSCMaster].[dbo].[STOCK_AC] 
                        WHERE AS_CODE = @Code";

    int count = await Connection.ExecuteScalarAsync<int>(query, new { Code = code }, Transaction);
    return count > 0;
  }
}
