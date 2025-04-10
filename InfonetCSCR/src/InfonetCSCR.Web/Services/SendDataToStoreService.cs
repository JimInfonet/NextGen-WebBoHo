namespace InfonetCSCR.Web.Services;

public class SendDataToStoreService : ISendDataToStoreService
{
  private readonly IHttpService _http;
  private readonly IStringLocalizer<SendDataToStoreService> _localizer;
  private readonly string _dataTransferConfigFilePath;
  private readonly IPolicyService _policyService;
  private readonly IHttpContextAccessor _contextAccessor;

  public SendDataToStoreService(IHttpService http, IApplicationConfig config, IStringLocalizer<SendDataToStoreService> localizer, IPolicyService policyService, IHttpContextAccessor contextAccessor)
  {
    _http = http;
    _localizer = localizer;
    _dataTransferConfigFilePath = config.FileConfig.DataTransferConfigFilePath;
    _policyService = policyService;
    _contextAccessor = contextAccessor;
  }

  public async Task<IEnumerable<RegionViewModel>> GetRegionsAsync()
  {
    const string url = "Region/GetRegion";
    return await _http.GetAndParseAsync<IEnumerable<RegionViewModel>>(url);
  }

  public async Task<IEnumerable<StoreViewModel>> GetAllStoresListAsync()
  {
    const string url = "Store/GetAllStoresList";
    return await _http.GetAndParseAsync<IEnumerable<StoreViewModel>>(url);
  }

  public async Task<IEnumerable<StoreViewModel>> GetStoresByRegionAsync(IEnumerable<string> regionIds)
  {
    const string url = "DataDefinition/GetStoresByRegionIds";
    return await _http.PostAndParseAsync<IEnumerable<StoreViewModel>>(url, regionIds);
  }

  public async Task<SelectList> GetDataTransferTypeListAsync()
  {
    if (!File.Exists(_dataTransferConfigFilePath))
    {
      throw new FileNotFoundException(_localizer.GetString("fileNotFound"));
    }

    var dataTransferTypes = new List<SelectListItem>();
    var lines = File.ReadAllLines(_dataTransferConfigFilePath);
    var tablesSectionStarted = false;

    foreach (var line in lines)
    {
      if (line.StartsWith("[END]", StringComparison.OrdinalIgnoreCase))
      {
        break;
      }

      if (!tablesSectionStarted)
      {
        if (line.StartsWith("[TABLES]", StringComparison.OrdinalIgnoreCase))
        {
          tablesSectionStarted = true;
        }

        continue;
      }

      if (!line.StartsWith("["))
      {
        var parts = line.Split(',');
        var tableName = parts[0].Trim();
        var databaseName = GetDatabaseName(tableName);

        if (await CheckIfTableExistsAsync(databaseName, tableName))
        {
          var description = parts.Length > 1 ? parts[1].Trim() : tableName;
          description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(description);
          dataTransferTypes.Add(new SelectListItem(description, tableName));
        }
      }
    }

    if (!dataTransferTypes.Any())
    {
      throw new Exception(_localizer.GetString("noFilesAvailableForExport"));
    }

    return new SelectList(dataTransferTypes, "Value", "Text");
  }

  public async Task<SendDataToStoreItems> GetItemsAsync(string tableName)
  {
    var masterTable = new DataTransferViewModel()
    {
      TableName = tableName,
      DatabaseName = GetDatabaseName(tableName)
    };

    masterTable.RelatedTables = await ParseRelatedTablesAsync(masterTable.DatabaseName, tableName);
    await ParseReferencesAsync(masterTable);
    await ParsePrimaryKeysAsync(masterTable);
    await ParseDescriptionsAsync(masterTable);
    await CheckCreateTableAsync(masterTable);

    var item = new SendDataToStoreItems
    {
      MasterTableItems = await FetchItemsAsync(masterTable.DatabaseName, masterTable.TableName, masterTable.PrimaryKeys.Union(masterTable.Descriptions), true),
      SelectedItems = await FetchItemsAsync("CSCTransfer", $"Exp{masterTable.TableName}", masterTable.PrimaryKeys, true),
      SelectedStoreIDs = await GetSelectedStoreIDsAsync(masterTable.TableName),
      PrimaryColumns = masterTable.PrimaryKeys,
      DescriptionColumns = masterTable.Descriptions,
    };

    return item;
  }

