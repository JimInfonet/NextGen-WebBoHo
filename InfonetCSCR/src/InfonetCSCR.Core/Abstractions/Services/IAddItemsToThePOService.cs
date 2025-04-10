namespace InfonetCSCR.Core.Abstractions.Services;

public interface IAddItemsToThePOService
{
  Task<List<VendorPartDetailsDto>> GetVendorParts(string vendorId, AddItemsToThePOSelectedCriteria selectedCriteria);
  Task<List<string>> GetExistingPartNos(string purchaseNo, List<string> partNos);
  Task AddItemsToThePO(AddVendorPartsDto model);
}
