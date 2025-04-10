namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
public class ProductTaxExemptRepository : BaseRepository, IProductTaxExemptRepository
{
	private readonly IApplicationConfig _config;
	public ProductTaxExemptRepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
	{
		_config = config;
	}

  public async Task<bool> AddProductTaxExemptAsync(ProductTaxExempt productTaxExempt)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[ProductTaxExempt] (
													[ProductKey],
	                        [UpcCode],
	                        [CategoryFK],
	                        [TaxFreePrice],
	                        [UnitsPerPkg],
	                        [EmplID],
	                        [LastUpdateTime],
                          [TAX_CODE],
	                        [ProductCode],
	                        [InStock],
	                        [Available],
	                        [Hold],
	                        [Waste],
	                        [ReorderQuantity],
	                        [LastSaleTime],
	                        [QuantityPerPkg],
	                        [BaseUnitQty],
	                        [MaxQty],
	                        [TEVendor]
												)
												VALUES (
													@ProductKey,
	                        @UpcCode,
	                        @CategoryFK,
	                        @TaxFreePrice,
	                        @UnitsPerPkg,
	                        @EmployeeId,
	                        @LastUpdateTime,
                          @TaxCode,
	                        @ProductCode,
	                        @InStock,
	                        @Available,
	                        @Hold,
	                        @Waste,
	                        @ReorderQuantity,
	                        @LastSaleTime,
	                        @QuantityPerPkg,
	                        @BaseUnitQuantity,
	                        @MaxQuantity,
	                        @TaxExemptVendor
												)";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@ProductKey", SqlDbType.VarChar, 50).Value = productTaxExempt.ProductKey;
    cmd.Parameters.Add("@UpcCode", SqlDbType.VarChar, 50).Value = productTaxExempt.UpcCode;
    cmd.Parameters.Add("@CategoryFK", SqlDbType.Int).Value = productTaxExempt.CategoryFK;
    cmd.Parameters.Add("@TaxFreePrice", SqlDbType.SmallMoney).Value = productTaxExempt.TaxFreePrice;
    cmd.Parameters.Add("@UnitsPerPkg", SqlDbType.Int).Value = productTaxExempt.UnitsPerPkg;
    cmd.Parameters.Add("@EmployeeId", SqlDbType.VarChar, 50).Value = productTaxExempt.EmplID;
    cmd.Parameters.Add("@LastUpdateTime", SqlDbType.VarChar, 50).Value = DateTime.Now.ToString();
    cmd.Parameters.Add("@TaxCode", SqlDbType.SmallInt).Value = productTaxExempt.TaxCode ?? 0;
    cmd.Parameters.Add("@ProductCode", SqlDbType.VarChar, 25).Value = productTaxExempt.ProductCode ?? "";
    cmd.Parameters.Add("@InStock", SqlDbType.Real).Value = productTaxExempt.InStock ?? 0;
    cmd.Parameters.Add("@Available", SqlDbType.Real).Value = productTaxExempt.Available ?? 0;
    cmd.Parameters.Add("@Hold", SqlDbType.Real).Value = productTaxExempt.Hold ?? 0;
    cmd.Parameters.Add("@Waste", SqlDbType.Real).Value = productTaxExempt.Waste ?? 0;
    cmd.Parameters.Add("@ReorderQuantity", SqlDbType.Real).Value = productTaxExempt.ReorderQuantity ?? 0;
    cmd.Parameters.Add("@LastSaleTime", SqlDbType.SmallDateTime).Value = productTaxExempt.LastSaleTime ?? (object)DBNull.Value;
    cmd.Parameters.Add("@QuantityPerPkg", SqlDbType.SmallInt).Value = productTaxExempt.QuantityPerPkg ?? 0;
    cmd.Parameters.Add("@BaseUnitQuantity", SqlDbType.SmallInt).Value = productTaxExempt.BaseUnitQty ?? 0;
    cmd.Parameters.Add("@MaxQuantity", SqlDbType.Real).Value = productTaxExempt.MaxQty ?? (object)DBNull.Value;
    cmd.Parameters.Add("@TaxExemptVendor", SqlDbType.VarChar, 10).Value = productTaxExempt.TEVendor ?? (object)DBNull.Value;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> UpdateProductTaxExemptAsync(ProductTaxExempt productTaxExempt)
  {
    string sql = $@"UPDATE [CSCMaster].[dbo].[ProductTaxExempt]
											SET
	                        [UpcCode] = @UpcCode,
	                        [CategoryFK] = @CategoryFK,
	                        [TaxFreePrice] = @TaxFreePrice,
	                        [UnitsPerPkg] = @UnitsPerPkg,
	                        [EmplID] = @EmployeeId,
	                        [LastUpdateTime] = @LastUpdateTime,
													[TAX_CODE] = @TaxCode,
	                        [ProductCode] = @ProductCode,
	                        [InStock] = @InStock,
	                        [Available] = @Available,
	                        [Hold] = @Hold,
	                        [Waste] = @Waste,
	                        [ReorderQuantity] = @ReorderQuantity,
	                        [LastSaleTime] = @LastSaleTime,
	                        [QuantityPerPkg] = @QuantityPerPkg,
	                        [BaseUnitQty] = @BaseUnitQuantity,
	                        [MaxQty] = @MaxQuantity,
	                        [TEVendor] = @TaxExemptVendor
											WHERE [ProductKey] = @ProductKey";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@ProductKey", SqlDbType.VarChar, 50).Value = productTaxExempt.ProductKey;
    cmd.Parameters.Add("@UpcCode", SqlDbType.VarChar, 50).Value = productTaxExempt.UpcCode;
    cmd.Parameters.Add("@CategoryFK", SqlDbType.Int).Value = productTaxExempt.CategoryFK;
    cmd.Parameters.Add("@TaxFreePrice", SqlDbType.SmallMoney).Value = productTaxExempt.TaxFreePrice;
    cmd.Parameters.Add("@UnitsPerPkg", SqlDbType.Int).Value = productTaxExempt.UnitsPerPkg;
    cmd.Parameters.Add("@EmployeeId", SqlDbType.VarChar, 50).Value = productTaxExempt.EmplID;
    cmd.Parameters.Add("@LastUpdateTime", SqlDbType.VarChar, 50).Value = DateTime.Now.ToString();
    cmd.Parameters.Add("@TaxCode", SqlDbType.SmallInt).Value = productTaxExempt.TaxCode ?? 0;
    cmd.Parameters.Add("@ProductCode", SqlDbType.VarChar, 25).Value = productTaxExempt.ProductCode ?? "";
    cmd.Parameters.Add("@InStock", SqlDbType.Real).Value = productTaxExempt.InStock ?? 0;
    cmd.Parameters.Add("@Available", SqlDbType.Real).Value = productTaxExempt.Available ?? 0;
    cmd.Parameters.Add("@Hold", SqlDbType.Real).Value = productTaxExempt.Hold ?? 0;
    cmd.Parameters.Add("@Waste", SqlDbType.Real).Value = productTaxExempt.Waste ?? 0;
    cmd.Parameters.Add("@ReorderQuantity", SqlDbType.Real).Value = productTaxExempt.ReorderQuantity ?? 0;
    cmd.Parameters.Add("@LastSaleTime", SqlDbType.SmallDateTime).Value = productTaxExempt.LastSaleTime ?? (object)DBNull.Value;
    cmd.Parameters.Add("@QuantityPerPkg", SqlDbType.SmallInt).Value = productTaxExempt.QuantityPerPkg ?? 0;
    cmd.Parameters.Add("@BaseUnitQuantity", SqlDbType.SmallInt).Value = productTaxExempt.BaseUnitQty ?? 0;
    cmd.Parameters.Add("@MaxQuantity", SqlDbType.Real).Value = productTaxExempt.MaxQty ?? (object)DBNull.Value;
    cmd.Parameters.Add("@TaxExemptVendor", SqlDbType.VarChar, 10).Value = productTaxExempt.TEVendor ?? (object)DBNull.Value;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<ProductTaxExempt?> GetProductTaxExemptByProductKeyAsync(string productKey)
  {
    string sql = @$"SELECT
	                        [ProductKey] ProductKey,
	                        [UpcCode] UpcCode,
	                        [CategoryFK] CategoryFK,
	                        [TaxFreePrice] TaxFreePrice,
	                        [UnitsPerPkg] UnitsPerPkg,
	                        [EmplID] EmplID,
	                        [LastUpdateTime] LastUpdateTime,
                          [TAX_CODE] TaxCode,
	                        [ProductCode] ProductCode,
	                        [InStock] InStock,
	                        [Available] Available,
	                        [Hold] Hold,
	                        [Waste] Waste,
	                        [ReorderQuantity] ReorderQuantity,
	                        [LastSaleTime] LastSaleTime,
	                        {(_config.IsBackOffice ? @$"[QuantityPerPkg] QuantityPerPkg,
	                        [BaseUnitQty] BaseUnitQty,
	                        [MaxQty] MaxQty," : "")}
	                        [TEVendor] TEVendor
                        FROM [CSCMaster].[dbo].[ProductTaxExempt]
                        WHERE [ProductKey] = @ProductKey";

    var parameters = new { ProductKey = productKey };
    ProductTaxExempt? productTaxExempts = await Connection.QueryFirstOrDefaultAsync<ProductTaxExempt?>(sql, parameters, Transaction);

    if (productTaxExempts == null)
    {
      productTaxExempts = new ProductTaxExempt
      {
        ProductKey = productKey,
        UpcCode = productKey
      };
    }

    return productTaxExempts;
  }

  public async Task<bool> DeleteProductTaxExemptByStockCodeAsync(string productKey)
  {
    const string sql = @"Delete from ProductTaxExempt
                        Where ProductTaxExempt.ProductKey = @ProductKey";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@ProductKey", SqlDbType.VarChar, 16).Value = productKey;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<ProductTaxExempt> GetProductTaxExemptAsync(string stockCode)
  {
    string sql = $@"SELECT
													[ProductKey] ProductKey,
													[UpcCode] UpcCode,
													[CategoryFK] CategoryFK,
													[TaxFreePrice] TaxFreePrice,
													[UnitsPerPkg] UnitsPerPkg,
													[EmplID] EmplID,
													[LastUpdateTime] LastUpdateTime,
													[TAX_CODE] TaxCode,
													[ProductCode] ProductCode,
													[InStock] InStock,
													[Available] Available,
													[Hold] Hold,
													[Waste] Waste,
													[ReorderQuantity] ReorderQuantity,
													[LastSaleTime] LastSaleTime,
													[QuantityPerPkg] QuantityPerPkg,
													[BaseUnitQty] BaseUnitQty,
													[MaxQty] MaxQty,
													[TEVendor] TEVendor
												FROM [CSCMaster].[dbo].[ProductTaxExempt]
												WHERE [ProductKey] = @StockCode";

    var parameters = new { StockCode = stockCode };
    ProductTaxExempt resultProductTaxExempt = await Connection.QuerySingleOrDefaultAsync<ProductTaxExempt>(sql, parameters, Transaction);
    return resultProductTaxExempt;
  }

  public async Task<bool> UpdateProductTaxExemptMaxQtyAndReorderQtyAsync(ProductTaxExempt productTaxExempt)
  {
    const string sql = @"UPDATE [CSCMaster].[dbo].[ProductTaxExempt]
                         SET
                           [MaxQty] = @MaxQty,
                           [ReorderQuantity] = @ReOrderQuantity
                         WHERE [ProductKey] = @StockCode";

    using SqlCommand cmd = new(sql, (SqlConnection?)Connection, (SqlTransaction?)Transaction);
    cmd.Parameters.Add("@StockCode", SqlDbType.VarChar, 50).Value = productTaxExempt.ProductCode;
    cmd.Parameters.Add("@MaxQty", SqlDbType.Real).Value = productTaxExempt.MaxQty ?? 0;
    cmd.Parameters.Add("@ReOrderQuantity", SqlDbType.Real).Value = productTaxExempt.ReorderQuantity ?? 0;

    cmd.CommandType = CommandType.Text;
    int rowAffected = cmd.ExecuteNonQuery();

    return await Task.FromResult(rowAffected > 0);
  }

  public async Task<bool> AddProductTaxExemptBatchAsync(IEnumerable<ProductTaxExempt> productTaxExempts)
  {
    string sql = $@"INSERT INTO [CSCMaster].[dbo].[ProductTaxExempt] (
											[ProductKey],
											[UpcCode],
											[CategoryFK],
											[TaxFreePrice],
											[UnitsPerPkg],
											[EmplID],
											[LastUpdateTime],
											[TAX_CODE],
											[ProductCode],
											[InStock],
											[Available],
											[Hold],
											[Waste],
											[ReorderQuantity],
											[LastSaleTime],
											[QuantityPerPkg],
											[BaseUnitQty],
											[MaxQty],
											[TEVendor]
											) VALUES (
												@ProductKey,
												@UpcCode,
												@CategoryFK,
												@TaxFreePrice,
												@UnitsPerPkg,
												@EmplID,
												@LastUpdateTime,
												@TaxCode,
												@ProductCode,
												@InStock,
												@Available,
												@Hold,
												@Waste,
												@ReorderQuantity,
												@LastSaleTime,
												@QuantityPerPkg,
												@BaseUnitQty,
												@MaxQty,
												@TEVendor
											)";

    int affectedRows = await Connection.ExecuteAsync(sql, productTaxExempts, Transaction);
    return affectedRows == productTaxExempts.Count();
  }

  public async Task<bool> DeleteProductTaxExemptByStockCodeBatchAsync(IEnumerable<string> productKeys)
  {
    string sql = @"DELETE FROM ProductTaxExempt
                   WHERE ProductTaxExempt.ProductKey IN @ProductKeys";

    int affectedRows = await Connection.ExecuteAsync(sql, new { ProductKeys = productKeys }, Transaction);
    return affectedRows == productKeys.Count();
  }
}