  public async Task<bool> RemoveItemFromTransferDatabaseAsync(RemoveItemRequestViewModel request)
  {
    var param = new
    {
      TableName = $"Exp{request.TableName}",
      Values = request.Items
    };

    var url = $"SendDataToStore/RemoveItemFromTransferDatabase";
    return await _http.PostAndParseAsync<bool>(url, param);
  }

  public async Task<bool> RemoveSelectedStoresFromTransferDatabaseAsync(string masterTableName)
  {
    var relatedTables = await ParseRelatedTablesAsync(GetDatabaseName(masterTableName), masterTableName);
    var tableNames = relatedTables.Select(table => table.TableName).Distinct();
    const string url = "SendDataToStore/RemoveSelectedStoresFromTransferDatabase";
    return await _http.PostAndParseAsync<bool>(url, tableNames);
  }

  public async Task<bool> DeleteExpTableDataAsync(string tableName)
  {
    const string url = "SendDataToStore/DeleteExpTableData";
    var databaseName = GetDatabaseName(tableName);
    IEnumerable<DataTransferViewModel> RelatedTables = await ParseRelatedTablesAsync(databaseName, tableName);
    string[] tableNames = RelatedTables
    .Where(rt => rt.TableName != null)
    .Select(rt => rt.TableName)
    .ToArray();
    return await _http.PostAndParseAsync<bool>(url, tableNames);
  }

  public async Task<SendDataToStoreItems> CreateTableAsync(SendDataToStoreItems request)
  {
    var items = new SendDataToStoreItems
    {
      LocalizedTitle = new Dictionary<string, string> {{ "StoreID", _localizer.GetString("StoreID") }}
    };

    var databaseName = GetDatabaseName(request.TableName);
    var param = new
    {
      DatabaseName = databaseName,
      request.TableName,
      Values = request.SelectedItems,
      Stores = request.SelectedStoreIDs,
      Dts = string.Equals(_policyService.GetPolicy(PolicyName.TRANS_DATA), "DTS", StringComparison.OrdinalIgnoreCase),
      RelatedTables = await ParseRelatedTablesAsync(databaseName, request.TableName),
      UserCode = _contextAccessor.HttpContext?.Session.GetString(SessionKeys.UserCode)
    };

    const string url = "SendDataToStore/CreateTable";
    await _http.PostAndParseAsync<bool>(url, param);
    items.MasterTableItems = await FetchItemsAsync("CSCTransfer", $"Exp{request.TableName}", null, true);
    var firstItem = items.MasterTableItems.FirstOrDefault();

    foreach (var key in firstItem.Keys)
    {
      items.LocalizedTitle[key] = _localizer.GetString(key);
    }

    return items;
  }

  public async Task<DataTransferRequestResponseViewModel> TransferDataAsync(SendDataToStoreItems request)
  {
    var relatedTables = await ParseRelatedTablesAsync(GetDatabaseName(request.TableName), request.TableName);
    var ret = new DataTransferRequestResponseViewModel
    {
      Dts = string.Equals(_policyService.GetPolicy(PolicyName.TRANS_DATA), "DTS", StringComparison.OrdinalIgnoreCase)
    };

    if (ret.Dts)
    {
      const string url = "SendDataToStore/TransferData";
      await _http.PostAndParseAsync<bool>(url, new DataTransferViewModel { RelatedTables = relatedTables });
    }
    else
    {
      ret.StoreOptions = await GetDataPathsAsync(request.SelectedStoreIDs);
      ret.TableData = await GetTransferDataAsync(relatedTables);
      await ArchiveExpTableDataAsync(request.TableName);
    }

    return ret;
  }

