namespace InfonetCSCR.Web;

public class ApplicationConfig : IApplicationConfig
{
  private readonly IConfiguration _config;
  public ApplicationConfig(IConfiguration config)
  {
    _config = config;
  }

  public FileConfigViewModel FileConfig => new FileConfigViewModel
  {
    IsAutoImportExport = bool.Parse(_config["FileConfig:IsAutoImportExport"]),
    ImportFilePathForStockReceiving = _config["FileConfig:ImportFilePathForStockReceiving"],
    ImportFilePathForStockTakes = _config["FileConfig:ImportFilePathForStockTakes"],
    ExportFolderPath = _config["FileConfig:ExportFolderPath"],
    TimesheetPayPeriodPYFileExportFolderPath = _config["FileConfig:TimesheetPayPeriodPYFileExportFolderPath"],
    POPaymentOutputFolderPath = _config["FileConfig:POPaymentOutputFolderPath"],
    POPaymentArchiveFolderPath = _config["FileConfig:POPaymentArchiveFolderPath"],
    ImagesFolderPath = _config["FileConfig:ImagesFolderPath"],
    BackOfficeFolderPath = _config["FileConfig:BackOfficeFolderPath"],
    HeadOfficeFolderPath = _config["FileConfig:HeadOfficeFolderPath"],
    HeadOfficeLogFolderPath = _config["FileConfig:HeadOfficeLogFolderPath"],
    BackOfficeLogFolderPath = _config["FileConfig:BackOfficeLogFolderPath"],
    DataTransferConfigFilePath = _config["FileConfig:DataTransferConfigFilePath"]
  };

  public ImageNameConfigViewModel ImageNameConfig => new ImageNameConfigViewModel
  {
    YellowButtonUpImageName = _config["ImageNameConfig:YellowButtonUpImageName"],
    YellowButtonDownImageName = _config["ImageNameConfig:YellowButtonDownImageName"],
    GreenButtonUpImageName = _config["ImageNameConfig:GreenButtonUpImageName"]
  };

  public string SenderName => _config["EmailConfig:SenderName"];
  public string SenderEmailAddress => _config["EmailConfig:SenderEmailAddress"];
  public string SenderAppPassword => _config["EmailConfig:SenderAppPassword"];
  public string SmtpHost => _config["EmailConfig:SmtpHost"];
  public int SmtpPort => int.Parse(_config["EmailConfig:SmtpPort"]);
  public string SendToEmailAddr => _config["EmailConfig:SendToEmailAddr"];
  public bool UseHttps => bool.Parse(_config["UseHttps"] ?? "true");
  public string BatchFilePath => _config["FileConfig:BatchFilePath"];
  public string Language => _config["Language"] ?? "en";
  public string ApiBaseUrl => _config["ApiBaseUrl"];
  public string ReportBaseUrl => _config["ReportBaseUrl"];
  public string WebBaseUrl => _config["WebBaseUrl"];
  public int CookieExpiryTimeInMinute => int.Parse(_config["CookieExpiryTimeInMinute"]);
  public bool IsBackOffice => bool.Parse(_config["BackOffice"] ?? "true");
  public bool IsServerSidePagination => bool.Parse(_config["ServerSidePagination"] ?? "true");
  public int AcceptedDateRange => int.Parse(_config["ElectronicJournalReportRange"]);
}
