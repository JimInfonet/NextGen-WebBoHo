namespace InfonetCSCR.Web.Abstractions.Services;

public interface ISendDataToStoreService
{
  Task<IEnumerable<RegionViewModel>> GetRegionsAsync();
  Task<IEnumerable<StoreViewModel>> GetAllStoresListAsync();
  Task<IEnumerable<StoreViewModel>> GetStoresByRegionAsync(IEnumerable<string> regionIds);
  Task<SelectList> GetDataTransferTypeListAsync();
  Task<SendDataToStoreItems> GetItemsAsync(string tableName);
  Task<bool> RemoveItemFromTransferDatabaseAsync(RemoveItemRequestViewModel request);
  Task<bool> RemoveSelectedStoresFromTransferDatabaseAsync(string tableName);
  Task<SendDataToStoreItems> CreateTableAsync(SendDataToStoreItems request);
  Task<bool> DeleteExpTableDataAsync(string tableName);
  Task<DataTransferRequestResponseViewModel> TransferDataAsync(SendDataToStoreItems request);
  Task<bool> UpdateFlagAsync(SendDataToStoreItems request);
}
