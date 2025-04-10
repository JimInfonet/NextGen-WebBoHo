namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class FileController : Controller
{
  private readonly IApplicationConfig _appConfig;
  private readonly ILogger<FileController> _logger;
  private readonly Serilog.ILogger _seriLogger;

  public FileController(IApplicationConfig appConfig, ILogger<FileController> logger)
  {
    _appConfig = appConfig;
    _logger = logger;
    _seriLogger = Log.Logger;
  }

  [HttpPost]
  public IActionResult DeleteImportedFile(string filePath)
  {
    try
    {
      // Check if the file path is valid and exists.
      if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
      {
        // Delete the file.
        System.IO.File.Delete(filePath);

        // Optionally, you can perform additional actions or return a success message.
        return Json("File deleted successfully.");
      }

      // File does not exist or invalid file path.
      return NotFound("File not found.");
    }
    catch (Exception ex)
    {
      _seriLogger.Error(ex.Message, ex);
      // Handle any exceptions that might occur during the file deletion process.
      return StatusCode(500, $"An error occurred while deleting the file: {ex.Message}");
    }
  }

  [HttpGet]
  public async Task<IActionResult> GetImportFileContent(int purpose)
  {
    if (_appConfig.FileConfig.IsAutoImportExport)
    {
      string fileContents;
      string filePath = purpose == (int)StockCountType.StockReceiving ?
                                   _appConfig.FileConfig.ImportFilePathForStockReceiving
                                   : _appConfig.FileConfig.ImportFilePathForStockTakes;

      if (!System.IO.File.Exists(filePath))
      {
        return Json("FileNotFound");
      }

      try
      {
        // Read the entire content of the file into a string
        fileContents = await System.IO.File.ReadAllTextAsync(filePath);
      }
      catch (Exception ex)
      {
        _seriLogger.Error(ex.Message, ex);
        return StatusCode(500, $"An error occurred while importing the file: {ex.Message}");
      }
      return Json(fileContents);
    }
    else
    {
      _seriLogger.Error("File Auto Import Export is disabled.");
      return StatusCode((int)HttpStatusCode.BadRequest);
    }
  }

  [HttpPost]
  public async Task<IActionResult> SaveFile([FromBody] string content, string fileName, FileType? fileType = null, bool forceAutoImportExport = false)
  {
    if (_appConfig.FileConfig.IsAutoImportExport || forceAutoImportExport)
    {
      try
      {
        string folderPath = "";
        if (fileType == FileType.TimesheetPayPeriodPYFile) folderPath = _appConfig.FileConfig.TimesheetPayPeriodPYFileExportFolderPath;
        else if(fileType == FileType.FleetCardSaleReport || fileType == FileType.CustomerSalesDetailReport)
        {
          folderPath = Path.Combine(
            _appConfig.IsBackOffice ? _appConfig.FileConfig.BackOfficeFolderPath : _appConfig.FileConfig.HeadOfficeFolderPath,
            fileType == FileType.FleetCardSaleReport ? "SavedReports" : "OUT"
          );
        }
        else folderPath = _appConfig.FileConfig.ExportFolderPath;
        string filePath = folderPath + "\\" + fileName;

        //Create Folder Path if doesn't exist
        if (folderPath != null && !Directory.Exists(folderPath))
        {
          Directory.CreateDirectory(folderPath);
        }

        // Create File Path if doesn't exist
        if (!System.IO.File.Exists(filePath))
        {
          FileStream fs = System.IO.File.Create(filePath);
          // Dispose FileStream
          fs.Close();
        }

        if (fileType == FileType.FleetCardSaleReport || fileType == FileType.CustomerSalesDetailReport)
        {
          // Decode the Base64 string to byte array
          byte[] fileBytes = Convert.FromBase64String(content);

          // Write the bytes to the file
          await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);
        }
        else
        {
          // Write to file. It will replace file if exists or create otherwise
          await System.IO.File.WriteAllTextAsync(filePath, content);
        }
      }
      catch (Exception ex)
      {
        _seriLogger.Error(ex.Message, ex);
        return StatusCode(500, $"An error occurred while exporting the file: {ex.Message}");
      }

      return Json(true);
    }
    else
    {
      _seriLogger.Error("File Auto Import Export is disabled.");
      return StatusCode((int)HttpStatusCode.BadRequest);
    }
  }

  [HttpPost]
  public async Task<IActionResult> SaveLogWithBackup([FromBody] LogViewModel logRequest)
  {
    if (_appConfig.FileConfig.IsAutoImportExport)
    {
      try
      {
        var logFilePath = Path.Combine(logRequest.LogFolderPath, $"{logRequest.LogFileName}.log");
        var backupFolderPath = Path.Combine(logRequest.LogFolderPath, "Backup");
        var backupFilePath = Path.Combine(backupFolderPath, $"{logRequest.LogFileName}.{DateTime.Now:MMddyyyy_HHmmss}.log");

        Directory.CreateDirectory(logRequest.LogFolderPath);
        Directory.CreateDirectory(backupFolderPath);

        if (System.IO.File.Exists(logFilePath))
        {
          var fileInfo = new FileInfo(logFilePath);

          if (fileInfo.Length > 100000)
          {
            System.IO.File.Copy(logFilePath, backupFilePath, true);
            System.IO.File.Delete(logFilePath);
          }
        }

        var sb = new StringBuilder();
        sb.AppendLine(DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt"));
        sb.AppendLine(logRequest.LogText);
        await System.IO.File.AppendAllTextAsync(logFilePath, sb.ToString());
      }
      catch (Exception ex)
      {
        _seriLogger.Error(ex.Message, ex);
        return StatusCode(500, $"An error occurred while exporting the file: {ex.Message}");
      }

      return Json(true);
    }
    else
    {
      _seriLogger.Error("File Auto Import Export is disabled.");
      return StatusCode(400, "File Auto Import Export is disabled");
    }
  }

  [HttpPost]
  public async Task<IActionResult> SaveFilePO([FromBody] POFileViewModel poFile)
  {
    if (_appConfig.FileConfig.IsAutoImportExport)
    {
      try
      {
        string folderPath = _appConfig.FileConfig.POPaymentOutputFolderPath;
        string headerFilePath = folderPath + "\\" + poFile.HeaderFileName;
        string detailFilePath = folderPath + "\\" + poFile.DetailFileName;

        //Create Folder Path if doesn't exist
        if (folderPath != null && !Directory.Exists(folderPath))
        {
          Directory.CreateDirectory(folderPath);
        }

        // Create File Path if doesn't exist
        if (!System.IO.File.Exists(headerFilePath))
        {
          FileStream fs = System.IO.File.Create(headerFilePath);
          // Dispose FileStream
          fs.Close();
        }

        // Write to file. It will replace file if exists or create otherwise
        await System.IO.File.WriteAllTextAsync(headerFilePath, poFile.HeaderFileContent);

        if (!string.IsNullOrEmpty(poFile.DetailFileContent))
        {
          // Create File Path if doesn't exist
          if (!System.IO.File.Exists(detailFilePath))
          {
            FileStream fs = System.IO.File.Create(detailFilePath);
            fs.Close();
          }

          // Write to file. It will replace file if exists or create otherwise
          await System.IO.File.WriteAllTextAsync(detailFilePath, poFile.DetailFileContent);
        }
      }
      catch (Exception ex)
      {
        _seriLogger.Error(ex.Message, ex);
        return StatusCode(500, $"An error occurred while exporting the file: {ex.Message}");
      }
      return Json(true);
    }
    else
    {
      _seriLogger.Error("File Auto Import Export is disabled.");
      return StatusCode((int)HttpStatusCode.BadRequest);
    }
  }

  [HttpPost]
  public async Task<bool> ExeceuteBatchFile([FromBody] string batchFilePath)
  {
    Process process = new();
    process.StartInfo.FileName = "cmd.exe";
    process.StartInfo.Arguments = "/c " + batchFilePath; // /c is used to run the command and then terminate
    process.StartInfo.UseShellExecute = false; // Set to false to redirect input and output
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;

    process.Start();

    await process.StandardOutput.ReadToEndAsync();
    var error = await process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    if (string.IsNullOrWhiteSpace(error)) return true;
    else
    {
      _logger.LogError(error.ToString());
    }

    return false;
  }
}