  public async Task<bool> UpdateFlagAsync(SendDataToStoreItems request)
  {
    var param = new
    {
      request.Flag,
      Values = request.SelectedItems,
      RelatedTables = await ParseRelatedTablesAsync(GetDatabaseName(request.TableName), request.TableName)
    };

    const string url = "SendDataToStore/UpdateFlag";
    return await _http.PostAndParseAsync<bool>(url, param);
  }

  private async Task<bool> ArchiveExpTableDataAsync(string tableName)
  {
    var masterTable = new DataTransferViewModel
    {
      RelatedTables = await ParseRelatedTablesAsync(GetDatabaseName(tableName), tableName)
    };

    const string url = "SendDataToStore/ArchiveExpTableData";
    return await _http.PostAndParseAsync<bool>(url, masterTable);
  }

  #region Private Methods

  private async Task<bool> CheckIfTableExistsAsync(string databaseName, string tableName)
  {
    databaseName = string.IsNullOrEmpty(databaseName) ? "CSCMaster" : databaseName;
    var url = $"SendDataToStore/CheckIfTableExists?databaseName={databaseName}&tableName={tableName}";
    return await _http.GetAndParseAsync<bool>(url);
  }

  private async Task<IEnumerable<string>> GetColumnNamesAsync(string databaseName, string tableName)
  {
    databaseName = string.IsNullOrEmpty(databaseName) ? "CSCMaster" : databaseName;
    var url = $"SendDataToStore/GetColumnNames?databaseName={databaseName}&tableName={tableName}";
    return await _http.GetAndParseAsync<IEnumerable<string>>(url);
  }

  private async Task<IEnumerable<DataTransferViewModel>> ParseRelatedTablesAsync(string databaseName, string tableName)
  {
    var masterTable = new DataTransferViewModel
    {
      DatabaseName = databaseName,
      TableName = tableName,
      PrimaryKeys = IniFileReader.ReadFromFile("KEY", tableName, _dataTransferConfigFilePath).Split(',').ToList(),
    };
    var relatedTableList = new List<DataTransferViewModel> { masterTable };

    if (tableName.Equals("PRICEL", StringComparison.OrdinalIgnoreCase))
    {
      return await ResolveOtherTableDependency(relatedTableList);
    }

    var sectionStarted = false;
    var lines = File.ReadAllLines(_dataTransferConfigFilePath);

    foreach (var line in lines)
    {
      if (line.StartsWith("[END2]"))
      {
        break;
      }

      if (!sectionStarted || !line.StartsWith(tableName, StringComparison.OrdinalIgnoreCase))
      {
        if (line.StartsWith("[RELATED TABLES]", StringComparison.OrdinalIgnoreCase))
        {
          sectionStarted = true;
        }

        continue;
      }

      if (!line.Contains('='))
      {
        throw new Exception(_localizer.GetString("missingEqualSign", tableName));
      }

      var relatedTables = line.Split('=')[1];

      if (relatedTables == "0")
      {
        return await ResolveOtherTableDependency(relatedTableList);
      }

      if (relatedTables.Length <= 1)
      {
        throw new Exception(_localizer.GetString("noChildTablesDefined", tableName));
      }

      relatedTableList.AddRange(await ProcessRelatedTablesAsync(databaseName, relatedTables));
      return await ResolveOtherTableDependency(relatedTableList);
    }

    throw new Exception(_localizer.GetString("wrongMasterTableName", tableName));
  }

