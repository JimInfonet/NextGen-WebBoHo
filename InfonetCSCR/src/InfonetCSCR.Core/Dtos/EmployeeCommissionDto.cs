namespace InfonetCSCR.Core.Dtos
{
  public class EmployeeCommissionDto
  {
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? StockCode { get; set; }
    public string? Description { get; set; }
    public DateTime? ShiftDate { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public string? Dept { get; set; }
    public string? SubDept { get; set; }
    public string? SubDetail { get; set; }
    public int RowNumber { get; set; }
  }
}
