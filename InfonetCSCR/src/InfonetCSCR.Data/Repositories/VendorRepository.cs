namespace InfonetCSCR.Data.Repositories;

//CSCMaster Connection
internal class VendorRepository : BaseRepository, IVendorRepository
{
  public VendorRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<IEnumerable<Vendor>> GetAllVendorsAsync()
  {
    const string sql = @"SELECT Code, Name, Address, City, Province, Country, Postal, Phone, Fax, Email, ContactName1, ContactName2, TaxNumber, IsTEVendor FROM [CSCMaster].[dbo].[Vendors]";

    IEnumerable<Vendor> vendors = await Connection.QueryAsync<Vendor>(sql, null, Transaction);
    return vendors;
  }

  public async Task<Vendor> GetVendorByCodeAsync(string code)
  {
    string sql = @$"SELECT
                        Code,
                        Name,
                        CustAccount,
                        PoDiscount,
                        ISNULL(PoCostTaxInclude, 0) AS PoCostTaxInclude
                      FROM[CSCMaster].[dbo].[Vendors]
                      WHERE Code = @Code";

    var parameter = new { Code = code };
    Vendor vendor = await Connection.QueryFirstOrDefaultAsync<Vendor>(sql, parameter, Transaction);
    return vendor;
  }

  public async Task<IEnumerable<Vendor>> GetVendorsByVendorIdsAsync(IEnumerable<string> vendorIds)
  {
    StringBuilder query = new StringBuilder();
    query.Append($@"select Code, Name from  vendors where Code in @vendorIds");

    IEnumerable<Vendor> vendors = await Connection.QueryAsync<Vendor>(query.ToString(), new { vendorIds }, Transaction);
    return vendors;
  }
  public async Task<Vendor?> GetVendorByPurchaseNoAsync(long purchaseNo)
  {
    string sql = @$"SELECT
                          Vendors.code,
                          Vendors.email,
                          Vendors.name
                      FROM
                          CSCMaster.dbo.Vendors AS Vendors
                      INNER JOIN
                          CSCTrans.dbo.PurchaseHeader AS PurchaseHeader
                          ON Vendors.code = PurchaseHeader.accountno
                      WHERE
                          PurchaseHeader.purchaseno = @PurchaseNo";

    Vendor? vendor = await Connection.QueryFirstOrDefaultAsync<Vendor>(sql, new { PurchaseNo = purchaseNo }, Transaction);
    return vendor;
  }

  public async Task<IEnumerable<Vendor>> GetActiveVendorsOrderByColumnNumberAsync(int columnNumber)
  {
    string sql = $@"SELECT Code , Name FROM [CSCMaster].[dbo].[Vendors] WHERE Active = 1 ORDER BY {columnNumber}";

    IEnumerable<Vendor> vendors = await Connection.QueryAsync<Vendor>(sql, null, Transaction);
    return vendors;
  }

  public async Task<double> GetPoDiscountAsync(string vendorId)
  {
    string sql = $@"SELECT PoDiscount from [CSCMaster].[dbo].[Vendors] where Code = @VendorId";

    var parameters = new { VendorId = vendorId };
    
    double? poDiscount = await Connection.QueryFirstOrDefaultAsync<double?>(sql, parameters, Transaction);

    if(poDiscount == null)
    {
      poDiscount = 0.00;
    }

    poDiscount = (double?)Math.Round((decimal)poDiscount, 2);

    return (double)poDiscount;
  }

  public async Task<bool> GetIsTaxIncludeInPOAsync(string vendorId)
  {
    string sql = $@"SELECT PoCostTaxInclude from [CSCMaster].[dbo].[Vendors] where Code = @VendorId";

    var parameters = new { VendorId = vendorId };
    
    bool? isTaxIncluded = await Connection.QueryFirstOrDefaultAsync<bool?>(sql, parameters, Transaction);

    if(isTaxIncluded == null)
    {
      isTaxIncluded = false;
    }

    return (bool)isTaxIncluded;
  }

  public async Task<string> GetCustAccountAsync(string vendorId)
  {
    string sql = $@"SELECT CustAccount from [CSCMaster].[dbo].[Vendors] where Code = @VendorId";

    var parameters = new { VendorId = vendorId };

    string custAccount = await Connection.QueryFirstOrDefaultAsync<string>(sql, parameters, Transaction);

    return custAccount;
  }

