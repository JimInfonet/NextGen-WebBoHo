namespace InfonetCSCR.Services;

public class PromotionalItemsService : IPromotionalItemsService
{
  #region Private Properties
  private readonly IUnitOfWork _unitOfWork;
  private readonly IApplicationConfig _config;
  #endregion
  public PromotionalItemsService(
      IApplicationConfig config)
  {
    this._unitOfWork = new UnitOfWork(config, config.CSCMasterConnectionString);
    _config = config;
  }

  public async Task<IEnumerable<PromoHeader>> GetPromoHeaders()
  {
    IEnumerable<PromoHeader> promoHeaders;

    promoHeaders = await _unitOfWork.PromotionalItems.GetPromoHeadersAsync();
    _unitOfWork.Commit();

    return promoHeaders;
  }

  public async Task<PromoHeader> GetPromoHeader(string promoId)
  {
    var promoHeader = await _unitOfWork.PromotionalItems.GetPromoHeaderAsync(promoId);
    _unitOfWork.Commit();

    return promoHeader;
  }

  public async Task<IEnumerable<PromoDetail>> GetPromoDetail(string promoId)
  {
    var promoDetail = await _unitOfWork.PromotionalItems.GetPromoDetailAsync(promoId);
    _unitOfWork.Commit();

    return promoDetail;
  }

  public async Task<bool> IsPromoIdExist(string promoId)
  {
    var promoIdCount = await _unitOfWork.PromotionalItems.IsPromoIdExist(promoId);

    if (promoIdCount > 0)
    {
      return true;
    }

    return false;
  }

  public async Task<bool> AddPromotionalItemHeader(PromoHeader promoHeader)
  {
    bool success = await _unitOfWork.PromotionalItems.AddPromotionalItemHeaderAsync(promoHeader);
    _unitOfWork.Commit();
    return success;
  }

  public async Task<bool> UpdatePromotionalItemHeader(PromoHeader promoHeader)
  {
    bool success = await _unitOfWork.PromotionalItems.UpdatePromotionalItemHeaderAsync(promoHeader);
    _unitOfWork.Commit();
    return success;
  }

  public async Task<bool> AddPromotionalItemDetials(IEnumerable<PromoDetail> promoDetails)
  {
    bool success = await _unitOfWork.PromotionalItems.AddPromotionalItemDetailsAsync(promoDetails);
    _unitOfWork.Commit();
    return success;
  }

  public async Task<bool> DeletePromotionalItemDetails(string promoId)
  {
    bool isDeleted = await _unitOfWork.PromotionalItems.DeletePromotionalItemDetailsAsync(promoId);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<bool> DeletePromotionalItemHeader(string promoId)
  {
    bool isDeleted = await _unitOfWork.PromotionalItems.DeletePromotionalItemHeaderAsync(promoId);
    _unitOfWork.Commit();
    return isDeleted;
  }

  public async Task<StockCodesForModalDto> IsStockCodeValid(bool sellInactive, string stockCode)
  {
    var result = new StockCodesForModalDto();
    var stockCodeMst = await _unitOfWork.StockItems.GetStockMstForPromotion(sellInactive);
    var pluMSt = await _unitOfWork.PLUMasts.GetPLUMast();

    var matchedPluCode = pluMSt.FirstOrDefault(plu => string.IsNullOrWhiteSpace(plu?.PLUCode) ? false :
                        plu.PLUCode.Equals(stockCode, StringComparison.InvariantCultureIgnoreCase));

    if (matchedPluCode != null)
    {
      stockCode = matchedPluCode.PLUPrim;
      var matchedStockCode = stockCodeMst?.FirstOrDefault(scp => scp.StockCode == stockCode);

      if (matchedStockCode != null)
      {
        return matchedStockCode;
      }
    }

    return result;
  }
}
