namespace InfonetCSCR.Core.Dtos;

public class StockTakesAndReceiveRequestDto
{
  public int Purpose { get; set; }
  public int Length { get; set; }
  public int Start { get; set; }
  public string? SearchValue { get; set; }
  public string? SearchColumnName { get; set; }
}
