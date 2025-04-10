namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ICardProductRestrictionRepository
{
  Task<bool> AddProductRestrictionAsync(ProductRestriction productRestriction);
  Task<bool> DeleteProductRestrictionAsync(ProductRestriction productRestriction);
  Task<IEnumerable<ProductRestriction>> GetProductRestrictionsAsync(string? bankCardId, string? profileId);
  Task<IEnumerable<StockCodesForModalDto>> GetStockCodesForProductRestrictions(bool sellInactivePolicy, bool? isOrderByStockCode = false);
  Task<bool> IsProductRestrictionExist(ProductRestriction productRestriction);
  Task<bool> UpdateProductRestriction(ProductRestriction productRestriction);
  Task<bool> DeleteCardProductRestrictionByBankCardIDAsync(string bankCardID);
}
