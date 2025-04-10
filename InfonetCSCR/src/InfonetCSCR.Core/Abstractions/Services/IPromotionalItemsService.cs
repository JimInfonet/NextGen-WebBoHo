namespace InfonetCSCR.Core.Abstractions.Services;

public interface IPromotionalItemsService
{
  Task<IEnumerable<PromoHeader>> GetPromoHeaders();
  Task<PromoHeader> GetPromoHeader(string promoId);
  Task<IEnumerable<PromoDetail>> GetPromoDetail(string promoId);
  Task<bool> IsPromoIdExist(string promoId);
  Task<bool> AddPromotionalItemHeader(PromoHeader promoHeader);
  Task<bool> UpdatePromotionalItemHeader(PromoHeader promoHeader);
  Task<bool> AddPromotionalItemDetials(IEnumerable<PromoDetail> promoDetails);
  Task<bool> DeletePromotionalItemDetails(string promoId);
  Task<bool> DeletePromotionalItemHeader(string promoId);
  Task<StockCodesForModalDto> IsStockCodeValid(bool sellInactive, string stockCode);
}
