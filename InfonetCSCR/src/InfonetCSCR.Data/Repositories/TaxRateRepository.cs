namespace InfonetCSCR.Data.Repositories;

public class TaxRateRepository : BaseRepository, ITaxRateRepository
{
  public TaxRateRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<TaxRate>> GetTaxRatesByTaxNameAsync(string taxName)
  {
    string sql = @$"SELECT TaxRate.Tax_Code as [Code],
                           TaxRate.Tax_Rate as [Rate],
                           TaxRate.Tax_Incl as [Included],
                           TaxRate.Tax_Desc as [Description],
                           TaxRate.Tax_Name as [Name],
                           TaxRate.Tax_Rebate as [TaxRebate]
                    FROM
                           [CSCMaster].[dbo].[TaxRate] AS TaxRate
                    WHERE
                           TaxRate.Tax_Name = @TaxName
                    ORDER BY TaxRate.Tax_Code ";

    var taxRates = await Connection.QueryAsync<TaxRate>(sql, new { TaxName = taxName }, Transaction);
    return taxRates;
  }

  public async Task<bool> TaxCodeAlreadyExistsAsync(TaxRate taxRate)
  {
    string sql = @$"SELECT COUNT(1)
                        FROM [CSCMaster].[dbo].[TaxRate]
                        WHERE TAX_NAME = @Name
                        AND TAX_CODE = @Code";

    int count = await Connection.ExecuteScalarAsync<int>(sql, taxRate, Transaction);

    return count > 0;
  }

  public async Task<bool> AddTaxRateAsync(TaxRate taxRate)
  {
    string sql = @$"INSERT INTO [CSCMaster].[dbo].[TaxRate]
                        (TAX_NAME, TAX_CODE, TAX_RATE, TAX_INCL, TAX_DESC, TAX_REBATE)
                        VALUES (@Name, @Code, @Rate, @Included, @Description, @TaxRebate)";

    int rowsAffected = await Connection.ExecuteAsync(sql, taxRate, Transaction);

    return rowsAffected > 0;
  }

  public async Task<bool> UpdateTaxRateAsync(TaxRate taxRate)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[TaxRate]
                        SET TAX_RATE = @Rate,
                            TAX_INCL = @Included,
                            TAX_DESC = @Description,
                            TAX_REBATE = @TaxRebate
                        WHERE TAX_NAME = @Name AND TAX_CODE = @Code";

    int rowsAffected = await Connection.ExecuteAsync(sql, taxRate, Transaction);

    return rowsAffected > 0;
  }
}