  private async Task<IEnumerable<DataTransferViewModel>> ProcessRelatedTablesAsync(string databaseName, string line)
  {
    var ret = new List<DataTransferViewModel>();
    var maxAllowedRelatedTables = int.Parse(IniFileReader.ReadFromFile("CONFIG", "TABLE_NUM", _dataTransferConfigFilePath));
    var relatedTables = line.Split(';');

    if (maxAllowedRelatedTables < relatedTables.Length)
    {
      throw new Exception(_localizer.GetString("maxAllowedRelatedTablesExceeded", maxAllowedRelatedTables.ToString()));
    }

    foreach (var table in relatedTables)
    {
      if (!table.Contains(':'))
      {
        throw new Exception(_localizer.GetString("missingColon"));
      }

      var tableInfo = table.Split(':');

      if (!await CheckIfTableExistsAsync(databaseName, tableInfo[0]))
      {
        throw new Exception(_localizer.GetString("wrongTableName", tableInfo[0]));
      }

      var relatedTable = new DataTransferViewModel()
      {
        DatabaseName = databaseName,
        TableName = tableInfo[0],
        PrimaryKeys = tableInfo[1].Split(',').ToList(),
      };

      var invalidKey = await ValidateKeysAsync(databaseName, relatedTable.TableName, relatedTable.PrimaryKeys);

      if (!string.IsNullOrEmpty(invalidKey))
      {
        throw new Exception(_localizer.GetString("invalidKey", invalidKey, tableInfo[0]));
      }

      ret.Add(relatedTable);
    }

    return ret;
  }

  private async Task<IEnumerable<DataTransferViewModel>> ResolveOtherTableDependency(List<DataTransferViewModel> relatedTables)
  {
    var otherRelatedTables = new List<DataTransferViewModel>();
    var masterTableDatabaseName = relatedTables.FirstOrDefault()?.DatabaseName;
    var masterTablePrimaryKeys = relatedTables.FirstOrDefault()?.PrimaryKeys;

    foreach (var table in relatedTables)
    {
      if (table.TableName.Equals("PRICEL", StringComparison.OrdinalIgnoreCase))
      {
        otherRelatedTables.Add(new DataTransferViewModel { DatabaseName = masterTableDatabaseName, TableName = "Stock_Prices", PrimaryKeys = masterTablePrimaryKeys });
        otherRelatedTables.Add(new DataTransferViewModel { DatabaseName = masterTableDatabaseName, TableName = "StockMst", PrimaryKeys = masterTablePrimaryKeys });
      }

      if (table.TableName.Equals("TENDCARD", StringComparison.OrdinalIgnoreCase))
      {
        otherRelatedTables.Add(new DataTransferViewModel { DatabaseName = masterTableDatabaseName, TableName = "CARDS", PrimaryKeys = new List<string> { "CardCode" } });
        otherRelatedTables.Add(new DataTransferViewModel { DatabaseName = masterTableDatabaseName, TableName = "CARDCODES", PrimaryKeys = new List<string> { "CardCode" } });
      }

      if (table.TableName.Equals("CARDS", StringComparison.OrdinalIgnoreCase))
      {
        otherRelatedTables.Add(new DataTransferViewModel { DatabaseName = masterTableDatabaseName, TableName = "CARDCODES", PrimaryKeys = new List<string> { "CardCode" } });
      }

      if (table.TableName.Equals("USER", StringComparison.OrdinalIgnoreCase))
      {
        otherRelatedTables.Add(new DataTransferViewModel { DatabaseName = masterTableDatabaseName, TableName = "UIG", PrimaryKeys = new List<string> { "User" } });
      }
    }

    return relatedTables.Union(otherRelatedTables).Distinct();
  }

  private async Task ParsePrimaryKeysAsync(DataTransferViewModel masterTable)
  {
    masterTable.PrimaryKeys = new List<string>();
    var keys = IniFileReader.ReadFromFile("KEY", masterTable.TableName, _dataTransferConfigFilePath);

    if (string.IsNullOrEmpty(keys))
    {
      return;
    }

    masterTable.PrimaryKeys = keys.Split(',').ToList();
    var invalidKey = await ValidateKeysAsync(masterTable.DatabaseName, masterTable.TableName, masterTable.PrimaryKeys);

    if (!string.IsNullOrEmpty(invalidKey))
    {
      throw new Exception(_localizer.GetString("invalidKey", invalidKey, masterTable.TableName));
    }

    var minForeignKeyCount = int.MaxValue;

    foreach (var table in masterTable.RelatedTables)
    {
      minForeignKeyCount = Math.Min(minForeignKeyCount, table.PrimaryKeys.Count());
    }

    if (minForeignKeyCount < masterTable.PrimaryKeys.Count() - 1 && !masterTable.References.Any())
    {
      throw new Exception(_localizer.GetString("inadequateForeignKey", masterTable.TableName));
    }
  }

