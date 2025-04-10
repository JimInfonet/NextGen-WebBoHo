namespace InfonetCSCR.Data.Repositories;

public class SendDataToStoreRepository : BaseRepository, ISendDataToStoreRepository
{
  public SendDataToStoreRepository(IDbTransaction transaction) : base(transaction) { }

  public async Task<bool> CheckIfTableExistsAsync(string databaseName, string tableName)
  {
    var query = $@"
      IF EXISTS (SELECT * FROM {databaseName}.INFORMATION_SCHEMA.TABLES
      WHERE TABLE_NAME = '{tableName}')
      SELECT 1 ELSE SELECT 0";

    return await Connection.ExecuteScalarAsync<bool>(query, null, Transaction);
  }

  public async Task<IEnumerable<string>> GetColumnNamesAsync(string databaseName, string tableName)
  {
    var query = $@"
      SELECT COLUMN_NAME 
      FROM {databaseName}.INFORMATION_SCHEMA.COLUMNS 
      WHERE TABLE_NAME = '{tableName}'";

    return await Connection.QueryAsync<string>(query, null, Transaction);
  }

  public async Task<CheckCreateTableResult> CheckCreateTableAsync(CheckCreateTable param)
  {
    return await Connection.QuerySingleAsync<CheckCreateTableResult>("Sproc_CheckCreateTables", param, Transaction, commandType: CommandType.StoredProcedure);
  }

  public async Task<IEnumerable<IDictionary<string, object>>> GetItemsAsync(TableDto table)
  {
    var query = string.Empty;
    var whereClause = string.Empty;

    if (table.DatabaseName.Equals("CSCTransfer", StringComparison.OrdinalIgnoreCase) && table.Dts)
    {
      whereClause = "WHERE [ReadyForTransferTbl] IS NULL";
    }

    if (table.PrimaryKeys == null || !table.PrimaryKeys.Any())
    {
      if (table.DatabaseName.Equals("CSCTransfer", StringComparison.OrdinalIgnoreCase))
      {
        query = $@"
          DECLARE @COLUMNS NVARCHAR(MAX);
          SELECT @COLUMNS = STUFF((
            SELECT ', ' + '[' + COLUMN_NAME + ']'
            FROM CSCTransfer.INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = '{table.TableName}'
            AND COLUMN_NAME NOT IN ('StoreID', 'ReadyForTransferTbl')
            FOR XML PATH(''), TYPE
          ).value('.', 'NVARCHAR(MAX)'), 1, 2, '');
          EXEC('SELECT DISTINCT ' + @COLUMNS + ' FROM [CSCTransfer].[dbo].[{table.TableName}] {whereClause}');";
      }
      else
      {
        query = $@"SELECT * FROM [{table.DatabaseName}].[dbo].[{table.TableName}]";
      }
    }
    else
    {
      var columns = string.Join(", ", table.PrimaryKeys.Select(pk => $"[{pk}]"));
      query = $@"SELECT DISTINCT {columns} FROM [{table.DatabaseName}].[dbo].[{table.TableName}] {whereClause}";
    }

    var result = await Connection.QueryAsync<dynamic>(query, null, Transaction);
    var itemsDictionary = new List<IDictionary<string, object>>();

    foreach (var row in result)
    {
      var rowAsDict = (IDictionary<string, object>)row;
      itemsDictionary.Add(rowAsDict);
    }

    return itemsDictionary;
  }

  public async Task<IEnumerable<string>> GetSelectedStoreIDsAsync(string tableName)
  {
    const string query = "SELECT [Store_ID] FROM [CSCTransfer].[dbo].[STORE] WHERE TableName = @TableName";
    return await Connection.QueryAsync<string>(query, new { TableName = tableName }, Transaction);
  }

