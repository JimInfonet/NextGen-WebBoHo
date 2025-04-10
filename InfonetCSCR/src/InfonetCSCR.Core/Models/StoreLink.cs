namespace InfonetCSCR.Core.Models;

public class StoreLink
{
  public string Store_ID { get; set; } = default!;
  public string? Store_Desc { get; set; }
  public string? Region_Id { get; set; }
  public string? PriceGroup_Id { get; set; }
}