  private async Task ParseReferencesAsync(DataTransferViewModel masterTable)
  {
    masterTable.References = new List<string>();
    var references = IniFileReader.ReadFromFile("REFERENCES", masterTable.TableName, _dataTransferConfigFilePath);

    if (string.IsNullOrEmpty(references))
    {
      return;
    }

    masterTable.References = references.Split(',').ToList();
    var invalidKey = await ValidateKeysAsync(masterTable.DatabaseName, masterTable.TableName, masterTable.References);

    if (!string.IsNullOrEmpty(invalidKey))
    {
      throw new Exception(_localizer.GetString("invalidReferenceKey", invalidKey, masterTable.TableName));
    }
  }

  private async Task ParseDescriptionsAsync(DataTransferViewModel masterTable)
  {
    masterTable.Descriptions = new List<string>();
    var descriptions = IniFileReader.ReadFromFile("DESCRIPTION", masterTable.TableName, _dataTransferConfigFilePath);

    if (string.IsNullOrEmpty(descriptions))
    {
      return;
    }

    masterTable.Descriptions = descriptions.Split(',').ToList();
    var invalidKey = await ValidateKeysAsync(masterTable.DatabaseName, masterTable.TableName, masterTable.Descriptions);

    if (!string.IsNullOrEmpty(invalidKey))
    {
      throw new Exception(_localizer.GetString("invalidDescriptionKey", invalidKey, masterTable.TableName));
    }
  }

  private async Task<string> ValidateKeysAsync(string databaseName, string tableName, IEnumerable<string> keys)
  {
    var columnNames = await GetColumnNamesAsync(databaseName, tableName);

    foreach (var key in keys)
    {
      if (!columnNames.Any(column => column.Equals(key, StringComparison.OrdinalIgnoreCase)))
      {
        return key;
      }
    }

    return string.Empty;
  }

  private async Task<CheckCreateTableResultViewModel> CheckCreateTableAsync(DataTransferViewModel masterTable)
  {
    var relatedTables = masterTable.RelatedTables.Select(rt => rt.TableName);
    var param = new
    {
      masterTable.DatabaseName,
      MasterTableName = masterTable.TableName,
      RelatedTables = string.Join(',', relatedTables.Distinct()),
      Dts = string.Equals(_policyService.GetPolicy(PolicyName.TRANS_DATA), "DTS", StringComparison.OrdinalIgnoreCase)
    };

    const string url = "SendDataToStore/CheckCreateTable";
    return await _http.PostAndParseAsync<CheckCreateTableResultViewModel>(url, param);
  }

  private async Task<IEnumerable<IDictionary<string, object>>> FetchItemsAsync(string databaseName, string tableName, IEnumerable<string>? columns, bool convertToDsiplayText = false)
  {
    var primaryParam = new
    {
      DatabaseName = databaseName,
      TableName = tableName,
      PrimaryKeys = columns,
      Dts = string.Equals(_policyService.GetPolicy(PolicyName.TRANS_DATA), "DTS", StringComparison.OrdinalIgnoreCase)
    };

    const string url = "SendDataToStore/GetItems";
    var items = await _http.PostAndParseAsync<IEnumerable<IDictionary<string, object>>>(url, primaryParam);

    if(convertToDsiplayText)
    {
      formatDisplayText(items);
    }

    return items;
  }

  private async Task<IEnumerable<string>> GetSelectedStoreIDsAsync(string tableName)
  {
    var url = $"SendDataToStore/GetSelectedStoreIDs?tableName={tableName}";
    return await _http.GetAndParseAsync<IEnumerable<string>>(url);
  }

  private string GetDatabaseName(string tableName)
  {
    var databaseNmae = IniFileReader.ReadFromFile("CONNECTION", tableName, _dataTransferConfigFilePath);

    if (string.IsNullOrEmpty(databaseNmae))
    {
      databaseNmae = "CSCMaster";
    }

    return databaseNmae;
  }

