namespace InfonetCSCR.Web.Abstractions;

public interface IApplicationConfig
{
  public FileConfigViewModel FileConfig { get; }
  public ImageNameConfigViewModel ImageNameConfig { get; }
  public string SenderName { get; }
  public string SenderEmailAddress { get; }
  public string SenderAppPassword { get; }
  public string SmtpHost { get; }
  public int SmtpPort { get; }
  public string SendToEmailAddr { get; }
  public bool UseHttps { get; }
  public string BatchFilePath { get; }
  public string Language { get; }
  public string ApiBaseUrl { get; }
  public string ReportBaseUrl { get; }
  public string WebBaseUrl { get; }
  public int CookieExpiryTimeInMinute { get; }
  public bool IsBackOffice { get; }
  public bool IsServerSidePagination { get; }
  public int AcceptedDateRange { get; }
}
