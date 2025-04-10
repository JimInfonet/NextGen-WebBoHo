namespace InfonetCSCR.Core.Dtos;

public class PendingPromotionTransferDto
{
  public IEnumerable<CreatePromotionTransferDto> PromotionTransferItems { get; set; }
  public IEnumerable<string> StoreIds { get; set; }
}
