namespace InfonetCSCR.Web.Models;

public class SubDepartmentViewModel
{
  public string DeptId { get; set; } = default!;
  public string SubDeptId { get; set; } = default!;
  public string SubDeptName { get; set; } = default!;
  public string? CombinedId { get; set; }
}
