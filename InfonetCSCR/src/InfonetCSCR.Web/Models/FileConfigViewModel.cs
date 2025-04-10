namespace InfonetCSCR.Web.Models;

public class FileConfigViewModel
{
  public bool IsAutoImportExport { get; set; }
  public string ImportFilePathForStockReceiving { get; set; } = default!;
  public string ImportFilePathForStockTakes { get; set; } = default!;

  public string ExportFolderPath { get; set; } = default!;
  public string TimesheetPayPeriodPYFileExportFolderPath { get; set; } = default!;
  public string POPaymentOutputFolderPath { get; set; } = default!;
  public string POPaymentArchiveFolderPath { get; set; } = default!;
  public string ImagesFolderPath { get; set; } = default!;
  public string BackOfficeFolderPath { get; set; } = default!;
  public string HeadOfficeFolderPath { get; set; } = default!;
  public string HeadOfficeLogFolderPath { get; set; } = default!;
  public string BackOfficeLogFolderPath { get; set; } = default!;
  public string DataTransferConfigFilePath { get; set; } = default!;
}