  public async Task<bool> RemoveItemFromTransferDatabaseAsync(TableDto table)
  {
    var sb = new StringBuilder();
    sb.AppendLine($"DELETE FROM [CSCTransfer].[dbo].[{table.TableName}] WHERE");

    var conditions = table.Values.Select(value =>
          "(" + string.Join(" AND ", value.Select(column => $"ISNULL([{column.Key}], '') = '{column.Value ?? string.Empty}'")) + ")"
      );

    sb.AppendLine(string.Join(" OR ", conditions));

    var rowsAffected = await Connection.ExecuteAsync(sb.ToString(), null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> RemoveSelectedStoresFromTransferDatabaseAsync(IEnumerable<string> tableNames)
  {
    const string query = "DELETE FROM [CSCTransfer].[dbo].[STORE] WHERE TableName in @TableNames";
    var rowsAffected = await Connection.ExecuteAsync(query, new { TableNames = tableNames }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> ClearTransferTableAsync(string tableName, bool dts)
  {
    var sb = new StringBuilder();
    sb.AppendLine($"DELETE FROM [CSCTransfer].[dbo].[Exp{tableName}]");

    if (dts)
    {
      sb.AppendLine("WHERE [ReadyForTransferTbl] IS NULL");
    }

    var rowsAffected = await Connection.ExecuteAsync(sb.ToString(), null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> InsertIntoTransferDatabaseAsync(TableDto masterTable, TableDto childTable)
  {
    var sb = new StringBuilder();
    var trDate = DateTime.Now;
    var tableColumns = await GetColumnNamesAsync(masterTable.DatabaseName, childTable.TableName);
    var columns = string.Join(", ", tableColumns.Select(c => $"[{c}]"));
    var childTableColumns = string.Join(", ", tableColumns.Select(c => $"[{childTable.TableName}].[{c}]"));

    sb.AppendLine($"INSERT INTO [CSCTransfer].[dbo].[Exp{childTable.TableName}]");
    sb.AppendLine($"({columns}, [StoreID], [TrDate], [UserCode], [Flag])");
    sb.AppendLine($"SELECT {childTableColumns}, [SelectedStores].[StoreID], '{trDate}', '{masterTable.UserCode}', 'A' FROM [{masterTable.DatabaseName}].[dbo].[{childTable.TableName}]");
    sb.AppendLine("CROSS JOIN (");
    sb.AppendLine($"    SELECT '{string.Join("' AS [StoreID] UNION SELECT '", masterTable.Stores)}' AS [StoreID]");
    sb.AppendLine(") [SelectedStores]");

    if (childTable.TableName.Equals("CARDS", StringComparison.OrdinalIgnoreCase) || childTable.TableName.Equals("CARDCODES", StringComparison.OrdinalIgnoreCase))
    {
      sb.AppendLine($"INNER JOIN [CSCMaster].[dbo].[TendCard] ON [{childTable.TableName}].[CardCode] = [TendCard].[CardCode]");
      sb.AppendLine("WHERE [TendCard].[TendCode] IN (");

      // Dynamically add TendCode values
      var tendCodes = masterTable.Values
        .SelectMany(value => value
          .Where(column => string.Equals(column.Key, "TendCode", StringComparison.OrdinalIgnoreCase))
          .Select(column => $"'{column.Value}'")
        );

      sb.AppendLine(string.Join(", ", tendCodes));
      sb.AppendLine(")");
    }
    else if (childTable.TableName.Equals("EXPCARDMSGTBL", StringComparison.OrdinalIgnoreCase))
    {
      sb.AppendLine($"INNER JOIN [CSCMaster].[dbo].[NegCardstbl] ON [{childTable.TableName}].[Code] = [NegCardstbl].[Code]");
      sb.AppendLine("WHERE [NegCardstbl].[CardNumber] IN (");

      // Dynamically add CardNumber values
      var cardNumbers = masterTable.Values
        .SelectMany(value => value
          .Where(column => string.Equals(column.Key, "CardNumber", StringComparison.OrdinalIgnoreCase))
          .Select(column => $"'{column.Value}'")
        );

      sb.AppendLine(string.Join(", ", cardNumbers));
      sb.AppendLine(")");
    }
    else
    {
      sb.AppendLine("WHERE");

      var conditions = masterTable.Values.Select(value =>
        "(" + string.Join(" AND ", value
          .Where(column => childTable.PrimaryKeys.Any(pk => string.Equals(pk, column.Key, StringComparison.OrdinalIgnoreCase)))
          .Select(column => $"ISNULL([{column.Key}], '') = '{column.Value ?? string.Empty}'")) + ")"
      );

      sb.AppendLine(string.Join(" OR ", conditions));
    }

    var rowsAffected = await Connection.ExecuteAsync(sb.ToString(), null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> SaveSelectedStoresAsync(string tableName, IEnumerable<string> stores)
  {
    var sb = new StringBuilder();
    sb.AppendLine("INSERT INTO [CSCTransfer].[dbo].[STORE]([Store_Id], [TableName])");
    sb.AppendLine("VALUES");
    var values = stores.Select(store => $"('{store}', '{tableName}')");
    sb.AppendLine(string.Join(", ", values));

    var rowsAffected = await Connection.ExecuteAsync(sb.ToString(), null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteDuplicatesAsync(TableDto table)
  {
    var primaryColumns = string.Join(", ", table.PrimaryKeys.Select(pk => $"[{pk}]"));
    var query = $@"
      WITH CTE AS (
        SELECT ROW_NUMBER() OVER (PARTITION BY {primaryColumns}, [StoreID] ORDER BY [TrDate] DESC) AS RowNumber
	      FROM [CSCTransfer].[dbo].[Exp{table.TableName}]
      )
      DELETE FROM CTE WHERE RowNumber > 1;";

    var rowsAffected = await Connection.ExecuteAsync(query, null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateItemsReadyForTransferStatusAsync(string tableName)
  {
    var query = $"UPDATE [CSCTransfer].[dbo].[Exp{tableName}] SET [ReadyForTransferTbl] = 1";
    var rowsAffected = await Connection.ExecuteAsync(query, null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateSelectedStoresTransferStatusAsync(IEnumerable<string> tableNames)
  {
    const string query = @"
      UPDATE [CSCTransfer].[dbo].[STORE] 
      SET 
        [ReadyForTransferTbl] = [TableName], 
        [TableName] = '' 
      WHERE [TableName] in @TableNames";

    var rowsAffected = await Connection.ExecuteAsync(query, new { TableNames = tableNames }, Transaction);
    return rowsAffected > 0;
  }

  public async Task<IEnumerable<StoreOptionDto>> GetDataPathsAsync(IEnumerable<string> storeIDs)
  {
    const string query = @"
      SELECT 
        [Store_ID] AS [StoreId],
        [Data_Path] AS [DataPath]
      FROM [CSCMaster].[dbo].[STORE] 
      WHERE [Store_ID] in @StoreIDs";

    return await Connection.QueryAsync<StoreOptionDto>(query, new { StoreIDs = storeIDs }, Transaction);
  }

  public async Task<bool> ArchiveExpTableDataAsync(string tableName)
  {
    var query = $@"
      INSERT INTO [CSCTransfer].[dbo].[ArchExp{tableName}]
      SELECT * FROM [CSCTransfer].[dbo].[Exp{tableName}]";

    var rowsAffected = await Connection.ExecuteAsync(query, null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> DeleteExpTableData(string tableName)
  {
    var query = $"DELETE FROM [CSCTransfer].[dbo].[Exp{tableName}]";
    var rowsAffected = await Connection.ExecuteAsync(query, null, Transaction);
    return rowsAffected > 0;
  }

  public async Task<bool> UpdateFlagAsync(TableDto masterTable, TableDto targetTable)
  {
    var sb = new StringBuilder();
    sb.AppendLine($@"
      UPDATE [CSCTransfer].[dbo].[Exp{targetTable.TableName}] 
      SET [Flag] = '{masterTable.Flag}'");

    if (targetTable.TableName.Equals("CARDS", StringComparison.OrdinalIgnoreCase) || targetTable.TableName.Equals("CARDCODES", StringComparison.OrdinalIgnoreCase))
    {
      sb.AppendLine("WHERE [CardCode] IN (");
      sb.AppendLine(@"
        SELECT [CardCode] FROM [CSCMaster].[dbo].[TendCard]
        WHERE [TendCode] IN (");

      // Dynamically add TendCode values
      var tendCodes = masterTable.Values
        .SelectMany(value => value
          .Where(column => string.Equals(column.Key, "TendCode", StringComparison.OrdinalIgnoreCase))
          .Select(column => $"'{column.Value}'")
        );

      sb.AppendLine(string.Join(", ", tendCodes));
      sb.AppendLine("))");
    }
    else if (targetTable.TableName.Equals("EXPCARDMSGTBL", StringComparison.OrdinalIgnoreCase))
    {
      sb.AppendLine("WHERE [Code] IN (");
      sb.AppendLine(@"
        SELECT [Code] FROM [CSCMaster].[dbo].[NegCardstbl]
        WHERE [CardNumber] IN (");

      // Dynamically add TendCode values
      var cardNumbers = masterTable.Values
        .SelectMany(value => value
          .Where(column => string.Equals(column.Key, "CardNumber", StringComparison.OrdinalIgnoreCase))
          .Select(column => $"'{column.Value}'")
        );

      sb.AppendLine(string.Join(", ", cardNumbers));
      sb.AppendLine("))");
    }
    else
    {
      sb.AppendLine("WHERE");

      var conditions = masterTable.Values.Select(value =>
        "(" + string.Join(" AND ", value
          .Where(column => targetTable.PrimaryKeys.Any(pk => string.Equals(pk, column.Key, StringComparison.OrdinalIgnoreCase)))
          .Select(column => $"ISNULL([{column.Key}], '') = '{column.Value ?? string.Empty}'")) + ")"
      );

      sb.AppendLine(string.Join(" OR ", conditions));
    }

    var rowsAffected = await Connection.ExecuteAsync(sb.ToString(), null, Transaction);
    return rowsAffected > 0;
  }
}
