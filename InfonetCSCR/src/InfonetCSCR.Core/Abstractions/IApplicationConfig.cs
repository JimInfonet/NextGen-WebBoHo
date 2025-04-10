namespace InfonetCSCR.Core.Abstractions;

public interface IApplicationConfig
{
  public bool IsBackOffice { get; }
  public string CSCAdminConnectionString { get; }
  public string CSCMasterConnectionString { get; }
  public string CSCTransConnectionString { get; }
  public string CSCTransferConnectionString { get; }
  public string CSCPumpConnectionString { get; }
  public string CSCTillsConnectionString { get; }
}
