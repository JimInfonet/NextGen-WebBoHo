namespace InfonetCSCR.Core.Dtos;

public class GetDataDefinitionsByCriteriaDto
{
  public string ProductId { get; set; } = default!;
  public string StoreId { get; set; } = default!;
  public string DefType { get; set; } = default!;
  public string? VendorId { get; set; }
}
