namespace InfonetCSCR.Core.Models;

public class Store
{
  public string Store_ID { get; set; } = default!;
  public string? Store_Desc { get; set; }
  public string? GroupId { get; set; }
  public string? Addr1 { get; set; }
  public string? City { get; set; }
  public string? Province { get; set; }
  public string? Post_Code { get; set; }
  public string? Phone { get; set; }
  public string? Fax { get; set; }
  public string? Contact { get; set; }
  public string? Data_Path { get; set; }
  public bool? IsWarehouse { get; set; }
}
