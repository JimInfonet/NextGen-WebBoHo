namespace InfonetCSCR.Core.Abstractions.Services;

public interface ICardProductRestrictionService
{
  Task<bool> AddProductRestriction(ProductRestriction productRestriction);
  Task<bool> DeleteProductRestriction(ProductRestriction productRestriction);
  Task<IEnumerable<ProductRestriction>> GetProductRestrictions(string? bankCardId, string? profileId);
  Task<IEnumerable<StockCodesForModalDto>> GetStockCodesForProductRestrictions(bool sellInactivePolicy, bool? isOrderByStockCode = false);
  Task<bool> IsProductRestrictionExist(ProductRestriction productRestriction);
  Task<bool> UpdateProductRestriction(ProductRestriction productRestriction);
}
