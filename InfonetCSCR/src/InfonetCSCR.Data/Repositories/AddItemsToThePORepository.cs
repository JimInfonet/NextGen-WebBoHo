namespace InfonetCSCR.Data.Repositories;

public class AddItemsToThePORepository : BaseRepository, IAddItemsToThePORepository
{
  private readonly IApplicationConfig _config;
  private const string _typeSpecificConditions = "__TYPE_SPECIFIC_CONDITIONS__";
  public AddItemsToThePORepository(IApplicationConfig config, IDbTransaction transaction) : base(transaction)
  {
    _config = config;
  }

  public async Task<IEnumerable<VendorPartDetailsDto>> GetVendorPartsAsync(string vendorId, AddItemsToThePOSelectedCriteria selectedCriteria)
  {
    string sql = @$"SELECT DISTINCT 
	                         VendorParts.PartNo
	                        ,VendorParts.VendorId
	                        ,LTRIM(RTRIM(VendorParts.Stock_Code)) AS StockCode
	                        ,StockMST.Descript AS Description
                    FROM VendorParts 
                    INNER JOIN StockMST 
	                    ON Vendorparts.Stock_Code = StockMST.Stock_Code   
                    INNER JOIN Stock_Br 
	                    ON Stock_Br.Stock_Code    = StockMST.Stock_Code  
                    WHERE (Stock_Br.ActiveStock = 1 OR Stock_Br.ActiveStock IS NULL)  
                      {_typeSpecificConditions}
                    ORDER BY Partno, StockCode  ";

    if (selectedCriteria == AddItemsToThePOSelectedCriteria.VendorPartsForTheVendorX)
    {
      sql = sql.Replace(_typeSpecificConditions, $"AND VendorParts.VendorId = '{vendorId}' ");
    }
    else if (selectedCriteria == AddItemsToThePOSelectedCriteria.NonVendorParts)
    {
      sql = sql.Replace(_typeSpecificConditions, "AND (VendorId = 'ALL' ) ");
    }
    else if (selectedCriteria == AddItemsToThePOSelectedCriteria.VendorPartsForOtherVendors)
    {
      sql = sql.Replace(_typeSpecificConditions, $"AND VendorParts.VendorId <> '{vendorId}' AND VendorId IS NOT NULL ");
    }
    else if (selectedCriteria == AddItemsToThePOSelectedCriteria.AllVendorParts)
    {
      sql = sql.Replace(_typeSpecificConditions, "");
    }

    var vendorPart = await Connection.QueryAsync<VendorPartDetailsDto>(sql, null, Transaction);
    return vendorPart;
  }

  public async Task<PoDiscountAndTaxIncludeDto> GetPoDiscountAndTaxIncludeAsync(string vendorId)
  {
    string sql = @"SELECT Code VendorId, PoDiscount, PoCostTaxInclude FROM Vendors WHERE Code = @vendorId ";
    var poDisCountAndTaxInclude = await Connection.QueryFirstOrDefaultAsync<PoDiscountAndTaxIncludeDto>(sql, new { vendorId }, Transaction);
    return poDisCountAndTaxInclude;
  }

  public async Task<IEnumerable<string>> GetExistingPartNosAsync(string purchaseNo, List<string> partNos)
  {
    string sql = $@"SELECT PartNum FROM PurchaseDetail
                    WHERE PurchaseNo = @purchaseNo AND PartNum IN @partNos ";

    var existingPartNos = await Connection.QueryAsync<string>(sql, new { purchaseNo, partNos }, Transaction);
    return existingPartNos;
  }
}