  public async Task<Vendor> GetVendorByVendorCodeAsync(string code, bool isBackOffice)
  {
    string sql = @$"SELECT 
                        Code
                        ,Name
                        ,Address
                        ,City	
                        ,Province
                        ,Country
                        ,Postal
                        ,Phone
                        ,Fax
                        ,Email
                        ,ContactName1
                        ,ContactName2
                        ,TaxNumber
                        ,CustAccount
                        ,IsTEVendor
                        ,OrderFreq
                        ,Active
                        {
                        (
                          isBackOffice?
                          @"
                              ,DiscOnTotal
                              ,PoDiscount
                              ,PoCostTaxInclude
                            " : ""
                        )
                      }
                      FROM [CSCMaster].[dbo].[Vendors] 
                      WHERE Code = @Code";

    var parameter = new { Code = code };
    return await Connection.QueryFirstOrDefaultAsync<Vendor>(sql, parameter, Transaction);
  }

  public async Task<bool> AddVendorAsync(Vendor vendor, bool isBackOffice)
  {
    string sql = $@"Insert into [CSCMaster].[dbo].[Vendors] 
                      (
                        Code
                        ,Name
                        ,Address
                        ,City	
                        ,Province
                        ,Country
                        ,Postal
                        ,Phone
                        ,Fax
                        ,Email
                        ,ContactName1
                        ,ContactName2
                        ,TaxNumber
                        ,CustAccount
                        ,IsTEVendor
                        ,OrderFreq
                        ,Active
                        {
                        (
                          isBackOffice?
                            @",DiscOnTotal
                              ,PoDiscount
                              ,PoCostTaxInclude" : ""
                        )
                      }
                      )
                      Values
                      (
                        @Code
                        ,@Name
                        ,@Address
                        ,@City
                        ,@Province      
                        ,@Country
                        ,@Postal
                        ,@Phone
                        ,@Fax
                        ,@Email
                        ,@ContactName1
                        ,@ContactName2
                        ,@TaxNumber
                        ,@CustAccount            
                        ,@IsTEVendor
                        ,@OrderFreq
                        ,@Active
                        {
                        (
                          isBackOffice?
                            @",0
                              ,@PoDiscount
                              ,@PoCostTaxInclude" : ""
                        )
                      }
                      )";
    int count = await Connection.ExecuteAsync(sql, vendor, Transaction);
    return count > 0;
  }

  public async Task<bool> UpdateVendorAsync(Vendor vendor, bool isBackOffice)
  {
    string sql = $@"Update [CSCMaster].[dbo].[Vendors] 
                      Set
                        Name = @Name
                        ,Address = @Address
                        ,City	= @City
                        ,Province = @Province
                        ,Country = @Country
                        ,Postal = @Postal
                        ,Phone = @Phone
                        ,Fax = @Fax
                        ,Email = @Email
                        ,ContactName1 = @ContactName1
                        ,ContactName2 = @ContactName2
                        ,TaxNumber = @TaxNumber
                        ,CustAccount = @CustAccount
                        ,IsTEVendor = @IsTEVendor
                        ,OrderFreq = @OrderFreq
                        ,Active = @Active
                        {
                        (
                          isBackOffice?
                            @",PoDiscount = @PoDiscount
                              ,PoCostTaxInclude = @PoCostTaxInclude" : ""
                        )
                      }
                        Where Code = @Code
                      ";
    var count = await Connection.ExecuteAsync(sql, vendor, Transaction);
    return count > 0;
  }

  public async Task<int> GetVendorCountStockMstAsync(string vendorId)
  {
    string sql = $@"Select Count(*) from [CSCMaster].[dbo].[StockMst] Where Vendor = @VendorId";
    return await Connection.QueryFirstOrDefaultAsync<int>(sql, new { VendorId = vendorId }, Transaction);
  }

  public async Task<int> GetVendorCountStockCostsAsync(string vendorId)
  {
    string sql = $@"Select Count(*) from [CSCMaster].[dbo].[StockCosts] Where VendorId = @VendorId";
    return await Connection.QueryFirstOrDefaultAsync<int>(sql, new { VendorId = vendorId }, Transaction);
  }

