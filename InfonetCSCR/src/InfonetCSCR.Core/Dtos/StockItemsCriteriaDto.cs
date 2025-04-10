namespace InfonetCSCR.Core.Dtos;

public class StockItemsCriteriaDto
{
	public string? Department { get; set; }
	public string? SubDept { get; set; }
	public string? SubDetail { get; set; }
	public string? AllOption { get; set; }
	public bool SellInactivePolicy { get; set; }
}

