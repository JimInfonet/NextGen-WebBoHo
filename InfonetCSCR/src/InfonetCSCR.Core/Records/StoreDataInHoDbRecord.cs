namespace InfonetCSCR.Core.Records;

public class StoreDataInHoDbRecord
{
  public IList<string>? NotExistsInGradeTable { get; set; }
  public IList<string>? NotExistsInTierTable { get; set; }
  public IList<string>? NotExistsInServiceLevelTable { get; set; }
  public IList<string>? NotExistsInTierLevelPriceDiffTable { get; set; }
  public bool? InSameTierLevel { get; set; }
  public bool? InSameServiceLevel { get; set; }
  public IList<string>? StoreIdsThatCannotBeAdded { get; set; }
}
