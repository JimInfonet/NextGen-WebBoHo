namespace InfonetCSCR.Web.Abstractions.Services;

public interface IPromotionalItemsService
{
  Task<bool> IsPromoIdExist(string promoId);
  Task SavePromotionalItems(PromoHeaderViewModel model);
}
