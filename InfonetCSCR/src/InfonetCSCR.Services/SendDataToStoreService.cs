namespace InfonetCSCR.Services;

public class SendDataToStoreService : ISendDataToStoreService
{
  private readonly UnitOfWork _unitOfWork;

  public SendDataToStoreService(IApplicationConfig config)
  {
    _unitOfWork = new UnitOfWork(config, config.CSCMasterConnectionString);
  }

  public async Task<bool> CheckIfTableExistsAsync(string databaseName, string tableName)
  {
    var result = await _unitOfWork.SendDataToStore.CheckIfTableExistsAsync(databaseName, tableName);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<string>> GetColumnNamesAsync(string databaseName, string tableName)
  {
    var items = await _unitOfWork.SendDataToStore.GetColumnNamesAsync(databaseName, tableName);
    _unitOfWork.Commit();
    return items;
  }

  public async Task<CheckCreateTableResult> CheckCreateTableAsync(CheckCreateTable param)
  {
    var result = await _unitOfWork.SendDataToStore.CheckCreateTableAsync(param);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<IDictionary<string, object>>> GetItemsAsync(TableDto table)
  {
    var result = await _unitOfWork.SendDataToStore.GetItemsAsync(table);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<IEnumerable<string>> GetSelectedStoreIDsAsync(string tableName)
  {
    var result = await _unitOfWork.SendDataToStore.GetSelectedStoreIDsAsync(tableName);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> RemoveSelectedStoresFromTransferDatabaseAsync(IEnumerable<string> tableNames)
  {
    var result = await _unitOfWork.SendDataToStore.RemoveSelectedStoresFromTransferDatabaseAsync(tableNames);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> RemoveItemFromTransferDatabaseAsync(TableDto table)
  {
    var result = await _unitOfWork.SendDataToStore.RemoveItemFromTransferDatabaseAsync(table);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> ClearTransferTableAsync(string tableName, bool dts)
  {
    var result = await _unitOfWork.SendDataToStore.ClearTransferTableAsync(tableName, dts);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> CreateTableAsync(TableDto masterTable)
  {
    var insertTasks = new List<Task>();
    var deleteTasks = new List<Task>
    {
      _unitOfWork.SendDataToStore.RemoveSelectedStoresFromTransferDatabaseAsync(masterTable.RelatedTables.Select(table => table.TableName))
    };

    foreach (var table in masterTable.RelatedTables)
    {
      deleteTasks.Add(_unitOfWork.SendDataToStore.ClearTransferTableAsync(table.TableName, masterTable.Dts));
      insertTasks.Add(_unitOfWork.SendDataToStore.InsertIntoTransferDatabaseAsync(masterTable, table));
      insertTasks.Add(_unitOfWork.SendDataToStore.SaveSelectedStoresAsync(table.TableName, masterTable.Stores));
    }

    await Task.WhenAll(deleteTasks);
    await Task.WhenAll(insertTasks);
    return _unitOfWork.Commit();
  }

  public async Task<bool> TransferDataAsync(TableDto masterTable)
  {
    var tasks = new List<Task>
    {
      _unitOfWork.SendDataToStore.UpdateSelectedStoresTransferStatusAsync(masterTable.RelatedTables.Select(table => table.TableName))
    };

    foreach (var table in masterTable.RelatedTables)
    {
      tasks.Add(_unitOfWork.SendDataToStore.DeleteDuplicatesAsync(table));
      tasks.Add(_unitOfWork.SendDataToStore.UpdateItemsReadyForTransferStatusAsync(table.TableName));
    }

    await Task.WhenAll(tasks);
    return _unitOfWork.Commit();
  }

  public async Task<bool> ArchiveExpTableDataAsync(TableDto masterTable)
  {
    var relatedTableNames = masterTable.RelatedTables.Select(table => table.TableName);
    var tasks = new List<Task>
    {
      _unitOfWork.SendDataToStore.RemoveSelectedStoresFromTransferDatabaseAsync(relatedTableNames)
    };

    foreach (var tableName in relatedTableNames)
    {
      var tableTasks = Task.Run(async () =>
      {
        await _unitOfWork.SendDataToStore.ArchiveExpTableDataAsync(tableName);
        await _unitOfWork.SendDataToStore.DeleteExpTableData(tableName);
      });

      tasks.Add(tableTasks);
    }

    await Task.WhenAll(tasks);
    return _unitOfWork.Commit();
  }

  public async Task<bool> DeleteExpTableDataAsync(string[] tableNames)
  {
    foreach (var tableName in tableNames)
    {
      await _unitOfWork.SendDataToStore.DeleteExpTableData(tableName);
    }
    return _unitOfWork.Commit();
  }

  public async Task<IEnumerable<StoreOptionDto>> GetDataPathsAsync(IEnumerable<string> storeIDs)
  {
    var result = await _unitOfWork.SendDataToStore.GetDataPathsAsync(storeIDs);
    _unitOfWork.Commit();
    return result;
  }

  public async Task<bool> UpdateFlagAsync(TableDto masterTable)
  {
    var tasks = new List<Task>();

    foreach(var table in masterTable.RelatedTables)
    {
      tasks.Add(_unitOfWork.SendDataToStore.UpdateFlagAsync(masterTable, table));
    }

    await Task.WhenAll(tasks);
    return _unitOfWork.Commit();
  }
}
