namespace InfonetCSCR.Api;

public class ApplicationConfig : IApplicationConfig
{
  private readonly IConfiguration _config;
  public ApplicationConfig(IConfiguration config)
  {
    _config = config;
  }

  public bool IsBackOffice => Boolean.Parse(_config["BackOffice"] ?? "true");
  public string CSCAdminConnectionString => _config.GetConnectionString("CSCAdmin");
  public string CSCMasterConnectionString => _config.GetConnectionString("CSCMaster");
  public string CSCTransConnectionString => _config.GetConnectionString("CSCTrans");
  public string CSCTransferConnectionString => _config.GetConnectionString("CSCTransfer");
  public string CSCPumpConnectionString => _config.GetConnectionString("CSCPump");
  public string CSCTillsConnectionString => _config.GetConnectionString("CSCTills");
}
