namespace InfonetCSCR.Core.Dtos;

public class StockProcessParametersDto
{
  public string? UserCode { get; set; }
  public int Purpose { get; set; }
  public int NumPricePolicy { get; set; }
  public bool IsCostMarkupPolicy { get; set; }
  public bool IsChangePrice { get; set; }
  public bool UpdateNetworkPolicy { get; set; }
  public bool SetVendPolicy { get; set; }
}
