namespace InfonetCSCR.Web.Models;

public class SubDetailViewModel
{
  public string DeptId { get; set; } = default!;
  public string SubDeptId { get; set; } = default!;
  public string SubDetailId { get; set; } = default!;
  public string SubDetailName { get; set; } = default!;
  public string? CombinedId { get; set; }
}
