namespace InfonetCSCR.Core.Abstractions.Services;

public interface ISendDataToStoreService
{
  Task<bool> CheckIfTableExistsAsync(string databaseName, string tableName);
  Task<IEnumerable<string>> GetColumnNamesAsync(string databaseName, string tableName);
  Task<CheckCreateTableResult> CheckCreateTableAsync(CheckCreateTable param);
  Task<IEnumerable<IDictionary<string, object>>> GetItemsAsync(TableDto table);
  Task<IEnumerable<string>> GetSelectedStoreIDsAsync(string tableName);
  Task<bool> RemoveItemFromTransferDatabaseAsync(TableDto table);
  Task<bool> RemoveSelectedStoresFromTransferDatabaseAsync(IEnumerable<string> tableNames);
  Task<bool> ClearTransferTableAsync(string tableName, bool dts);
  Task<bool> CreateTableAsync(TableDto table);
  Task<bool> TransferDataAsync(TableDto table);
  Task<bool> ArchiveExpTableDataAsync(TableDto table);
  Task<bool> DeleteExpTableDataAsync(string[] tableNames);
  Task<IEnumerable<StoreOptionDto>> GetDataPathsAsync(IEnumerable<string> storeIDs);
  Task<bool> UpdateFlagAsync(TableDto masterTable);
}
