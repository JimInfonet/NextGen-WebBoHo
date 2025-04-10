namespace InfonetCSCR.Api.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class SendDataToStoreController : Controller
{
  private readonly ISendDataToStoreService _sendDataToStoreService;

  public SendDataToStoreController(ISendDataToStoreService sendDataToStoreService)
  {
    _sendDataToStoreService = sendDataToStoreService;
  }

  // api/SendDataToStore/CheckIfTableExists
  [HttpGet]
  public async Task<IActionResult> CheckIfTableExists(string databaseName, string tableName)
  {
    return Ok(await _sendDataToStoreService.CheckIfTableExistsAsync(databaseName, tableName));
  }

  // api/SendDataToStore/GetColumnNames
  [HttpGet]
  public async Task<IActionResult> GetColumnNames(string databaseName, string tableName)
  {
    return Ok(await _sendDataToStoreService.GetColumnNamesAsync(databaseName, tableName));
  }

  // api/SendDataToStore/CheckCreateTable
  [HttpPost]
  public async Task<IActionResult> CheckCreateTable(CheckCreateTable param)
  {
    return Ok(await _sendDataToStoreService.CheckCreateTableAsync(param));
  }

  // api/SendDataToStore/GetItems
  [HttpPost]
  public async Task<IActionResult> GetItems(TableDto table)
  {
    return Ok(await _sendDataToStoreService.GetItemsAsync(table));
  }

  // api/SendDataToStore/GetSelectedStoreIDs
  [HttpGet]
  public async Task<IActionResult> GetSelectedStoreIDs(string tableName)
  {
    return Ok(await _sendDataToStoreService.GetSelectedStoreIDsAsync(tableName));
  }

  // api/SendDataToStore/RemoveItemFromTransferDatabase
  [HttpPost]
  public async Task<IActionResult> RemoveItemFromTransferDatabase(TableDto table)
  {
    return Ok(await _sendDataToStoreService.RemoveItemFromTransferDatabaseAsync(table));
  }

  // api/SendDataToStore/RemoveSelectedStoresFromTransferDatabase
  [HttpPost]
  public async Task<IActionResult> RemoveSelectedStoresFromTransferDatabase(IEnumerable<string> tableNames)
  {
    return Ok(await _sendDataToStoreService.RemoveSelectedStoresFromTransferDatabaseAsync(tableNames));
  }

  // api/SendDataToStore/ClearTransferTable
  [HttpGet]
  public async Task<IActionResult> ClearTransferTable(string tableName, bool dts)
  {
    return Ok(await _sendDataToStoreService.ClearTransferTableAsync(tableName, dts));
  }

  // api/SendDataToStore/CreateTable
  [HttpPost]
  public async Task<IActionResult> CreateTable(TableDto table)
  {
    return Ok(await _sendDataToStoreService.CreateTableAsync(table));
  }

  // api/SendDataToStore/GetDataPaths
  [HttpPost]
  public async Task<IActionResult> GetDataPaths(IEnumerable<string> storeIDs)
  {
    return Ok(await _sendDataToStoreService.GetDataPathsAsync(storeIDs));
  }

  // api/SendDataToStore/TransferData
  [HttpPost]
  public async Task<IActionResult> TransferData(TableDto table)
  {
    return Ok(await _sendDataToStoreService.TransferDataAsync(table));
  }

  // api/SendDataToStore/ArchiveExpTableData
  [HttpPost]
  public async Task<IActionResult> ArchiveExpTableData(TableDto table)
  {
    return Ok(await _sendDataToStoreService.ArchiveExpTableDataAsync(table));
  }

  // api/SendDataToStore/DeleteExpTableData
  [HttpPost]
  public async Task<IActionResult> DeleteExpTableData([FromBody] string[] tableNames)
  {
    return Ok(await _sendDataToStoreService.DeleteExpTableDataAsync(tableNames));
  }

  // api/SendDataToStore/UpdateFlag
  [HttpPost]
  public async Task<IActionResult> UpdateFlag(TableDto table)
  {
    return Ok(await _sendDataToStoreService.UpdateFlagAsync(table));
  }
}
