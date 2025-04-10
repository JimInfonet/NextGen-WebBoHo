namespace InfonetCSCR.Core.Models;

public class Vendor
{
  public string Code { get; set; } = default!;
  public string? Address { get; set; }
  public string? City { get; set; }
  public string? Province { get; set; }
  public string? Postal { get; set; }
  public string? Phone { get; set; }
  public string? Fax { get; set; }
  public string? CustAccount { get; set; }
  public string Name { get; set; } = default!;
  public string? Email { get; set; }
  public string? Country { get; set; }
  public double? PoDiscount { get; set; }
  public bool? PoCostTaxInclude { get; set; }
  public string? ContactName1 { get; set; }
  public string? ContactName2 { get; set; }
  public string? TaxNumber { get; set; }
  public bool? IsTEVendor { get; set; } = false;
  public bool? Active { get; set; }
  public float? OrderFreq { get; set; }
}
