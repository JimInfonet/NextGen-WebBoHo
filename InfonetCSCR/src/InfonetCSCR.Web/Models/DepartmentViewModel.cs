namespace InfonetCSCR.Web.Models;

public class DepartmentViewModel
{
  public string DeptId { get; set; } = default!;
  public string DeptName { get; set; } = default!;
  public int EODDetail { get; set; }
  public int EODGroup { get; set; }
  public int CntDetail { get; set; }
}
