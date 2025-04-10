namespace InfonetCSCR.Core.Abstractions.Repositories;

public interface ISendDataToStoreRepository
{
  Task<bool> CheckIfTableExistsAsync(string databaseName, string tableName);
  Task<IEnumerable<string>> GetColumnNamesAsync(string databaseName, string tableName);
  Task<CheckCreateTableResult> CheckCreateTableAsync(CheckCreateTable param);
  Task<IEnumerable<IDictionary<string, object>>> GetItemsAsync(TableDto table);
  Task<IEnumerable<string>> GetSelectedStoreIDsAsync(string tableName);
  Task<bool> RemoveItemFromTransferDatabaseAsync(TableDto table);
  Task<bool> RemoveSelectedStoresFromTransferDatabaseAsync(IEnumerable<string> tableNames);
  Task<bool> ClearTransferTableAsync(string tableName, bool dts);
  Task<bool> SaveSelectedStoresAsync(string tableName, IEnumerable<string> stores);
  Task<bool> InsertIntoTransferDatabaseAsync(TableDto masterTable, TableDto childTable);
  Task<bool> DeleteDuplicatesAsync(TableDto table);
  Task<bool> UpdateItemsReadyForTransferStatusAsync(string tableName);
  Task<bool> UpdateSelectedStoresTransferStatusAsync(IEnumerable<string> tableNames);
  Task<IEnumerable<StoreOptionDto>> GetDataPathsAsync(IEnumerable<string> storeIDs);
  Task<bool> ArchiveExpTableDataAsync(string tableName);
  Task<bool> DeleteExpTableData(string tableName);
  Task<bool> UpdateFlagAsync(TableDto masterTable, TableDto targetTable);
}
