namespace InfonetCSCR.Core.Dtos;

public class DeleteProductDto
{
  public string StockCode { get; set; } = default!;
  public string UserCode { get; set; } = default!;
  public bool BatchDelete { get; set; }
  public bool UseTaxExempt { get; set; }
}
