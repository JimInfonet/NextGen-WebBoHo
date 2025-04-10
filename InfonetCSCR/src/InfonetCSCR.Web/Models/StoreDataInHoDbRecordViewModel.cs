namespace InfonetCSCR.Web.Models;

public class StoreDataInHoDbRecordViewModel
{
  public IList<string>? NotExistsInGradeTable { get; set; }
  public IList<string>? NotExistsInTierTable { get; set; }
  public IList<string>? NotExistsInServiceLevelTable { get; set; }
  public IList<string>? NotExistsInTierLevelPriceDiffTable { get; set; }
  public bool? InSameTierLevel { get; set; }
  public bool? InSameServiceLevel { get; set; }
  public IList<string>? StoreIdsThatCannotBeAdded { get; set; }
}
