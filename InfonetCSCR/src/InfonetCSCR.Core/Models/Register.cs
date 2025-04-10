namespace InfonetCSCR.Core.Models;

public class Register
{
  public int RegNo { get; set; } = default!;
  public int DeviceID { get; set; } = default!;
  public bool? Active { get; set; }
  public bool? UseOPOS { get; set; }
  public string? DeviceName { get; set; }
  public int? PortNum { get; set; }
  public string? PortSetting { get; set; }
  public string? DriverName { get; set; }
  public string? FontName { get; set; }
  public int? FontSize { get; set; }
  public bool? WritePosLog { get; set; }
}
