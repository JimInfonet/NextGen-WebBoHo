namespace InfonetCSCR.Web.Models;

public class PendingCommodityTransferDtoViewModel
{
  public List<string> StoreIds { get; set; }
  public List<DepartmentViewModel> Departments { get; set; }
  public List<SubDepartmentViewModel> SubDepartments { get; set; }
  public List<SubDetailViewModel> SubDetails { get; set; }
  public Dictionary<string, string> TransTypes { get; set; }
}