  private async Task<bool> ClearTransferTableAsync(string tableName)
  {
    var dts = string.Equals(_policyService.GetPolicy(PolicyName.TRANS_DATA), "DTS", StringComparison.OrdinalIgnoreCase);
    var url = $"SendDataToStore/ClearTransferTable?tableName={tableName}&dts={dts}";
    return await _http.GetAndParseAsync<bool>(url);
  }

  private async Task<IEnumerable<StoreOptionViewModel>> GetDataPathsAsync(IEnumerable<string> storeIDs)
  {
    const string url = "SendDataToStore/GetDataPaths";
    var storeOptions = await _http.PostAndParseAsync<IEnumerable<StoreOptionViewModel>>(url, storeIDs);
    var invalidChars = Path.GetInvalidPathChars();

    foreach (var store in storeOptions)
    {
      if(!string.IsNullOrEmpty(store.DataPath) && store.DataPath.IndexOfAny(invalidChars) != -1)
      {
        store.DataPath = string.Empty;
      }
    }

    return storeOptions;
  }

  private async Task<IDictionary<string, object>> GetTransferDataAsync(IEnumerable<DataTransferViewModel> relatedTables)
  {
    var ret = new Dictionary<string, object>();
    var tasks = relatedTables.Select(async table =>
    {
      var tableName = $"Exp{table.TableName}";
      var data = await FetchItemsAsync("CSCTransfer", tableName, null);
      ret[tableName] = ConvertDataToString(data);
    });

    await Task.WhenAll(tasks);
    return ret;
  }

  private string ConvertDataToString(IEnumerable<IDictionary<string, object>> data)
  {
    var sb = new StringBuilder();

    foreach (var row in data)
    {
      var flagValue = row["Flag"];

      // Join all values into a single comma-separated line, excluding the Flag value
      var line = string.Join(",", row.Where(kvp => kvp.Key != "Flag").Select(kvp =>
      {
        var jElement = kvp.Value != null ? (JsonElement)kvp.Value : default;
        var elementType = kvp.Value != null ? jElement.ValueKind : JsonValueKind.Null;

        if (elementType == JsonValueKind.String)
        {
          if(jElement.TryGetDateTime(out DateTime dateTimeValue))
          {
            return $"'{dateTimeValue:M/d/yyyy h:mm:ss tt}'";
          }

          return $"'{kvp.Value}'";
        }
        else if(elementType == JsonValueKind.True || elementType == JsonValueKind.False)
        {
          return jElement.GetBoolean() ? "1" : "0";
        }
        else if(elementType == JsonValueKind.Number)
        {
          if (jElement.TryGetDecimal(out decimal numberValue))
          {
            return numberValue.ToString("G29");  // "G29" removes trailing zeros while keeping necessary precision
          }

          return jElement.GetRawText();
        }
        else
        {
          return kvp.Value ?? "''";
        }
      })); 

      // Prepend the Flag value to the line
      sb.AppendLine($"{flagValue},{line}");
    }

    return sb.ToString();
  }

  private void formatDisplayText(IEnumerable<IDictionary<string, object>> data)
  {
    foreach (var item in data)
    {
      var keys = item.Keys.ToList();

      foreach (var key in keys)
      {
        if (item[key] != null && item[key] is JsonElement jElement)
        {
          var elementType = jElement.ValueKind;

          if (elementType == JsonValueKind.String && jElement.TryGetDateTime(out DateTime dateTimeValue))
          {
            item[key] = $"{dateTimeValue:M/d/yyyy h:mm:ss tt}";
          }
          else if (elementType == JsonValueKind.True || elementType == JsonValueKind.False)
          {
            item[key] = jElement.GetBoolean() ? "True" : "False";
          }
          else if (elementType == JsonValueKind.Number)
          {
            if (jElement.TryGetDecimal(out decimal numberValue))
            {
              item[key] = numberValue.ToString("G29");
            }
          }
        }
      }
    }
  }
  #endregion
}
