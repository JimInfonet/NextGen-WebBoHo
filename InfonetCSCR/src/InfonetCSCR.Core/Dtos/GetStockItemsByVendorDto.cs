namespace InfonetCSCR.Core.Dtos;

public class GetStockItemsByVendorDto
{
  public string VendorId { get; set; }
  public int? Length { get; set; } //Do not rename. Used for pagination to define number of items per page
  public int? Start { get; set; }  //Do not rename. Used for pagination to define index of the first item
  public string? SearchValue { get; set; }
  public string? SearchColumnName { get; set; }
  public string? DataTransferType { get; set; }
}
