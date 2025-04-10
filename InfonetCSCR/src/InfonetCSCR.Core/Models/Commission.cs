namespace InfonetCSCR.Core.Models;

public class Commission
{
	public string? EmployeeID { get; set; }
	public string? EmployeeName { get; set; }
	public string? Department { get; set; }
	public string? SubDept { get; set; }
	public string? SubDetail { get; set; }
	public string? StockCode { get; set; }
	public string? GroupName { get; set; }
	public float CommissionPercentage { get; set; }
	public float CommissionAmount { get; set; }
	public bool Apply { get; set; }
	public string? AllOption { get; set; }
	public bool ByStockCode { get; set; }
	public bool SellInactivePolicy { get; set; }
}
