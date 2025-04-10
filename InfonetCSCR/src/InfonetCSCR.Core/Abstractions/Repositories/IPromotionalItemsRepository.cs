namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IPromotionalItemsRepository
{
  Task<IEnumerable<PromoHeader>> GetPromoHeadersAsync();
  Task<PromoHeader> GetPromoHeaderAsync(string PromoId);
  Task<IEnumerable<PromoDetail>> GetPromoDetailAsync(string PromoId);
  Task<IEnumerable<string>> GetAllPromoIdsAsync();
  Task<bool> AddPromotionalItemHeaderAsync(PromoHeader promoHeader);
  Task<bool> UpdatePromotionalItemHeaderAsync(PromoHeader promoHeader);
  Task<bool> AddPromotionalItemDetailsAsync(IEnumerable<PromoDetail> promoDetails);
  Task<bool> DeletePromotionalItemDetailsAsync(string promoId);
  Task<bool> DeletePromotionalItemHeaderAsync(string promoId);
  Task<int> IsPromoIdExist(string PromoId);
}
