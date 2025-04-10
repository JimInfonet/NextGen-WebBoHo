namespace InfonetCSCR.Web.Services;

public class CardProductRestrictionService : ICardProductRestrictionService
{
  private readonly IHttpService _http;

  public CardProductRestrictionService(IHttpService http)
  {
    _http = http;
  }

  public async Task<IEnumerable<ProductRestrictionViewModel>?> GetProductRestrictions(string? bankCardId, string? profileId)
  {
    string url = $"CardProductRestriction/GetProductRestrictions?bankCardId={bankCardId}&profileId={HttpUtility.UrlEncode(profileId)}";
    var productRestrictions = await _http.GetAndParseAsync<IEnumerable<ProductRestrictionViewModel>>(url);
    return productRestrictions;
  }

  public async Task<IEnumerable<StockCodesForModalViewModel>?> GetStockCodesForProductRestriction(bool sellInactivePolicy, bool? isOrderByStockCode = false)
  {
    string url = $"CardProductRestriction/GetStockCodesForProductRestrictions?sellInactivePolicy={sellInactivePolicy}";
    if (isOrderByStockCode == true)
    {
      url += "&isOrderByStockCode=true";
    }
    var stockCodes = await _http.GetAndParseAsync<IEnumerable<StockCodesForModalViewModel>>(url);
    return stockCodes;
  }

  public async Task<bool> AddProductRestriction(ProductRestrictionViewModel productRestriction)
  {
    productRestriction = SanitizeProductRestriction(productRestriction);

    string url = "CardProductRestriction/AddProductRestriction";
    bool added = await _http.PostAndParseAsync<bool>(url, productRestriction);
    return added;
  }

  public async Task<bool> UpdateProductRestriction(ProductRestrictionViewModel productRestriction)
  {
    productRestriction = SanitizeProductRestriction(productRestriction);

    string url = "CardProductRestriction/UpdateProductRestriction";
    bool updated = await _http.PostAndParseAsync<bool>(url, productRestriction);
    return updated;
  }

  public async Task<bool> DeleteProductRestriction(ProductRestrictionViewModel productRestriction)
  {
    productRestriction = SanitizeProductRestriction(productRestriction);

    string url = "CardProductRestriction/DeleteProductRestriction";
    bool deleted = await _http.PostAndParseAsync<bool>(url, productRestriction);
    return deleted;
  }

  public async Task<bool> IsProductRestrictionExist(ProductRestrictionViewModel productRestriction)
  {
    productRestriction = SanitizeProductRestriction(productRestriction);

    string url = "CardProductRestriction/IsProductRestrictionExist";
    bool exist = await _http.PostAndParseAsync<bool>(url, productRestriction);
    return exist;
  }

  private static ProductRestrictionViewModel SanitizeProductRestriction(ProductRestrictionViewModel productRestriction)
  {
    if (productRestriction.ProfileId != null)
    {
      productRestriction.BankCardId ??= "";
      productRestriction.CardProductCode ??= "";
      productRestriction.CardProductDescription ??= "";
      productRestriction.CardNumber ??= "";
    }

    return productRestriction;
  }
}
