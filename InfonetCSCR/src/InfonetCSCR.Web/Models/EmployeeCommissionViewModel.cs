namespace InfonetCSCR.Web.Models
{
  public class EmployeeCommissionViewModel
  {
    public string? EmployeeID { get; set; }
    public string? StockCode { get; set; }
    public string? Dept { get; set; }
    public string? SubDept { get; set; }
    public string? SubDetail { get; set; }
    public string? Name { get; set; }
    public decimal? Commission { get; set; }
    public decimal? CommissionByDol { get; set; }
    public bool? ByStockCode { get; set; }
    public bool? Apply { get; set; }
  }
}
