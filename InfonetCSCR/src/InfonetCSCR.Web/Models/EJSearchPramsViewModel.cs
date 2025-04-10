namespace InfonetCSCR.Web.Models;

public class EJSearchParamViewModel
{
  public string? StartDate { get; set; }
  public string? EndDate { get; set; }
  public string? SearchDate { get; set; }
  public string? SearchOperator { get; set; }
  public bool? IsWareHouse { get; set; }
  public char? StoreRadioOption { get; set; }
  public bool? IsLocalStoreSelected { get; set; }
  public string? SelectedStoreId { get; set; }
  public int? Length { get; set; } //Do not rename. Used for pagination to define number of items per page
  public int? Start { get; set; } //Do not rename. Used for pagination to define number of items per page
  public IEnumerable<EJSearchDescriptionViewModel>? EJSearchDescription { get; set; }
}
