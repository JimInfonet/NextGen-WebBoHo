namespace InfonetCSCR.Core.Dtos;

public class DeleteFromTransferListDto
{
  public string DataTransferType { get; set; }
  public string? BatchId { get; set; }
  public string? ItemId { get; set; } // Would be different based on data transfer type, so, gave it a generic name 
  public string? Description { get; set; }
  public string? StoreId { get; set; }
  public bool? DeleteAll { get; set; }
}
