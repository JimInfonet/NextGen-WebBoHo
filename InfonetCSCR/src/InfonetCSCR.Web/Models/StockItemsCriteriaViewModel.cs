namespace InfonetCSCR.Web.Models;

public class StockItemsCriteriaViewModel
{
	public string? Department { get; set; }
	public string? SubDept { get; set; }
	public string? SubDetail { get; set; }
	public string? AllOption { get; set; }
	public bool SellInactivePolicy { get; set; }
}
