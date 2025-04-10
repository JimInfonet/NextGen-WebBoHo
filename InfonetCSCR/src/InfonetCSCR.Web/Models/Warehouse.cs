namespace InfonetCSCR.Web.Models;

public class Warehouse
{
  public string StoreID { get; set; } = default!;
  public string? StoreDescription { get; set; }
  public bool? IsWarehouse { get; set; }
}