  public async Task<int> GetVendorCountStockPricesAsync(string vendorId)
  {
    string sql = $@"Select Count(*) from [CSCMaster].[dbo].[Stock_Prices] Where VendorId = @VendorId";
    return await Connection.QueryFirstOrDefaultAsync<int>(sql, new { VendorId = vendorId }, Transaction);
  }

  public async Task VendorDeleteFromVendorAsync(string vendorId)
  {
    var sql = $@"Delete from [CSCMaster].[dbo].[Vendors] Where Code = @VendorId";
    await Connection.ExecuteAsync(sql, new { VendorId = vendorId }, Transaction);
  }

  public async Task VendorDeleteFromVendorSortAsync(string vendorId)
  {
    var sql = $@"Delete from [CSCMaster].[dbo].[VendorSort] Where VendorId = @VendorId";
    await Connection.ExecuteAsync(sql, new { VendorId = vendorId }, Transaction);
  }

  public async Task VendorDeleteFromStockCostsAsync(string vendorId)
  {
    var sql = $@"Delete from [CSCMaster].[dbo].[StockCosts] Where VendorId = @VendorId";
    await Connection.ExecuteAsync(sql, new { VendorId = vendorId }, Transaction);
  }

  public async Task VendorDeleteFromStockPricesAsync(string vendorId)
  {
    var sql = $@"Delete from [CSCMaster].[dbo].[Stock_Prices] Where VendorId = @VendorId";
    await Connection.ExecuteAsync(sql, new { VendorId = vendorId }, Transaction);
  }

  public async Task SetTEVendorNullProductTaxExemptAsync(string vendorId)
  {
    var sql = $@"Update ProductTaxExempt Set TEVendor = NULL Where TEVendor = @VendorId";
    await Connection.ExecuteAsync(sql, new { VendorId = vendorId }, Transaction);
  }

  public async Task<IEnumerable<Vendor>> GetAllVendorNameAsync()
  {
    const string sql = @"SELECT Code, Name FROM [CSCMaster].[dbo].[Vendors] Order by Code";
    return await Connection.QueryAsync<Vendor>(sql, null, Transaction);
  }

  public async Task<IEnumerable<VendorLink>> GetVendorLinksAsync(string vendorId)
  {
    const string sql = @"SELECT VendorId, RegionId, StoreId FROM [CSCMaster].[dbo].[VendorLink] Where VendorId = @VendorId Order by 1, 2, 3";
    return await Connection.QueryAsync<VendorLink>(sql, new { VendorId = vendorId }, Transaction);
  }

  public async Task<bool> DeleteVendorStoreAsync(string vendorId, string storeId)
  {
    const string sql = @"DELETE FROM [CSCMaster].[dbo].[VendorLink] Where VendorId = @VendorId And StoreId = @StoreId";
    var count = await Connection.ExecuteAsync(sql, new { VendorId = vendorId, StoreId = storeId }, Transaction);
    return count > 0;
  }

  public async Task<bool> DeleteVendorRegionAsync(string vendorId, string regionId)
  {
    const string sql = @"DELETE FROM [CSCMaster].[dbo].[VendorLink] Where VendorId = @VendorId And RegionId = @RegionId";
    var count = await Connection.ExecuteAsync(sql, new { VendorId = vendorId, RegionId = regionId }, Transaction);
    return count > 0;
  }

  public async Task<bool> AddVendorLinkAsync(VendorLink vendorLink)
  {
    const string sql = @"INSERT Into [CSCMaster].[dbo].[VendorLink](VendorId, StoreId, RegionId) Values(@VendorId, @StoreId, @RegionId)";
    var count = await Connection.ExecuteAsync(sql, vendorLink, Transaction);
    return count > 0;
  }

  public async Task<IEnumerable<Vendor>> GetVendorsOrderByColumnNumberAsync(int columnNumber)
  {
    string sql = $@"SELECT Code , Name FROM [CSCMaster].[dbo].[Vendors] ORDER BY {columnNumber}";

    IEnumerable<Vendor> vendors = await Connection.QueryAsync<Vendor>(sql, null, Transaction);
    return vendors;
  }
}
