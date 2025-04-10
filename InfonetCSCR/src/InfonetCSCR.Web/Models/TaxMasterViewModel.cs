namespace InfonetCSCR.Web.Models;

public class TaxMasterViewModel
{
  public string? Code { get; set; }
  public string? Name { get; set; }
  public double? Rate { get; set; }
  public bool? Active { get; set; }
  public short? Order { get; set; }
  public string? Description { get; set; }
  public string? Apply { get; set; }
  public char? Default { get; set; }
}
