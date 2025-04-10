namespace InfonetCSCR.Data.Repositories;

public class BrandRepository : BaseRepository, IBrandRepository
{
  public BrandRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IList<Brand>> GetAllBrandsAsync()
  {
    const string sql = @"SELECT 
	                        [BrandID] BrandID,
	                        [Description] Description
                        FROM [CSCMaster].[dbo].[Brand]  
                        ORDER BY [CSCMaster].[dbo].[Brand].BrandID";

    IList<Brand> brands = (IList<Brand>)await Connection.QueryAsync<Brand>(sql, null, Transaction);
    return brands;
  }

  public async Task<bool> AddBrandAsync(Brand brand)
  {
    string sql = @$"
            INSERT INTO [CSCMaster].[dbo].[Brand]
            (
                [BrandID],
                [Description]
            )
            VALUES
            (
                @BrandID,
                @Description
            )";

    int rowsAffected = await Connection.ExecuteAsync(sql, brand, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateBrandAsync(Brand brand)
  {
    string sql = @$"UPDATE [CSCMaster].[dbo].[Brand]
                      SET
                          Description = @Description
                      WHERE
                          BrandID = @BrandID";
    int rowsAffected = await Connection.ExecuteAsync(sql, brand, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteBrandAsync(string brandId)
  {
    string sql = @$"DELETE FROM [CSCMaster].[dbo].[Brand]
                        WHERE BrandID = @BrandID";

    int rowsAffected = await Connection.ExecuteAsync(sql, new { BrandID = brandId }, Transaction);
    return rowsAffected > 0;
  }
}
