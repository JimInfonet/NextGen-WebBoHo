using InfonetCSCR.Core.Enums;

namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface IAddItemsToThePORepository
{
  Task<IEnumerable<VendorPartDetailsDto>> GetVendorPartsAsync(string vendorId, AddItemsToThePOSelectedCriteria selectedCriteria);
  Task<PoDiscountAndTaxIncludeDto> GetPoDiscountAndTaxIncludeAsync(string vendorId);
  Task<IEnumerable<string>> GetExistingPartNosAsync(string purchaseNo, List<string> partNos);
}
