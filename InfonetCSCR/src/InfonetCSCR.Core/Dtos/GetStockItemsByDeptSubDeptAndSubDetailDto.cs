namespace InfonetCSCR.Core.Dtos;

public class GetStockItemsByDeptSubDeptAndSubDetailDto
{
  public List<Department>? Depts { get; set; }
  public List<SubDepartment>? SubDepts { get; set; }
  public List<SubDetail>? SubDetails { get; set; }
  public int? Length { get; set; } //Do not rename. Used for pagination to define number of items per page
  public int? Start { get; set; }  //Do not rename. Used for pagination to define index of the first item
  public string? SearchValue { get; set; }
  public string? SearchColumnName { get; set; }
  public bool? IsDistinct { get; set; }
  public bool? IsRecentChanges { get; set; }
  public bool? IsShowAllProduct { get; set; }
  public bool? IncludeProductWithoutSubDeptOrSubDetail { get; set; }
}
