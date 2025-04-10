namespace InfonetCSCR.Web.Abstractions.Services;

public interface ICardProductRestrictionService
{
  Task<bool> AddProductRestriction(ProductRestrictionViewModel productRestriction);
  Task<bool> DeleteProductRestriction(ProductRestrictionViewModel productRestriction);
  Task<IEnumerable<ProductRestrictionViewModel>?> GetProductRestrictions(string? bankCardId, string? profileId);
  Task<IEnumerable<StockCodesForModalViewModel>?> GetStockCodesForProductRestriction(bool sellInactivePolicy, bool? isOrderByStockCode = false);
  Task<bool> IsProductRestrictionExist(ProductRestrictionViewModel productRestriction);
  Task<bool> UpdateProductRestriction(ProductRestrictionViewModel productRestriction);
}
