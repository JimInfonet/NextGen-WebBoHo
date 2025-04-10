namespace InfonetCSCR.Web.Models;

public class GetStockItemsByDeptSubDeptAndSubDetailViewModel
{
  public List<DepartmentViewModel>? depts { get; set; }
  public List<SubDepartmentViewModel>? subDepts { get; set; }
  public List<SubDetailViewModel>? subDetails{ get; set; }
  public int? Length { get; set; } //Do not rename. Used for pagination to define number of items per page
  public int? Start { get; set; }  //Do not rename. Used for pagination to define index of the first item
  public string? SearchValue { get; set; }
  public string? SearchColumnName { get; set; }
  public bool? IsDistinct { get; set; }
  public bool? IsRecentChanges { get; set; }
  public bool? IsShowAllProduct { get; set; }
  public bool? IncludeProductWithoutSubDeptOrSubDetail { get; set; }
}
