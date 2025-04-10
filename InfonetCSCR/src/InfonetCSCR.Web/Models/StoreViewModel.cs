namespace InfonetCSCR.Web.Models;

public class StoreViewModel
{
  [Required(ErrorMessage = "storeIdRequired")]
  [MaxLength(6)]
  public string Store_ID { get; set; } = default!;
  [Required(ErrorMessage = "storeNameRequired")]
  [MaxLength(30)]
  public string? Store_Desc { get; set; }
  public string? PriceGroupId { get; set; }
  public string? RegionId { get; set; }
  public string? Addr1 { get; set; }
  public string? City { get; set; }
  public string? Province { get; set; }
  public string? Post_Code { get; set; }
  public string? Phone { get; set; }
  public string? Fax { get; set; }
  public string? Contact { get; set; }
  public string? Data_Path { get; set; }
  public bool? IsWarehouse { get; set; }
  public string? IsAction { get; set; }
  public string? CopyStoreId { get; set; }
  public bool IsWarehouseCheckBox
  {
    get => this.IsWarehouse.GetValueOrDefault();
    set { this.IsWarehouse = value; }
  }
}
