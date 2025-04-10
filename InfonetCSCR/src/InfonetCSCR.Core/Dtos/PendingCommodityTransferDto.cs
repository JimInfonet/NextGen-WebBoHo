namespace InfonetCSCR.Core.Dtos;

public class PendingCommodityTransferDto
{
  public List<string> StoreIds { get; set; }
  public List<Department> Departments { get; set; }
  public List<SubDepartment> SubDepartments { get; set; }
  public List<SubDetail> SubDetails { get; set; }
  public Dictionary<string, string> TransTypes { get; set; }
}
