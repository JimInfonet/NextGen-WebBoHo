namespace InfonetCSCR.Web.Models;

public class PendingPromotionTransferDtoViewModel
{
  public IEnumerable<CreatePromotionTransferDtoViewModel> PromotionTransferItems { get; set; }
  public IEnumerable<string> StoreIds {get; set;}
}
