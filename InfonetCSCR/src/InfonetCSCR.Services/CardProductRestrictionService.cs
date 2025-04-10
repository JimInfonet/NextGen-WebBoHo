namespace InfonetCSCR.Services;

public class CardProductRestrictionService : ICardProductRestrictionService
{
  private readonly IUnitOfWork _unitOfWork;

  public CardProductRestrictionService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<bool> AddProductRestriction(ProductRestriction productRestriction)
  {
    bool isAdded = await _unitOfWork.CardProductRestrictions.AddProductRestrictionAsync(productRestriction);
    _unitOfWork.Commit();
    return isAdded;
  }

  public async Task<bool> DeleteProductRestriction(ProductRestriction productRestriction)
  {
    bool isDeleted = await _unitOfWork.CardProductRestrictions.DeleteProductRestrictionAsync(productRestriction);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<IEnumerable<ProductRestriction>> GetProductRestrictions(string? bankCardId, string? profileId)
  {
    IEnumerable<ProductRestriction> productRestrictions = await _unitOfWork.CardProductRestrictions.GetProductRestrictionsAsync(bankCardId, profileId);
    _unitOfWork.Commit();
    return productRestrictions;
  }

  public async Task<IEnumerable<StockCodesForModalDto>> GetStockCodesForProductRestrictions(bool sellInactivePolicy, bool? isOrderByStockCode = false)
  {
    IEnumerable<StockCodesForModalDto> stockCodes = await _unitOfWork.CardProductRestrictions.GetStockCodesForProductRestrictions(sellInactivePolicy, isOrderByStockCode);
    _unitOfWork.Commit();
    return stockCodes;
  }

  public async Task<bool> IsProductRestrictionExist(ProductRestriction productRestriction)
  {
    bool isExist = await _unitOfWork.CardProductRestrictions.IsProductRestrictionExist(productRestriction);
    _unitOfWork.Commit();
    return isExist;
  }

  public async Task<bool> UpdateProductRestriction(ProductRestriction productRestriction)
  {
    bool isUpdated = await _unitOfWork.CardProductRestrictions.UpdateProductRestriction(productRestriction);
    _unitOfWork.Commit();
    return isUpdated;
  }
}
