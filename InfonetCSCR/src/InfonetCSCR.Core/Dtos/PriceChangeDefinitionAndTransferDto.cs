
namespace InfonetCSCR.Core.Dtos;

public class PriceChangeDefinitionAndTransferDto
{
  public List<PriceDefinition>? PriceDefinitions { get; set; }
  public List<PriceChangeTransfer>? PriceChangeTransfers { get; set; }
}
